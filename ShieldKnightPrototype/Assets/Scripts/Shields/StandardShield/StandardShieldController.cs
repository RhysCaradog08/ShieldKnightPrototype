using Basics.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StandardShieldController : MonoBehaviour
{
    [SerializeField] ShieldKnightController sk;
    AnimationController animControl;
    TargetSelector ts;
    [SerializeField] Rigidbody shieldRB;

    [SerializeField] GameObject marker;

    [Header("Throw")]
    public float throwForce;
    public GameObject target;
    TrailRenderer trail;
    GameObject hitStars;
    public bool thrown, canThrow, hasTarget;

    [Header("Recall")]
    Transform shieldHoldPos;
    Quaternion shieldHoldRot;
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

    [Header("Barge")]
    [SerializeField] float bargeDelay;
    public float bargeTime, bargeSpeed;
    public GameObject closest;
    public bool canBarge, isBarging;

    [Header("Slam")]
    public float slamForce, slamDelay, slamPushBack, slamRadius, slamLift, damageDelay;
    GameObject slamStars;
    bool showSlamVFX;

    Vector3 shieldScale;

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
        canBarge = true;
        isBarging = false;
        closest = null;

        shieldScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = shieldScale;

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

        if(Input.GetButton("Throw") && !thrown)
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

            if (Physics.Raycast(transform.position, -sk.transform.up, out hit))
            {
                //Debug.DrawLine(pc.transform.position, -pc.transform.up * 10, Color.green);

                float distToGround = hit.distance;

                //Debug.Log("Distance to Ground: " + distToGround);

                if (distToGround < 3)
                {
                    Debug.Log("Hit Ground");
                    /*SlamImpact();

                    if (!showSlamVFX)
                    {
                        slamStars = ObjectPoolManager.instance.CallObject("SlamStars", null, transform.position, Quaternion.Euler(-90, transform.rotation.y, transform.rotation.z), 1);
                        showSlamVFX = true;
                    }*/
                    if (slamDelay <= 0)
                    {
                        slamDelay = 5;
                    }
                }
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

        if (hasTarget)
        {
            if (!marker)
            {
                marker = ObjectPoolManager.instance.CallObject("TargetMarker", null, Vector3.zero, Quaternion.identity);
            }
            else if(marker != null)
            {
                marker.transform.parent = target.transform;
                marker.transform.position = new Vector3(target.transform.position.x, target.transform.position.y + 4.5f, target.transform.position.z -0.5f);
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
            if(marker != null) 
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
            //trailEffect.SetActive(true);
            sk.speed = 0;

            canBarge = false;

            /*if (ts.lockedOn)
            {
                Vector3 closestDir = (ts.closest.transform.position - transform.position).normalized;

                cc.Move(closestDir * bargeSpeed * Time.deltaTime);
            }
            else*/ 
            sk.cc.Move(sk.moveDir * bargeSpeed * Time.deltaTime);

            bargeDelay = 0.25f;

            yield return null;
        }

        /*if (marker != null)
        {
            ObjectPoolManager.instance.RecallObject(marker);
        }*/
    }

    private void OnCollisionEnter(Collision col)
    {
        if (thrown)
        {
            shieldRB.isKinematic = true;
            transform.rotation = Quaternion.Euler(-90, 0, 0);
            Debug.Log("Hit " + col.collider.name);

            if (col.gameObject.CompareTag("Sticky"))  //Makes rigidbody kinematic so Shield sticks to object.
            {
                shieldRB.isKinematic = true;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}
