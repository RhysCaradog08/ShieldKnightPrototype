using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoilShieldController : MonoBehaviour
{
    TargetingSystem ts;

    public GameObject coil;
    public GameObject head;
    LineRenderer lr;

    [Header("Whip")]
    [SerializeField] float whipSpeed;
    [SerializeField] float range;
    [SerializeField] float dist;
    [SerializeField] bool whipping;
    [SerializeField] bool extending;


    [Header("Coil Scale")]
    [SerializeField] Vector3 startScale;
    [SerializeField] float minScale;


    // Start is called before the first frame update
    void Start()
    {
        ts = FindObjectOfType<TargetingSystem>();

        lr = GetComponent<LineRenderer>();

        whipping = false;
        extending = false;

        startScale = coil.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Throw") && !whipping)
        {
            whipping = true;
            extending = true;
        }

        if(whipping)
        {
            head.SetActive(true);
            lr.enabled = true;

            StartCoroutine(ScaleCoil());
            NonTargetWhip();
        }
        else
        {
            head.SetActive(false);
            lr.enabled = false;

            coil.transform.localScale = startScale;
        }

        if (lr.enabled == true)
        {
            ShowLineRenderer();
        }
    }

    void NonTargetWhip()
    {
        dist = Vector3.Distance(transform.position, head.transform.position);

        if (dist < range && extending)
        {
            head.transform.Translate(transform.forward * whipSpeed * Time.deltaTime);
        }

        if (dist >= range)
        {
            extending = false;
        }

        if(!extending)
        {
            head.transform.Translate(-transform.forward * (whipSpeed * 2) * Time.deltaTime);

            if(dist < 1)
            {
                whipping = false;
                head.transform.position = transform.position;
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
