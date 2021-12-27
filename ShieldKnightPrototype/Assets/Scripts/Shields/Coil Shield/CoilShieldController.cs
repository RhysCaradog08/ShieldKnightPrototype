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

    [Header("Coil Scale")]
    [SerializeField] Vector3 startScale;
    [SerializeField] float minScale;


    // Start is called before the first frame update
    void Start()
    {
        ts = FindObjectOfType<TargetingSystem>();

        lr = GetComponent<LineRenderer>();

        whipping = false;

        startScale = coil.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Throw") && !whipping)
        {
            whipping = true;
        }

        if(whipping)
        {
            head.SetActive(true);
            lr.enabled = true;

            head.transform.Translate(transform.forward * whipSpeed * Time.deltaTime);

            StartCoroutine(ShrinkCoil());
            CalculateDistance();
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

    void CalculateDistance()
    {
        dist = Vector3.Distance(transform.position, head.transform.position);

        if (dist >= range)
        {
            head.transform.position = transform.position;
            whipping = false;
        }

        if (dist >= range)
        {
            whipping = false;
            head.transform.position = transform.position;
        }
    }

    void ShowLineRenderer()
    {
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, head.transform.position);
    }

    IEnumerator ShrinkCoil()
    {
        while (minScale < coil.transform.localScale.x)
        {
            coil.transform.localScale -= startScale * Time.deltaTime * dist/whipSpeed;
            yield return null;
        }
    }
}
