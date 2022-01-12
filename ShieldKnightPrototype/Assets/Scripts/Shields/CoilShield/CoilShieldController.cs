using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class CoilShieldController : MonoBehaviour
{
    Transform player;
    Camera cam;
    PlayerController pc;
    CharacterController cc;
    TargetingSystem ts;
    ShieldSelect select;
    HeadCollider hc;

    public GameObject coil, head;
    LineRenderer lr;

    [Header("Whip")]
    [SerializeField] float whipSpeed, range, stopTime;
    public float dist;
    public GameObject target;
    Vector3 dir;
    public bool canExtend, isExtending, hasTarget;

    [Header("Coil Scale")]
    [SerializeField] Vector3 startScale;
    [SerializeField] float minScale;

    [Header("Grapple")]
    [SerializeField] float grappleSpeed, throwForce, momentumExtra;
    public Transform tetherPoint, holdPos;
    public GameObject tetheredObject;
    Rigidbody tetheredRB;
    public bool enableTether, canTether, isTethered, isGrappling, hasObject;

    [Header("Spring Jump")]
    [SerializeField] float springHeight;
    RaycastHit hit;
    [SerializeField] Vector3 springPoint;
    public bool isSpringing;

    [Header("Slam")]
    [SerializeField] float slamForce, slamPushBack, slamRadius, slamLift, slamDelay, damageDelay, slamWait;
    GameObject slamStars;
    public bool canSlam, isSlamming;
    bool showSlamVFX;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main;
        pc = player.gameObject.GetComponent<PlayerController>();
        cc = player.gameObject.GetComponent<CharacterController>();
        ts = FindObjectOfType<TargetingSystem>();
        select = FindObjectOfType<ShieldSelect>();
        hc = FindObjectOfType<HeadCollider>();

        lr = GetComponent<LineRenderer>();       
    }

    // Start is called before the first frame update
    void Start()
    {
        //Whip
        canExtend = true;
        isExtending = false;

        //Coil Scale
        startScale = coil.transform.localScale;

        //Grapple
        tetheredObject = null;
        enableTether = false;
        canTether = false;
        isTethered = false;
        isGrappling = false;
        hasObject = false;

        //Spring Jump
        isSpringing = false;

        //Slam
        canSlam = false;
        isSlamming = false;

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(holdPos.position, dir, Color.green);

        if (target != null)
        {
            hasTarget = true;
        }
        else hasTarget = false;

        if(stopTime > 0)
        {
            stopTime -= Time.deltaTime;
            pc.enabled = false;
        }
        else if(stopTime <= 0)
        {
            if(canSlam)
            {
                CoilSlam();
            }

            stopTime = 0;
            pc.enabled = true;
        }

        if (cc.isGrounded)
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

        if (Input.GetButtonUp("Throw") && canExtend)
        {
            stopTime = 0.1f;
            isExtending = true;
        }
        else if (Input.GetButtonDown("Throw") && !isSpringing)
        {
            if(isTethered)
            {
                isTethered = false;
                tetherPoint = null;
            }

            isExtending = false;
        }

        if(Input.GetButtonUp("Throw") && hasObject)
        {
            isTethered = false;
            tetherPoint = null;

            ThrowTetheredObject();
        }

        if(Input.GetButtonDown("Barge"))
        {
            if(hasObject)
            {
                DropTetheredObject();

                target = null;
                ts.visibleTargets.Clear();
            }           
            else enableTether = true;
        }

        if (Input.GetButtonUp("Barge") && enableTether)
        {
            stopTime = 0.1f;
            isExtending = true;
        }

        if (!isSpringing && canExtend && cc.isGrounded)
        {
            if (Input.GetButton("Guard"))
            {
                pc.speed = 0;
                pc.enabled = false;
            }
            else
            {
                pc.speed = pc.moveSpeed;
                pc.enabled = true;
            }

            if (Input.GetButtonUp("Guard"))
            {
                pc.velocity.y = pc.jumpHeight * springHeight;

                if (Physics.Raycast(head.transform.position, head.transform.forward, out hit, Mathf.Infinity))
                {
                    springPoint = hit.point;
                }
                isSpringing = true;
            }
        }

        if (!cc.isGrounded && Input.GetButtonDown("Guard"))  //Input to perform Slam action.
        {
            stopTime = 0.15f;
            canSlam = true;

            /*if (Physics.Raycast(head.transform.position, head.transform.forward, out hit, Mathf.Infinity))
            {
                springPoint = hit.point;
                Debug.DrawLine(head.transform.position, springPoint, Color.green);
            }

            isExtending = true;

            head.transform.position = springPoint;*/
        }

        if (isSpringing)
        { 
            SpringJump();
        }

        if (isExtending)
        {
            select.canChange = false;

            if (enableTether)
            {
                canTether = true;
                enableTether = false;
            }

            StartCoroutine(ScaleCoil());

            if (!isSpringing || !canSlam || !isSlamming)
            {
                //stopTime = 0.1f;

                if (hasTarget)
                {
                    TargetedWhip();
                }
                else NonTargetWhip();
            }

            if(isSpringing)
            {
                stopTime = 0;
            }

            if(isSlamming)
            {
                if(dist >= 0.5f)
                {
                    head.transform.position = springPoint;
                }
            }

            canExtend = false;
            lr.enabled = true;
        }
        else
        {
            lr.enabled = false;

            coil.transform.localScale = startScale;
        }        

        if(isExtending && dist < 1 && !isTethered)
        {
            //Debug.Log("Coil Over Extended");
            isExtending = false;
        }

        if (!isExtending)
        {
            target = null;

            if (!isSpringing)
            {
                dist = Vector3.Distance(head.transform.position, transform.position);

                head.transform.position = Vector3.Lerp(head.transform.position, transform.position, whipSpeed * Time.deltaTime);

                if (dist < 0.1f)
                {
                    head.transform.position = transform.position;

                    canExtend = true;

                    if (canTether)
                    {
                        canTether = false;
                    }

                    if(!hasObject)
                    {
                        select.canChange = true;
                    }
                }
            }
        } 
        
        if(isTethered)
        {
            canExtend = false;
            isExtending = false;

            head.transform.position = tetherPoint.position;
            canTether = false;
            isGrappling = true;
        }

        if (isGrappling)
        {
            if (hc.grappleFixed)
            {
                Grapple();
            }

            if (hc.grappleLoose)
            {
                if (tetheredObject != null)
                {
                    GetTetheredObject();
                }
            }
        }

        if (hasObject)
        {
            select.canChange = false;

            canExtend = false;

            tetheredObject.transform.parent = holdPos;
            tetheredObject.transform.position = holdPos.position;
            tetheredRB = tetheredObject.GetComponent<Rigidbody>();
            tetheredRB.isKinematic = true;
        }

        if (isSlamming)
        {
            pc.speed = 0;
            pc.slamming = true;

            pc.velocity.y = slamForce;

            RaycastHit hit;

            if (Physics.Raycast(transform.position, -transform.up * 10, out hit))
            {
                //Debug.DrawLine(transform.position, transform.right, Color.red);
                float distToGround = hit.distance;

                if (distToGround < 1)
                {
                    //Debug.DrawLine(transform.position, -transform.up * 10, Color.green);
                    isExtending = false;

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
            pc.stopped = false;
            pc.slamming = false;
            showSlamVFX = false;
        }

        if (lr.enabled == true)
        {
            ShowLineRenderer();
        }

        if (cc.isGrounded && isSlamming)
        {
            slamWait -= Time.deltaTime;
        }
    }

    void NonTargetWhip()
    {
        dir = player.position + player.forward * 10;
        Debug.DrawLine(player.position, dir * range, Color.green);
        //Debug.Log("Whip Range: " + whipDir.z * range);

        dist = Vector3.Distance(head.transform.position, dir * range);

        if (dist >= 1 && isExtending)
        {
            head.transform.position = Vector3.Lerp(head.transform.position, dir * range, whipSpeed * Time.deltaTime);
        }
    }

    void TargetedWhip()
    {
        dist = Vector3.Distance(head.transform.position, target.transform.position);

        if (dist >= 1 && isExtending)
        {
            head.transform.position = Vector3.Lerp(head.transform.position, target.transform.position, whipSpeed * Time.deltaTime);
        }

        if(dist <= 0.9f)
        {
            foreach (GameObject target in ts.visibleTargets)
            {
                ts.markerCheck = target.GetComponent<MarkerCheck>();
                ts.markerCheck.RemoveMarker();
            }

            ts.visibleTargets.Remove(target);
        }
    }

    void ShowLineRenderer()
    {
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, head.transform.position);
    }

    IEnumerator ScaleCoil()
    {
        while(minScale < coil.transform.localScale.x && isExtending)
        {
            coil.transform.localScale -= startScale * Time.deltaTime * dist/whipSpeed;
            yield return null;
        }
        
        while(coil.transform.localScale.x < startScale.x && !isExtending)
        {
            coil.transform.localScale += startScale * Time.deltaTime * dist/(whipSpeed * 2);
            yield return null;
        }
    }

    void Grapple()
    {
        Debug.Log("Grappling");
        dir = (head.transform.position - player.position).normalized;
        dist = Vector3.Distance(player.position, head.transform.position);

        pc.enabled = false;

        if (dist < 1.5f)
        {

            isGrappling = false;
            isTethered = false;
            tetherPoint = null;
            isExtending = false;

            /*pc.velocityMomentum = dir * grappleSpeed * momentumExtra;
            pc.velocity.y = 0;*/

            hc.grappleFixed = false;
            pc.enabled = true;
            pc.speed = pc.moveSpeed;

        }
        else cc.Move(dir * grappleSpeed * Time.deltaTime);
    }

    void GetTetheredObject()
    {
        //Debug.Log("Get Tethered Object");
        dist = Vector3.Distance(tetheredObject.transform.position, holdPos.position);

        tetheredObject.transform.position = Vector3.Lerp(tetheredObject.transform.position, holdPos.position, grappleSpeed * Time.deltaTime);

        if(dist < 1.5f)
        {
            Debug.Log("Has Tethered Object");

            isGrappling = false;
            hc.grappleLoose = false;
            hasObject = true;
            isTethered = false;
        }
    }

    void ThrowTetheredObject()
    {
        //Debug.Log("Throw Tethered Object");
        tetheredObject.transform.parent = null;
        tetheredObject = null;
        tetheredRB.isKinematic = false;

        dir = player.position + player.forward * 10;

        tetheredRB.AddForce(dir * throwForce, ForceMode.Impulse);

        tetheredRB = null;
        hasObject = false;
        select.canChange = true;
    }

    void DropTetheredObject()
    {
        tetheredObject.transform.parent = null;
        tetheredObject = null;
        tetheredRB.isKinematic = false;
        tetheredRB = null;
        tetherPoint = null;
        hasObject = false;
        select.canChange = true;
    }

    void SpringJump()
    {
        isExtending = true;

        head.transform.position = springPoint;

        dist = Vector3.Distance(head.transform.position, transform.position);

        if (dist >= 10)
        {
            isSpringing = false;
            isExtending = false;
        }
    }

    void CoilSlam()
    {
        canSlam = false;

        if (Physics.Raycast(head.transform.position, head.transform.forward, out hit, Mathf.Infinity))
        {
            springPoint = hit.point;
            Debug.DrawLine(head.transform.position, springPoint, Color.green);
        }

        isExtending = true;

        head.transform.position = springPoint;

        isSlamming = true;
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

            if (enemy != null)
            {
                if (damageDelay <= 0)
                {
                    enemy.TakeDamage(10);
                }
            }
        }
    }
}
