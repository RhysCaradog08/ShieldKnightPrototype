using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoilShieldController : MonoBehaviour
{
    Transform player;
    Camera cam;
    [SerializeField] CharacterController cc;
    TargetingSystem ts;
    HeadCollider hc;

    public GameObject coil, head;
    LineRenderer lr;

    [Header("Whip")]
    [SerializeField] float whipSpeed, range;
    public float dist;
    public GameObject target;
    Vector3 dir;
    public bool canWhip, whipping, hasTarget;

    [Header("Coil Scale")]
    [SerializeField] Vector3 startScale;
    [SerializeField] float minScale;

    [Header("Grapple")]
    [SerializeField] float grappleSpeed, throwForce;
    public Transform tetherPoint, holdPos;
    public GameObject tetheredObject;
    Rigidbody tetheredRB;
    public bool enableTether, canTether, tethered, grappling, hasObject;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main;
        ts = FindObjectOfType<TargetingSystem>();
        hc = FindObjectOfType<HeadCollider>();

        lr = GetComponent<LineRenderer>();

        canWhip = true;
        whipping = false;

        startScale = coil.transform.localScale;

        tetheredObject = null;
        enableTether = false;
        canTether = false;
        tethered = false;
        grappling = false;
        hasObject = false;
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

        if (Input.GetButtonUp("Throw") && canWhip)
        {
            whipping = true;

        }
        else if (Input.GetButtonDown("Throw"))
        {
            if(tethered)
            {
                tethered = false;
                tetherPoint = null;
            }

            whipping = false;
        }

        if(Input.GetButtonUp("Throw") && hasObject )
        {
            tethered = false;
            tetherPoint = null;

            ThrowTetheredObject();
        }

        if(Input.GetButtonDown("Barge"))
        {
            enableTether = true;
        }

        if (Input.GetButtonUp("Barge"))
        {
            whipping = true;
        }

        if (whipping)
        {
            if(enableTether)
            {
                canTether = true;
                enableTether = false;
            }

            canWhip = false;
            lr.enabled = true;

            StartCoroutine(ScaleCoil());
            if(hasTarget)
            {
                TargetedWhip();
            }
            else NonTargetWhip();
        }
        else
        {
            lr.enabled = false;

            coil.transform.localScale = startScale;
        }

        if(whipping && dist < 1 && !tethered)
        {
            //Debug.Log("Coil Over Extended");
            whipping = false;
        }

        if (!whipping)
        {
            target = null;

            dist = Vector3.Distance(head.transform.position, transform.position);

            head.transform.position = Vector3.Lerp(head.transform.position, transform.position, whipSpeed * Time.deltaTime);

            if (dist < 0.1f)
            {
                head.transform.position = transform.position;

                canWhip = true;

                if(canTether)
                {
                    canTether = false;
                }
            }
        } 
        
        if(tethered)
        {
            canWhip = false;
            whipping = false;

            head.transform.position = tetherPoint.position;
            canTether = false;
            grappling = true;
        }

        if(grappling)
        {
            if(hc.grappleFixed)
            {
                Grapple();
            }

            if(hc.grappleLoose)
            {
                if (tetheredObject != null)
                {
                    GetTetheredObject();
                }
            }
        }

        if (hasObject)
        {
            canWhip = false;

            tetheredObject.transform.parent = holdPos;
            tetheredObject.transform.position = holdPos.position;
            tetheredRB = tetheredObject.GetComponent<Rigidbody>();
            tetheredRB.isKinematic = true;
        }

        if(tetheredObject != null)
        {
            Debug.Log("Is tethered to: " + tetheredObject.name);
        }
        else if(tetheredObject == null)
        {
            Debug.Log("Is not tethered to anything");
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

        if (dist >= 1 && whipping)
        {
            head.transform.position = Vector3.Lerp(head.transform.position, dir * range, whipSpeed * Time.deltaTime);
        }
    }

    void TargetedWhip()
    {
        dist = Vector3.Distance(head.transform.position, target.transform.position);

        if (dist >= 1 && whipping)
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
        while(minScale < coil.transform.localScale.x && whipping)
        {
            coil.transform.localScale -= startScale * Time.deltaTime * dist/whipSpeed;
            yield return null;
        }
        
        while(coil.transform.localScale.x < startScale.x && !whipping)
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

        PlayerController pc = player.GetComponent<PlayerController>();
        pc.enabled = false;

        cc = player.GetComponent<CharacterController>();
        cc.Move(dir * grappleSpeed * Time.deltaTime);

        if (dist < 2)
        {
            grappling = false;
            tethered = false;
            tetherPoint = null;
            whipping = false;

            hc.grappleFixed = false;
            pc.enabled = true;
            pc.speed = pc.moveSpeed;
        }
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
    }
}
