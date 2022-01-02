using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoilShieldController : MonoBehaviour
{
    Transform player;
    Camera cam;
    TargetingSystem ts;
    HeadCollider hc;

    public GameObject coil, head;
    LineRenderer lr;

    [Header("Whip")]
    [SerializeField] float whipSpeed, range;
    public float dist;
    public GameObject target;
    public Transform tetherPoint;
    public bool canWhip, whipping, hasTarget, enableTether, canTether, tethered, grappling;

    [Header("Coil Scale")]
    [SerializeField] Vector3 startScale;
    [SerializeField] float minScale;


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
        enableTether = false;
        canTether = false;
        tethered = false;

        startScale = coil.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
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

        if(Input.GetButtonDown("Barge"))
        {
            enableTether = true;
        }

        if(canTether)
        {
            Debug.Log("Can Tether");
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
            Debug.Log("Coil Over Extended");
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
            head.transform.position = tetherPoint.position;
            canTether = false;
            grappling = true;
        }

        if(grappling)
        {
            Grapple();
        }

        if (lr.enabled == true)
        {
            ShowLineRenderer();
        }
    }

    void NonTargetWhip()
    {
        Vector3 whipDir = player.position + player.forward * 10;
        Debug.DrawLine(player.position, whipDir * range, Color.green);
        //Debug.Log("Whip Range: " + whipDir.z * range);

        dist = Vector3.Distance(head.transform.position, whipDir * range);

        if (dist >= 1 && whipping)
        {
            head.transform.position = Vector3.Lerp(head.transform.position, whipDir * range, whipSpeed * Time.deltaTime);
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
        if(hc.grappleFixed)
        {

        }
        else if(hc.grappleLoose)
        {

        }
    }
}
