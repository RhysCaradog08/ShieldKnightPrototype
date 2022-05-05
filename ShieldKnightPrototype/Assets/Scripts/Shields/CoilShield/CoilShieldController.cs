using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Basics.ObjectPool;

public class CoilShieldController : MonoBehaviour
{
    PlayerController pc;
    CharacterController cc;
    TargetingSystem ts;
    ShieldSelect select;
    HeadCollider hc;

    Transform player;
    public GameObject coil, head;
    LineRenderer lr;

    [Header("Whip")]
    [SerializeField] float whipSpeed, range, stopTime;
    public float dist;
    public GameObject target;
    [SerializeField] Vector3 dir;
    public bool canExtend, isExtending, isWhipping, hasTarget;

    [Header("Coil Scale")]
    [SerializeField] Vector3 startScale;
    [SerializeField] float minScale;

    [Header("Grapple")]
    [SerializeField] float grappleSpeed;
    public Transform tetherPoint;
    public GameObject tetheredObject;
    public bool enableTether, canTether, isTethered, isGrappling;

    [Header("Throw")]
    [SerializeField] float throwForce, height, gravity, time;
    public Transform holdPos;
    Rigidbody tetheredRB;
    ObjectIsHeld heldObj;
    Vector3 throwDestination;
    public bool hasObject, throwObject;

    [Header("Spring Jump")]
    [SerializeField] float springHeight;
    RaycastHit hit;
    [SerializeField] Vector3 springPoint;
    public bool canSpring, isSpringing, isFalling;

    [Header("Slam")]
    [SerializeField] float slamForce, slamPushBack, slamRadius, slamLift, damageDelay, slamWait, distToGround;
    GameObject slamStars;
    public bool canSlam, isSlamming;
    bool showSlamVFX;

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
        player = pc.gameObject.transform;
        cc = player.gameObject.GetComponent<CharacterController>();
        ts = FindObjectOfType<TargetingSystem>();
        select = FindObjectOfType<ShieldSelect>();
        hc = FindObjectOfType<HeadCollider>();

        lr = GetComponent<LineRenderer>();       
    }

    // Start is called before the first frame update
    void Start()
    {
        lr.enabled = false;

        //Whip
        canExtend = true;
        isExtending = false;

        //Coil Scale
        startScale = coil.transform.localScale;

        //Grapple
        tetherPoint = null;
        tetheredObject = null;
        enableTether = false;
        canTether = false;
        isTethered = false;
        isGrappling = false;

        //Throw
        height = 0.25f;
        gravity = -9.81f;
        hasObject = false;
        throwObject = false;

        //Spring Jump
        isSpringing = false;
        springPoint = Vector3.zero;

        //Slam
        slamWait = 0;
        isSlamming = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Is Springing: " + isSpringing);

        if (target != null)
        {
            hasTarget = true;
        }
        else hasTarget = false;

        if (stopTime > 0)
        {
            stopTime -= Time.deltaTime;
            pc.enabled = false;
        }
        else if (stopTime <= 0)
        {
            if (canSlam)
            {
                CoilSlam();
                isExtending = true;
            }

            stopTime = 0;
            pc.enabled = true;
        }

        if (dist <= 0)
        {
            dist = 0;
        }

        if (slamWait > 0)
        {
            slamWait -= Time.deltaTime;
        }
        else if (slamWait <= 0)
        {
            slamWait = 0;
        }

        if (cc.isGrounded)
        {
            if (slamWait <= 0)  //Resets player being immobile once grounded after Slam action is performed.
            {
                isSlamming = false;
            }
            else //Whilst slamWait > 0 player is immobile.
            {
                isSlamming = true;
            }
        }

        if (Input.GetButtonUp("Throw") && canExtend)
        {
            stopTime = 0.1f;
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

        if(hasObject && throwObject)
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
        }

        if (!isSpringing && canExtend && cc.isGrounded)
        {
            if (Input.GetButton("Guard"))
            {
                pc.speed = 0;
                pc.anim.SetBool("Spring Set", true);
                pc.enabled = false;
            }
            else
            {
                pc.enabled = true;
                pc.speed = pc.moveSpeed;
            }

            if (Input.GetButtonUp("Guard"))
            {
                if (Physics.Raycast(pc.transform.position, -pc.transform.up, out hit, Mathf.Infinity))
                {
                    springPoint = hit.point;
                }

                isSpringing = true;
                pc.anim.SetBool("Spring Set", false);
                

                pc.velocity.y = pc.jumpHeight * springHeight;
            }
        }

        if (!cc.isGrounded && Input.GetButtonDown("Guard"))  //Input to perform Slam action.
        {
            stopTime = 0.15f;
            canSlam = true;
        }

        if (isSpringing)
        { 
            SpringJump();
            head.transform.position = springPoint;
            stopTime = 0;
        }

        if (isSpringing || isSlamming)
        {
            canSpring = false;
        }

        if (isExtending)
        {
            select.canChange = false;

            if (enableTether)
            {
                canTether = true;
                enableTether = false;
            }

            if (!isSpringing || !canSlam || !isSlamming)
            {
                pc.anim.SetBool("Extending", true);

                if (hasTarget)
                {
                    TargetedWhip();
                }
                else NonTargetWhip();
            }

            if(isSlamming)
            {
                slamWait = 1;

                if(dist >= 0.5f)
                {
                    head.transform.position = springPoint;
                }
            }

            StartCoroutine(ScaleCoil());

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
            pc.anim.SetBool("Extending", false);

            if (!ts.lockedOn)
            {
                //Debug.Log("Not Locked on to Target");
                target = null;
            }

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
            //Debug.DrawLine(transform.position, player.forward, Color.green);

            select.canChange = false;

            canExtend = false;

            tetheredObject.transform.parent = holdPos;
            tetheredObject.transform.position = holdPos.position;
            tetheredRB = tetheredObject.GetComponent<Rigidbody>();
            tetheredRB.isKinematic = true;

            if (!tetheredObject.GetComponent<ObjectIsHeld>())
            {
                tetheredObject.AddComponent<ObjectIsHeld>();
            }

            heldObj = tetheredObject.GetComponent<ObjectIsHeld>();
            heldObj.isHeld = true;
        }

        if (isSlamming)
        {
            canSlam = false;

            pc.speed = 0;
            pc.slamming = true;

            pc.velocity.y = slamForce;

            RaycastHit hit;

            if (Physics.Raycast(pc.transform.position, -pc.transform.up, out hit))
            {
                Debug.DrawRay(pc.transform.position, -pc.transform.up, Color.red);
                distToGround = hit.distance;

                if (distToGround < 2)
                {
                    //Debug.DrawLine(transform.position, -transform.up * 10, Color.green);
                    isExtending = false;
                    SlamImpact();

                    if (!showSlamVFX)
                    {
                        slamStars = ObjectPoolManager.instance.CallObject("SlamStars", null, transform.position, Quaternion.Euler(-90, transform.rotation.y, transform.rotation.z), 1);
                        showSlamVFX = true;
                    }

                    springPoint = Vector3.zero;
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
    }

    void NonTargetWhip() //Launches coil head forward to the extent of range value.
    {
        dir = player.position + player.forward * 10;
        //Debug.DrawLine(player.position, dir * range, Color.green);

        dist = Vector3.Distance(head.transform.position, dir * range);

        if (dist >= 1 && isExtending)
        {
            head.transform.position = Vector3.Lerp(head.transform.position, dir * range, whipSpeed * Time.deltaTime);
        }
    }

    void TargetedWhip() //Launches coil head towards target.
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
                MarkerCheck markerCheck = target.GetComponent<MarkerCheck>();
                markerCheck.RemoveMarker();
            }
            
            if(!ts.lockedOn)
            { 
                Debug.Log("Not Locked on to Target");
                ts.visibleTargets.Remove(target);
            }
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

        if(tetheredObject.GetComponent<MarkerCheck>() != null)
        {
            MarkerCheck markerCheck = tetheredObject.GetComponent<MarkerCheck>();

            if(!markerCheck.canAddMarker)
            {
                markerCheck.RemoveMarker();
            }
        }
  
    }

    public void ThrowTetheredObject()
    {
        //Debug.Log("Throw Tethered Object");
        tetheredObject.transform.parent = null;
        tetheredObject = null;
        tetheredRB.isKinematic = false;

        if (hasTarget)
        {
            throwDestination =  target.transform.position;
        }
        else throwDestination = player.position + player.forward * 10;

        tetheredRB.velocity = CalculateThrowVelocity() * throwForce;

        heldObj.isHeld = false;
        heldObj.isThrown = true;

        tetheredRB = null;
        hasObject = false;
        throwObject = false;
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

        heldObj.isHeld = false;

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
        if (Physics.Raycast(pc.transform.position, -pc.transform.up, out hit, Mathf.Infinity))
        {
            springPoint = hit.point;
            //Debug.DrawLine(head.transform.position, springPoint, Color.green);
        }

        //isExtending = true;

        head.transform.position = springPoint;

        isSlamming = true;
    }

    void SlamImpact()
    {
        Debug.Log("Slam Impact");

        distToGround = 0;

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

    Vector3 CalculateThrowVelocity()
    {
        float displacementY = throwDestination.y - tetheredRB.position.y;
        Vector3 displacementXZ = new Vector3(throwDestination.x - tetheredRB.position.x, 0, throwDestination.z - tetheredRB.position.z);

        //time = (Mathf.Sqrt(-2 * gravity * height) + Mathf.Sqrt(2 * (displacementY - height) / gravity));

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / time;

        return velocityXZ + velocityY * -Mathf.Sign(gravity);
    }
}
