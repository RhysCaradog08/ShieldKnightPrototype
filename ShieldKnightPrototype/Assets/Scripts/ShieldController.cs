using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    TargetingSystem ts;
    Animator anim;

    Rigidbody shieldRB;
    Transform shieldHoldPos;

    [Header("Throw")]
    public float throwForce;
    public GameObject target;

    [Header("Recall")]
    float lerpTime = 1f;

    [Header("Booleans")]
    public bool thrown;
    public bool hasTarget;

    // Start is called before the first frame update
    void Start()
    {
        shieldRB = GetComponent<Rigidbody>();

        shieldHoldPos = transform.parent.transform;

        ts = gameObject.transform.root.GetComponent<TargetingSystem>();

        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetButtonUp("Fire1") && !thrown)
        {
            if (hasTarget)
            {
                StartCoroutine(TargetedThrow());
            }
            else NonTargetThrow();
        }

        if(Input.GetButtonDown("Fire1") && thrown)
        {
            if(!hasTarget)
            {
                StartCoroutine(RecallShield());
            }
        }

        if (thrown)
        {
            anim.SetBool("IsThrown", true);
        }
        else anim.SetBool("IsThrown", false);
    }

    void NonTargetThrow()
    {
        thrown = true;

        shieldRB.isKinematic = false;
        shieldRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;
    }

    IEnumerator TargetedThrow()
    {
        foreach (GameObject nextTarget in ts.targets)
        {
            target = nextTarget;
            Vector3 nextTargetPos = nextTarget.transform.position;
            while (Vector3.Distance(transform.position, nextTargetPos) > 0.1f)
            {
                transform.parent = null;
                thrown = true;

                transform.position = Vector3.MoveTowards(transform.position, nextTargetPos, throwForce * Time.deltaTime);

                yield return null;
            }
        }
        target = null;
        ts.targets.Clear();
        StartCoroutine(RecallShield());
    }

    IEnumerator RecallShield()
    {
        Vector3 startPos = transform.position; //Get position of both Shield's current location the Shield Holder.
        Vector3 endPos = shieldHoldPos.position;

        Vector3 startRot = transform.eulerAngles; // Get rotation of Shield and Shield Holder.
        Vector3 endRot = shieldHoldPos.eulerAngles;

        float t = 0f;
        while (t < lerpTime) //Returns Shield to Shield Holder in one second.
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, shieldHoldPos.position, t / lerpTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, shieldHoldPos.rotation, t / lerpTime);
            yield return null;
        }

        transform.position = shieldHoldPos.position;
        transform.rotation = shieldHoldPos.rotation;

        transform.parent = shieldHoldPos;

        shieldRB.isKinematic = true;
        thrown = false;
    }
}


