using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoilShieldController : MonoBehaviour
{
    Transform player;
    Camera cam;
    TargetingSystem ts;

    public GameObject coil, head;
    LineRenderer lr;

    [Header("Whip")]
    [SerializeField] float whipSpeed, range, dist;
    [SerializeField] bool whipping, extending;
    public GameObject target;
    public bool hasTarget;

    [Header("Coil Scale")]
    [SerializeField] Vector3 startScale;
    [SerializeField] float minScale;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main;
        ts = FindObjectOfType<TargetingSystem>();

        lr = GetComponent<LineRenderer>();

        whipping = false;
        extending = false;

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

        if (Input.GetButtonDown("Throw") && !whipping)
        {
            whipping = true;
            extending = true;
        }
        else if (Input.GetButtonDown("Throw") && whipping)
        {
            whipping = false;
            if(extending)
            {
                extending = false;
            }

            head.transform.position = transform.position; 
        }

        if (whipping)
        {
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

        if(extending && dist < 1)
        {
            Debug.Log("Coil Over Extended");
            extending = false;
        }

        if (!extending)
        {
            dist = Vector3.Distance(head.transform.position, transform.position);

            head.transform.position = Vector3.Lerp(head.transform.position, transform.position, whipSpeed * Time.deltaTime);

            if (dist < 0.1f)
            {
                whipping = false;
                head.transform.position = transform.position;
            }
        }

        if (lr.enabled == true)
        {
            ShowLineRenderer();
        }
    }

    void NonTargetWhip()
    {
        dist = Vector3.Distance(head.transform.position, player.forward * range);

        if (dist >= 1 && extending)
        {
            head.transform.position = Vector3.Lerp(head.transform.position, player.transform.forward * range, whipSpeed * Time.deltaTime);
        }
    }

    void TargetedWhip()
    {
        dist = Vector3.Distance(head.transform.position, target.transform.position);

        if (dist >= 1 && extending)
        {
            head.transform.position = Vector3.Lerp(head.transform.position, target.transform.position, whipSpeed + 5 * Time.deltaTime);
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
}
