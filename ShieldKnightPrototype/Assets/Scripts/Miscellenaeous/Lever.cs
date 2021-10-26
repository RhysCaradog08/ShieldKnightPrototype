using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    Animator anim;

    public GameObject obj;

    public Vector3 onDest;
    public Vector3 offDest;
    public float speed;
    float changeDelay = 1;

    public bool isOn = false;
    public bool canChange = true;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canChange)
        {
            changeDelay -= Time.deltaTime;
        }

        if (changeDelay <= 0)
        {
            canChange = true;
            changeDelay = 1;
        }

        float t = speed * Time.deltaTime;

        if (isOn)
        {
            anim.SetBool("On", true);
            obj.transform.position = Vector3.Lerp(obj.transform.position, onDest, Mathf.SmoothStep(0, 1, t));
        }
        else if (!isOn)
        {
            anim.SetBool("On", false);
            obj.transform.position = Vector3.Lerp(obj.transform.position, offDest, Mathf.SmoothStep(0, 1, t));
        }
    }

    public void ChangeLever()
    {
        canChange = false;

        if (isOn)
        {
            isOn = false;
        }
        else if (!isOn)
        {
            isOn = true;
        }
    }
}
