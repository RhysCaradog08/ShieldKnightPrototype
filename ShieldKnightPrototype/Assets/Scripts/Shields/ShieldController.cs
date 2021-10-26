using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ShieldController : MonoBehaviour
{
    TargetingSystem ts;

    Rigidbody shieldRB;
    Transform shieldHoldPos;
    PlayerController player;

    [Header("Throw")]
    public float throwForce;
    public GameObject target;
    public GameObject model;
    public float rotateSpeed;
    TrailRenderer trail;
    GameObject hitStars;
    public bool thrown = false;
    public bool hasTarget;
    public bool canThrow;


    [Header("Recall")]
    public Transform curvePoint;
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

    [Header("Barge")]
    public bool isBarging;

    [Header("Slam")]
    [SerializeField] float slamForce;
    [SerializeField] float slamRadius;
    [SerializeField] float slamLift;
    [SerializeField]float damageDelay = 0.5f;
    GameObject slamStars;
    public bool isSlamming;
    bool showSlamVFX = false;

    [Header("UI")]
    private GameObject targetMarker;

    // Start is called before the first frame update
    void Awake()
    {
        shieldRB = GetComponent<Rigidbody>();
        shieldHoldPos = transform.parent.transform;

        player = FindObjectOfType<PlayerController>();

        ts = transform.root.GetComponent<TargetingSystem>();
        trail = GetComponent<TrailRenderer>();

        meshCol = GetComponentInChildren<MeshCollider>();
    }

    private void Update()
    {
        if (canThrow)  //Perform Throw action if Player has possession of Shield. 
        {
            if (hasTarget)
            {
                if (!ts.lockedOn) //Will throw to multiple targets if not locked on, otherwise only one target.
                {
                    StartCoroutine(TargetedThrow());
                }
                else StartCoroutine(LockOnThrow());
            }
            else NonTargetThrow();
        }

        if (Input.GetButtonDown("Fire1") && thrown) //If Player doesn't have possession of Shield it gets recalled to player.
        {
            if (!hasTarget)
            {
                StartCoroutine(RecallShield());
            }
        }

        if (thrown)  //Stops Player repeatedly throwing the shield.
        {
            trail.enabled = true;
            canThrow = false;
        }
        else trail.enabled = false;

        if (isSlamming)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, -transform.up * 10, out hit))
            {
                //Debug.DrawLine(transform.position, transform.right, Color.red);
                float distToGround = hit.distance;

                if (distToGround < 1)
                {
                    Debug.DrawLine(transform.position, -transform.up * 10, Color.green);

                    Slam();

                    if (!showSlamVFX)
                    {
                        slamStars = ObjectPoolManager.instance.CallObject("SlamStars", null, transform.position, Quaternion.Euler(-90, transform.rotation.y, transform.rotation.z), 1);
                        showSlamVFX = true;
                    }
                }
            }
        }
        else
        {
            showSlamVFX = false;
        }
    }

    void NonTargetThrow()  //Throws Shield in players forward vector if no targets are identified.
    {
        thrown = true;

        shieldRB.isKinematic = false;
        shieldRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;
    }

    IEnumerator TargetedThrow()  //Throws Shield towards any identified targets in range.
    {
        thrown = true;

        foreach (GameObject nextTarget in ts.visibleTargets) //Sets nextTarget in list to be target and move shield towards target.
        {
            target = nextTarget;
            Vector3 nextTargetPos = nextTarget.transform.position;
            while (Vector3.Distance(transform.position, nextTargetPos) > 0.1f)
            {
                transform.parent = null;

                transform.position = Vector3.MoveTowards(transform.position, nextTargetPos, throwForce * Time.deltaTime);

                yield return null;
            }

            if (Vector3.Distance(transform.position, nextTargetPos) < 0.1f)
            {
                hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, nextTargetPos, Quaternion.identity, 1);
            }
            
            if (nextTarget.GetComponent<MarkerCheck>() != null)
            {
                MarkerCheck markerCheck = nextTarget.GetComponent<MarkerCheck>();

                markerCheck.RemoveMarker();
            }

            if (nextTarget.GetComponent<EnemyHealth>() != null)
            {
                EnemyHealth enemy = nextTarget.GetComponent<EnemyHealth>();

                enemy.TakeDamage(10);
            }
        }
        target = null;  //Once all targets are reached return Shield to Player.
        ts.visibleTargets.Clear();
        StartCoroutine(RecallShield());
    }

    IEnumerator RecallShield()  //Recalls Shield back to Shield Holder.
    {
        shieldRB.isKinematic = false;

        float t = 0f;
        while (t < lerpTime) //Returns Shield to Shield Holder over the course of 1 second.
        {
            t += Time.deltaTime;
            //transform.position = BezierQuadraticCurve(t/lerpTime, transform.position, curvePoint.position, shieldHoldPos.position);
            transform.position = Vector3.Lerp(transform.position, shieldHoldPos.position, t / lerpTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, shieldHoldPos.rotation, t/lerpTime); // lerpTime);

            meshCol.enabled = false; //Prevents from unecessary collisions upon return.

            yield return null;
        }

        transform.parent = shieldHoldPos;  //Sets Shields position and parent to stay attached to Player.
        transform.localPosition = Vector3.zero;

        meshCol.enabled = true;

        shieldRB.isKinematic = true;

        thrown = false;
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

    void Slam()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, slamRadius);

        foreach (Collider col in colliders)
        {
            Rigidbody slamRB = col.GetComponent<Rigidbody>();

            if (slamRB != null)
            {
                slamRB.AddExplosionForce(slamForce, transform.position, slamRadius, slamLift, ForceMode.Impulse);
            }

            EnemyHealth enemy = col.GetComponent<EnemyHealth>();

            if(enemy != null)
            {
                if (damageDelay <= 0)
                {
                    enemy.TakeDamage(10);
                }
            }
        }
    }

    Vector3 BezierQuadraticCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0 + (2 * u * t * p1) + (tt * p2);

        return p;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (thrown)
        {
            if (other.CompareTag("Sticky"))  //Makes rigidbody kinematic so Shield sticks to object.
            {
                shieldRB.isKinematic = true;
            }

            if (other.transform.gameObject.GetComponent<Lever>())
            {
                Lever lever = other.transform.gameObject.GetComponent<Lever>();

                if (lever.canChange)
                {
                    lever.ChangeLever();
                }
            }
        }

        if (isBarging)
        {
            hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, other.transform.position, Quaternion.identity, 1);

            Vector3 knockbackDir = other.transform.position - player.transform.position;
            knockbackDir = knockbackDir.normalized;

            if(other.gameObject.layer == 7)
            {
                Debug.Log("Knockback");
                player.GetComponent<PlayerController>().Knockback(knockbackDir);
            }

            if (other.gameObject.GetComponent<EnemyHealth>())
            {
                EnemyHealth enemy = other.gameObject.GetComponent<EnemyHealth>();

                enemy.TakeDamage(10);
            }
        }

        if (isSlamming)
        {
            if (other.gameObject.GetComponent<EnemyHealth>())
            {
                EnemyHealth enemy = other.gameObject.GetComponent<EnemyHealth>();

                if (!enemy.squashed)
                {
                    enemy.Squash();
                }
            }
        }
    }

  

    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }*/
}


