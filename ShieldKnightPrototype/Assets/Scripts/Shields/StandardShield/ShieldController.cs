using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ShieldController : MonoBehaviour
{
    PlayerController player;
    CharacterController cc;
    TargetingSystem ts;
    ShieldSelect select;

    Rigidbody shieldRB;
    Transform shieldHoldPos;

    [Header("Throw")]
    public float throwForce;
    public GameObject target;
    TrailRenderer trail;
    GameObject hitStars;
    public bool thrown, hasTarget, canThrow;

    [Header("Recall")]
    public Transform curvePoint;
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

    [Header("Barge")]
    [SerializeField] float bargeDelay;
    public float bargeTime, bargeSpeed;
    public GameObject closest;
    GameObject marker;
    public bool canBarge, isBarging;

    [Header("Dodge")]
    public bool canDodge, isDodging;
    public float dodgeTime, dodgeSpeed;
    float dodgeDelay;

    [Header("Slam")]
    [SerializeField] float slamForce, slamPushBack, slamRadius, slamLift, slamDelay, damageDelay;
    [SerializeField] float waitTime;
    GameObject slamStars;
    public bool isSlamming;
    bool showSlamVFX;

    [Header("UI")]
    private GameObject targetMarker;

    // Start is called before the first frame update
    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        cc = player.gameObject.GetComponent<CharacterController>();
        ts = transform.root.GetComponent<TargetingSystem>();
        select = FindObjectOfType<ShieldSelect>();

        shieldRB = GetComponent<Rigidbody>();
        shieldHoldPos = transform.parent.transform;

        trail = GetComponent<TrailRenderer>();

        meshCol = GetComponentInChildren<MeshCollider>();
    }

    void Start()
    {
        //Throw
        thrown = false;

        //Barge
        canBarge = true;
        isBarging = false;
        closest = null;

        //Dodge
        isDodging = false;

        //Slam
        isSlamming = false;
        showSlamVFX = false;

        damageDelay = 0.5f;
    }

    private void Update()
    {
        //Debug.Log("Shield Barging: " + isBarging);
        //Debug.Log("Shield Dodging: " + isDodging);

        if (target != null)
        {
            hasTarget = true;
        }
        else hasTarget = false;

        if(cc.isGrounded)
        {
            if (waitTime <= 0)  //Resets player being immobile once grounded after Slam action is performed.
            {
                waitTime = 0;
                //stopped = false;
                isSlamming = false;
            }
            else //Whilst waitTime > 0 player is immobile.
            {
                //stopped = true;
                isSlamming = true;
            }
        }


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

        if (Input.GetButtonDown("Throw") && thrown) //If Player doesn't have possession of Shield it gets recalled to player.
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
            select.canChange = false;
        }
        else
        {
            trail.enabled = false;
            select.canChange = true;
        }

        bargeDelay -= Time.deltaTime;
        dodgeDelay -= Time.deltaTime;

        if (Input.GetButtonDown("Barge")) //Input to perform Barge/Dodge action.
        {
            if (!thrown)
            {
                if (canBarge && bargeDelay <= 0)
                {
                    StartCoroutine(Barge());
                }
            }
            else if (thrown)
            {
                if (canDodge && dodgeDelay <= 0)
                {
                    StartCoroutine(Dodge());
                }
            }
        }

        if (!cc.isGrounded && Input.GetButtonDown("Guard"))  //Input to perform Slam action.
        {
            waitTime = 0.5f;
            isSlamming = true;
        }

        if (isBarging)
        {
            player.barging = true;
        }
        else player.barging = false;

        if (isDodging)
        {
            player.dodging = true;
        }
        else player.dodging = false;

        canBarge = true;
        isBarging = false;

        canDodge = true;
        isDodging = false;        

        if (isSlamming)
        {
            player.stopped = true;
            player.slamming = true;

            StartCoroutine(SlamDown());

            RaycastHit hit;

            if (Physics.Raycast(transform.position, -transform.up * 10, out hit))
            {
                //Debug.DrawLine(transform.position, transform.right, Color.red);
                float distToGround = hit.distance;

                if (distToGround < 1)
                {
                    Debug.DrawLine(transform.position, -transform.up * 10, Color.green);

                    SlamImpact();

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
            player.stopped = false;
            player.slamming = false;
            showSlamVFX = false;
        }

        if (cc.isGrounded && isSlamming)
        {
            waitTime -= Time.deltaTime;
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

    IEnumerator Barge()
    {
        float startTime = Time.time;

        new WaitForSeconds(1); //Prevents player from stacking Barges.

        while (Time.time < startTime + bargeTime)  //Player movement speed is disabled then moved by bargeSpeed over bargeTime;
        {
            isBarging = true;
            //trailEffect.SetActive(true);
            player.speed = 0;

            canBarge = false;

            if (ts.lockedOn)
            {
                Vector3 closestDir = (ts.closest.transform.position - transform.position).normalized;
                Vector3 closestRot = Vector3.RotateTowards(transform.forward, closestDir, 10, 0);
                transform.rotation = Quaternion.LookRotation(closestRot);

                cc.Move(closestDir * bargeSpeed * Time.deltaTime);
            }
            else cc.Move(player.moveDir * bargeSpeed * Time.deltaTime);

            bargeDelay = 0.5f;

            yield return null;
        }

        if (marker != null)
        {
            ObjectPoolManager.instance.RecallObject(marker);
        }
    }

    IEnumerator Dodge()
    {
        float startTime = Time.time;

        new WaitForSeconds(1); //Prevents player from stacking Dodges.

        while (Time.time < startTime + bargeTime)  //Player movement speed is disabled then moved by dodgeSpeed over dodgeTime;
        {
            isDodging = true;
            canDodge = false;
            player.speed = 0;

            cc.Move(player.moveDir * dodgeSpeed * Time.deltaTime);

            dodgeDelay = 0.5f;

            yield return null;
        }
    }

    IEnumerator SlamDown() //Player movement is frozen then directed down by slamForce.
    {
        yield return new WaitForSeconds(slamDelay);
        player.velocity.y = slamForce;
    }

    void SlamImpact()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, slamRadius);

        foreach (Collider col in colliders)
        {
            Rigidbody slamRB = col.GetComponent<Rigidbody>();

            if (slamRB != null)
            {
                slamRB.AddExplosionForce(slamPushBack, transform.position, slamRadius, slamLift, ForceMode.Impulse);
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
}


