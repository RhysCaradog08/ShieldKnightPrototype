using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ShieldController : MonoBehaviour
{
    PlayerController pc;
    ShieldKnightController sk;
    CharacterController cc;
    TargetingSystem ts;
    ShieldSelect select;

    [SerializeField] Rigidbody shieldRB;
    Transform shieldHoldPos;

    [Header("Throw")]
    public float throwForce;
    public GameObject target;
    TrailRenderer trail;
    GameObject hitStars;
    public bool thrown, hasTarget, canThrow;

    [Header("Recall")]
    //public Transform curvePoint;
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
    [SerializeField] float dodgeDelay;

    [Header("Guard/Parry")]
    const float minButtonHold = 0.25f;
    float buttonHeldTime;
    bool buttonHeld;
    public GameObject parryBox;

    [Header("Slam")]
    [SerializeField] float slamForce, slamPushBack, slamRadius, slamLift, slamDelay, damageDelay, slamWait;
    GameObject slamStars;
    public bool isSlamming;
    bool showSlamVFX;

    [Header("UI")]
    private GameObject targetMarker;

    // Start is called before the first frame update
    void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
        sk = FindObjectOfType<ShieldKnightController>();
        cc = FindObjectOfType<CharacterController>();
        ts = FindObjectOfType<TargetingSystem>();
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
        transform.localScale = transform.localScale;

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
        }

        if(dodgeDelay > 0) 
        {
            dodgeDelay -= Time.deltaTime;
        }
        if(dodgeDelay <= 0)
        {
            dodgeDelay = 0;
        }

        if(slamDelay > 0)
        {
            slamDelay -= Time.deltaTime;
        }
        if(slamDelay <= 0)
        {
            slamDelay = 0;
            isSlamming = false;
        }

        if(cc.isGrounded)
        {
            if (slamWait <= 0)  //Resets player being immobile once grounded after Slam action is performed.
            {
                slamWait = 0;
                isSlamming = false;
            }
            else //Whilst waitTime > 0 player is immobile.
            {
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
        }
        else trail.enabled = false;

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

        if(sk.buttonHeld) 
        {
            
        }

        if (!cc.isGrounded && Input.GetButtonDown("Guard"))  //Input to perform Slam action.
        {
            slamWait = 0.5f;
            isSlamming = true;
        }

        if (isBarging)
        {
            pc.barging = true;
            sk.isBarging = true;
        }
        else
        {
            pc.barging = false;
            sk.isBarging = false;
        }

        if (isDodging)
        {
            pc.dodging = true;
        }
        else pc.dodging = false;


        if (isSlamming)
        {
            pc.stopped = true;
            pc.slamming = true;

            pc.velocity.y = slamForce;

            RaycastHit hit;

            if (Physics.Raycast(pc.transform.position, -pc.transform.up, out hit))
            {
                //Debug.DrawLine(pc.transform.position, -pc.transform.up * 10, Color.green);

                float distToGround = hit.distance;

                if (distToGround < 1)
                {
                    SlamImpact();

                    if (!showSlamVFX)
                    {
                        slamStars = ObjectPoolManager.instance.CallObject("SlamStars", null, transform.position, Quaternion.Euler(-90, transform.rotation.y, transform.rotation.z), 1);
                        showSlamVFX = true;
                    }

                    slamDelay = 0.5f;
                }
            }
        }
        else
        {
            pc.stopped = false;
            pc.slamming = false;
            showSlamVFX = false;
        }

        if (cc.isGrounded && isSlamming)
        {
            slamWait -= Time.deltaTime;
        }

        if (thrown || isBarging || isSlamming)
        {
            if (select.canChange)
            {
                select.canChange = false;
            }
        }
        else
        {
            if (!select.canChange)
            {
                select.canChange = true;
            }
        }

        canBarge = true;
        isBarging = false;

        canDodge = true;
        isDodging = false;        
    }

    void NonTargetThrow()  //Throws Shield in players forward vector if no targets are identified.
    {
        //Vector3 dir = pc.transform.position + pc.transform.forward * 10;

        thrown = true;

        shieldRB.isKinematic = false;
        //shieldRB.AddForce(pc.transform.forward * throwForce, ForceMode.Impulse);
        shieldRB.AddForce(sk.transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;
    }

    IEnumerator TargetedThrow()  //Throws Shield towards any identified targets in range.
    {
        thrown = true;

        foreach (GameObject nextTarget in ts.visibleTargets) //Sets nextTarget in list to be target and move shield towards target.
        {
            target = nextTarget;
            Vector3 nextTargetPos = nextTarget.transform.position;
            while (Vector3.Distance(nextTargetPos, transform.position) > 0.1f)
            {
                transform.parent = null;

                transform.position = Vector3.MoveTowards(transform.position, nextTargetPos, throwForce * Time.deltaTime);

                yield return null;
            }

            if (Vector3.Distance(nextTargetPos, transform.position) < 0.1f)
            {
                hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, nextTargetPos, Quaternion.identity, 1);


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
        }
        target = null;  //Once all targets are reached return Shield to Player.
        ts.visibleTargets.Clear();
        StartCoroutine(RecallShield());
    }

    IEnumerator RecallShield()  //Recalls Shield back to Shield Holder.
    {
        pc.anim.SetTrigger("Catch");

        float t = 0f;
        while (t < lerpTime) //Returns Shield to Shield Holder over the course of 1 second.
        {
            shieldRB.isKinematic = false;

            t += Time.deltaTime;
            //transform.position = BezierQuadraticCurve(t/lerpTime, transform.position, curvePoint.position, shieldHoldPos.position);
            transform.position = Vector3.Lerp(transform.position, shieldHoldPos.position, t / lerpTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, shieldHoldPos.rotation, t/lerpTime);

            meshCol.enabled = false; //Prevents from unecessary collisions upon return.

            yield return null;
        }


        shieldRB.isKinematic = true;

        transform.parent = shieldHoldPos;  //Sets Shields position and parent to stay attached to Player.
        transform.localPosition = Vector3.zero;

        meshCol.enabled = true;

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
            pc.speed = 0;

            canBarge = false;

            if (ts.lockedOn)
            {
                Vector3 closestDir = (ts.closest.transform.position - transform.position).normalized;

                cc.Move(closestDir * bargeSpeed * Time.deltaTime);
            }
            else cc.Move(pc.moveDir * bargeSpeed * Time.deltaTime);

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

        while (Time.time < startTime + dodgeTime)  //Player movement speed is disabled then moved by dodgeSpeed over dodgeTime;
        {
            isDodging = true;
            canDodge = false;
            pc.speed = 0;

            cc.Move(pc.moveDir * dodgeSpeed * Time.deltaTime);

            dodgeDelay = 0.5f;

            yield return null;
        }
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

                if(enemy.squashed)
                {
                    isSlamming = false;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (thrown)
        {
            if (col.gameObject.CompareTag("Sticky"))  //Makes rigidbody kinematic so Shield sticks to object.
            {
                shieldRB.isKinematic = true;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}


