using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] float whipSpeed, range;
    public float dist;
    public GameObject target;
    Vector3 dir;
    public bool canExtend, extending, hasTarget;

    [Header("Coil Scale")]
    [SerializeField] Vector3 startScale;
    [SerializeField] float minScale;

    [Header("Grapple")]
    [SerializeField] float grappleSpeed, throwForce, stopTime, momentumExtra;
    public Transform tetherPoint, holdPos;
    public GameObject tetheredObject;
    Rigidbody tetheredRB;
    public bool enableTether, canTether, tethered, grappling, hasObject;

    [Header("Spring Jump")]
    [SerializeField] float springHeight;
    RaycastHit hit;
    public GameObject hitObject;
    [SerializeField] Vector3 springPoint;
    public bool springing;

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
        
        startScale = coil.transform.localScale;
    }

    // Start is called before the first frame update
    void Start()
    {
        canExtend = true;
        extending = false;

        tetheredObject = null;
        enableTether = false;
        canTether = false;
        tethered = false;
        grappling = false;
        hasObject = false;

        springing = false;
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

        if (Input.GetButtonUp("Throw") && canExtend)
        {
            extending = true;
        }
        else if (Input.GetButtonDown("Throw") && !springing)
        {
            if(tethered)
            {
                tethered = false;
                tetherPoint = null;
            }

            extending = false;
        }

        if(Input.GetButtonUp("Throw") && hasObject)
        {
            tethered = false;
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
            extending = true;
        }

        if (!springing && canExtend && cc.isGrounded)
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
                springing = true;
                pc.velocity.y = pc.jumpHeight * springHeight;

                if (Physics.Raycast(head.transform.position, head.transform.forward, out hit, Mathf.Infinity))
                {
                    springPoint = hit.point;
                }
            }
        }

        if (springing)
        { 
            SpringJump();
        }
 

        if (extending)
        {
            select.canChange = false;

            if (enableTether)
            {
                canTether = true;
                enableTether = false;
            }

            canExtend = false;
            lr.enabled = true;

            if (!springing)
            {
                stopTime = 0.1f;

                StartCoroutine(ScaleCoil());
                if (hasTarget)
                {
                    TargetedWhip();
                }
                else NonTargetWhip();
            }
        }
        else
        {
            lr.enabled = false;

            coil.transform.localScale = startScale;
        }

        if(stopTime > 0)
        {
            stopTime -= Time.deltaTime;
            pc.enabled = false;
        }
        else if(stopTime <= 0)
        {
            stopTime = 0;
            pc.enabled = true;
        }
        

        if(extending && dist < 1 && !tethered)
        {
            //Debug.Log("Coil Over Extended");
            extending = false;
        }

        if (!extending)
        {
            target = null;

            if (!springing)
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
        
        if(tethered)
        {
            canExtend = false;
            extending = false;

            head.transform.position = tetherPoint.position;
            canTether = false;
            grappling = true;
        }

        if (grappling)
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


        if (lr.enabled == true)
        {
            ShowLineRenderer();
        }
    }

    void NonTargetWhip()
    {
        dir = player.position + player.forward * 10;
        Debug.DrawLine(player.position, dir * range, Color.green);
        //Debug.Log("Whip Range: " + whipDir.z * range);

        dist = Vector3.Distance(head.transform.position, dir * range);

        if (dist >= 1 && extending)
        {
            head.transform.position = Vector3.Lerp(head.transform.position, dir * range, whipSpeed * Time.deltaTime);
        }
    }

    void TargetedWhip()
    {
        dist = Vector3.Distance(head.transform.position, target.transform.position);

        if (dist >= 1 && extending)
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
        while(minScale < coil.transform.localScale.x && extending)
        {
            coil.transform.localScale -= startScale * Time.deltaTime * dist/whipSpeed;
            yield return null;
        }
        
        while(coil.transform.localScale.x < startScale.x && !extending)
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

            grappling = false;
            tethered = false;
            tetherPoint = null;
            extending = false;

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

            grappling = false;
            hc.grappleLoose = false;
            hasObject = true;
            tethered = false;
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
        extending = true;

        head.transform.position = springPoint;

        dist = Vector3.Distance(head.transform.position, transform.position);

        if (dist >= 10)
        {
            springing = false;
            extending = false;
        }
    }
}
