using Basics.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StandardShieldController : MonoBehaviour
{
    [SerializeField] ShieldKnightController sk;
    TargetSelector ts;

    public LayerMask ignoreLayer;

    [SerializeField] Rigidbody shieldRB;
    [SerializeField] GameObject marker;
    GameObject hitStars;

    [Header("Throw")]
    public float throwForce;
    public GameObject target;
    TrailRenderer trail;
    public bool thrown, canThrow, hasTarget;

    [Header("Recall")]
    Transform shieldHoldPos;
    Quaternion shieldHoldRot;
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

    [Header("Barge")]
    [SerializeField] float bargeDelay;
    public float bargeTime, bargeSpeed, bargeStartSpeed;
    public GameObject closest;
    public bool canBarge, isBarging;

    [Header("Slam")]
    public float slamForce, slamDelay, slamPushBack, slamRadius, slamLift, damageDelay;
    GameObject slamStars;
    Transform squashedObject;
    [SerializeField] List<Transform> slamObjects = new List<Transform>();
    bool showSlamVFX;

    Vector3 startScale;

    private void Awake()
    {
        sk = FindObjectOfType<ShieldKnightController>();
        ts = FindObjectOfType<TargetSelector>();

        shieldRB = GetComponentInChildren<Rigidbody>();

        trail = GetComponent<TrailRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Throw
        thrown = false;
        target = null;

        //Recall
        shieldHoldPos = transform.parent.transform;
        shieldHoldRot = Quaternion.Euler(-90, 90, 0);

        //Barge
        bargeStartSpeed = bargeSpeed;
        canBarge = true;
        isBarging = false;
        closest = null;

        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = startScale;

        if (target != null)
        {
            hasTarget = true;
        }
        else hasTarget = false;

        if (bargeDelay > 0)
        {
            bargeDelay -= Time.deltaTime;
        }
        if (bargeDelay <= 0)
        {
            bargeDelay = 0;

            if (!canBarge)
            {
                canBarge = true;
            }

            if(sk.isBarging)
            {
                sk.isBarging = false;
            }  
        }

        if (slamDelay > 0)
        {
            slamDelay -= Time.deltaTime;
        }
        if (slamDelay <= 0)
        {
            slamDelay = 0;

            if (sk.cc.isGrounded && sk.isSlamming)
            {
                sk.isSlamming = false;
            }
        }

        if (canThrow)
        {
            if (hasTarget)
            {
                sk.transform.LookAt(target.transform);
                StartCoroutine(TargetedThrow());

                /*if (!ts.lockedOn) //Will throw to multiple targets if not locked on, otherwise only one target.
                {
                    StartCoroutine(TargetedThrow());
                }
                else StartCoroutine(LockOnThrow());*/
            }
            else NonTargetThrow();
        }

        if (thrown)  //Stops Player repeatedly throwing the shield.
        {
            trail.enabled = true;
            canThrow = false;
        }
        else trail.enabled = false;

        if ((Input.GetButton("Throw") || Input.GetButtonDown("Barge")) && !thrown)
        {
            ts.FindTargets();
            ts.FindClosestTarget();
        }

        if (Input.GetButtonDown("Throw") && thrown) //If Player doesn't have possession of Shield it gets recalled to player.
        {
            StartCoroutine(RecallShield());
        }

        if (Input.GetButtonDown("Barge")) //Input to perform Barge/Dodge action.
        {
            if (!thrown)
            {
                if (canBarge && bargeDelay <= 0)
                {
                    StartCoroutine(Barge());
                    trail.enabled = true;
                }
            }
        }

        if (sk.isSlamming)
        {
            sk.velocity.y = -slamForce;

            RaycastHit hit;

            if (Physics.Raycast(transform.position, -sk.transform.up, out hit, Mathf.Infinity, ignoreLayer))
            {
                Debug.Log(hit.transform.name);
                float distToGround = hit.distance;

                Debug.DrawLine(sk.transform.position, -sk.transform.up * 10, Color.red);
                //Debug.Log("Distance to Ground: " + distToGround);

                if (distToGround < 3)
                {
                    Debug.DrawLine(sk.transform.position, -sk.transform.up * 10, Color.green);

                    if (!showSlamVFX)
                    {
                        slamStars = ObjectPoolManager.instance.CallObject("SlamStars", null, hit.point, Quaternion.Euler(-90, transform.rotation.y, transform.rotation.z), 1);
                        showSlamVFX = true;
                    }

                    Debug.Log("Hit Ground");
                    SlamImpact();

                    if (slamDelay <= 0)
                    {
                        slamDelay = 0.5f;
                    }
                }
            }
        }
        else
        {
            showSlamVFX = false;

            if (slamObjects.Count > 0)
            {
                foreach (Transform so in slamObjects)
                {
                    so.transform.localScale = Vector3.one;
                }

                slamObjects.Clear();
            }
        }

        if (sk.cc.isGrounded && sk.buttonHeld)
        {
            if(!thrown && !sk.isSlamming)
            {
                sk.isGuarding = true;
            }
        }
        else sk.isGuarding = false;

        if(sk.stopTime > 0 && !sk.isThrowing)
        {
            sk.isParrying = true;
        }

        if (hasTarget)
        {
            if (!marker)
            {
                marker = ObjectPoolManager.instance.CallObject("TargetMarker", null, Vector3.zero, Quaternion.identity);
            }
            else if (marker != null)
            {
                marker.transform.parent = target.transform;
                marker.transform.position = new Vector3(target.transform.position.x, target.transform.position.y + 4.5f, target.transform.position.z - 0.5f);
            }
        }
    }

    void NonTargetThrow()  //Throws Shield in players forward vector if no targets are identified.
    {
        thrown = true;

        shieldRB.isKinematic = false;
        shieldRB.AddForce(sk.transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;
    }

    IEnumerator TargetedThrow()  //Throws Shield towards any identified targets in range.
    {
        thrown = true;

        while (Vector3.Distance(target.transform.position, transform.position) > 0.1f)
        {
            transform.parent = null;

            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, throwForce * Time.deltaTime);

            yield return null;
        }

        if(Vector3.Distance(target.transform.position, transform.position) < 0.1f)
        {
            hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, target.transform.position, Quaternion.identity, 1);

            if (marker != null) 
            {
                ObjectPoolManager.instance.RecallObject(marker);
                marker = null;
            }
        }

        target = null;  //Once all targets are reached return Shield to Player.
        StartCoroutine(RecallShield());
    }

    IEnumerator LockOnThrow()
    {
        thrown = true;

        while (Vector3.Distance(transform.position, target.transform.position) > 0.1f)
        {
            transform.parent = null;

            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, throwForce * Time.deltaTime);

            yield return null;
        }

        if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
        {
            hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, target.transform.position, Quaternion.identity, 1);
        }

        if (target.GetComponent<EnemyHealth>() != null)
        {
            EnemyHealth enemy = target.GetComponent<EnemyHealth>();

            enemy.TakeDamage(10);
        }

        StartCoroutine(RecallShield());
    }

    IEnumerator RecallShield()  //Recalls Shield back to Shield Holder.
    {
        float t = 0f;
        while (t < lerpTime) //Returns Shield to Shield Holder over the course of 1 second.
        {
            shieldRB.isKinematic = false;

            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, shieldHoldPos.position, t / lerpTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, shieldHoldPos.rotation, t / lerpTime);
            
            meshCol.enabled = false; //Prevents from unecessary collisions upon return.

            yield return null;
        }

        shieldRB.isKinematic = true;

        transform.parent = shieldHoldPos;  //Sets Shields position and parent to stay attached to Player.
        transform.localPosition = Vector3.zero;
        transform.localRotation = shieldHoldRot;

        meshCol.enabled = true;

        thrown = false;
    }

    IEnumerator Barge()
    {
        float startTime = Time.time;

        new WaitForSeconds(1); //Prevents player from stacking Barges.

        while (Time.time < startTime + bargeTime)  //Player movement speed is disabled then moved by bargeSpeed over bargeTime;
        {
            sk.isBarging = true;
            sk.speed = 0;

            canBarge = false;

            if (hasTarget)
            {
                sk.transform.LookAt(target.transform.position); 

                Vector3 closestDir = (target.transform.position - transform.position).normalized;
                sk.cc.Move(closestDir * bargeSpeed * Time.deltaTime);

                if(Vector3.Distance(target.transform.position, transform.position) < 5)
                {
                    Debug.Log("Barge Distance" + Vector3.Distance(target.transform.position, transform.position));

                    bargeSpeed = 0;

                    if (marker != null)
                    {
                        ObjectPoolManager.instance.RecallObject(marker);
                        marker = null;
                    }

                    target = null;
                    ts.ClearTargets();  
                }
            }
            else 
            sk.cc.Move(sk.moveDir * bargeSpeed * Time.deltaTime);

            bargeDelay = 0.25f;
            bargeSpeed = bargeStartSpeed;

            yield return null;
        }
    }

    void SlamImpact()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, slamRadius);

        foreach (Collider col in colliders)
        {
            if (col.gameObject.layer == 15)
            {
                if(!slamObjects.Contains(col.transform))
                {
                    slamObjects.Add(col.transform.GetChild(0));
                }

                Vector3 squashedSize = new Vector3(col.transform.GetChild(0).localScale.x, 0.25f, col.transform.localScale.z);
                col.transform.GetChild(0).localScale = squashedSize;

                if (col.GetComponent<Rigidbody>() != null)
                {
                    Rigidbody slamRB = col.GetComponent<Rigidbody>();
                    slamRB.AddExplosionForce(slamPushBack, transform.position, slamRadius, slamLift, ForceMode.Impulse);
                }

                /*EnemyHealth enemy = col.GetComponent<EnemyHealth>();

                if (enemy != null)
                {
                    if (damageDelay <= 0)
                    {
                        enemy.TakeDamage(10);
                    }
                }*/
            }
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (thrown)
        {
            if (col.gameObject.tag == "Sticky" || col.gameObject.tag == "Target") //Makes rigidbody kinematic so Shield sticks to object.
            {
                shieldRB.isKinematic = true;
                transform.rotation = Quaternion.Euler(-90, 0, 0);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}
