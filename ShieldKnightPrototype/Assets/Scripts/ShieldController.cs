using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    TargetingSystem ts;

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
    bool hitTarget;

    // Start is called before the first frame update
    void Start()
    {
        shieldRB = GetComponent<Rigidbody>();

        shieldHoldPos = transform.parent.transform;

        ts = gameObject.transform.root.GetComponent<TargetingSystem>();
    }

    private void Update()
    {
    
    }

    void FixedUpdate()
    {
        if(hitTarget)
        {
            StartCoroutine(RecallShield());
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (thrown)
            {
                StartCoroutine(RecallShield());
            }
            else ThrowShield();
        }
    }

    public void ThrowShield()
    {
        shieldRB.isKinematic = false;

        if (hasTarget) //Shield will be thrown towards target.
        {
            Vector3 throwDirection = (target.transform.position - transform.position).normalized;

            shieldRB.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }
        else //Shield thrown in players forward direction.
        {
            shieldRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        transform.parent = null;

        thrown = true;
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
        hitTarget = false;

        if(ts.targets.Count > 0)
        {
            ts.targets.Clear();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Shield Hit");
        if(collision.gameObject == target)
        {
            Debug.Log("Hit Target");
            hitTarget = true;
            ts.targets.Remove(target);           
        }
    }
}


