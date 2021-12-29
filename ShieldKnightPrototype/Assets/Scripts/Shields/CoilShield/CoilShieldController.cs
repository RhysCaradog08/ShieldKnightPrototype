using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoilShieldController : MonoBehaviour
{
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
            dist = Vector3.Distance(transform.position, head.transform.position);

            head.SetActive(true);
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
            head.SetActive(false);
            lr.enabled = false;

            coil.transform.localScale = startScale;
        }

        if(dist >= range)
        {
            extending = false;
        }

        if (!extending)
        {
            head.transform.Translate(-transform.forward * (whipSpeed * 2) * Time.deltaTime);

            if (dist < 1)
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
        if (dist < range && extending)
        {
            head.transform.Translate(transform.forward * whipSpeed * Time.deltaTime);
        }
    }

    void TargetedWhip()
    {
        Vector3 dir = target.transform.position - transform.position.normalized;

        if (dist < range && extending)
        {
            head.transform.Translate(dir * whipSpeed * Time.deltaTime);
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
