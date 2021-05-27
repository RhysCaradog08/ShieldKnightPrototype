using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    GameObject player;
    PlayerController pc;
    Rigidbody shieldRB;
    Transform shieldHoldPos;

    [Header("Throw")]
    public float throwForce;
    public Transform target;


    [Header("Recall")]
    float lerpTime = 1f;

    [Header("Booleans")]
    public bool thrown;
    public bool hasTarget;
    bool hitTarget;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.gameObject;
        pc = player.GetComponent<PlayerController>();

        shieldRB = GetComponent<Rigidbody>();

        shieldHoldPos = transform.parent.transform;
    }

    private void Update()
    {
        //Debug.Log("Has Target " + hasTarget);
        //Debug.Log("Thrown " + thrown);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(hitTarget)
        {
            StartCoroutine(RecallShield());
        }

        if (Input.GetButtonDown("Fire1"))
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

        if (hasTarget)
        {
            Vector3 throwDirection = (target.position - transform.position).normalized;

            shieldRB.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }
        else
        {
            shieldRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        transform.parent = null;

        thrown = true;
    }

    IEnumerator RecallShield()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = shieldHoldPos.position;

        Vector3 startRot = transform.eulerAngles;
        Vector3 endRot = shieldHoldPos.eulerAngles;

        float t = 0f;
        while (t < lerpTime)
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Shield Hit");
        if(collision.gameObject.transform == target)
        {
            Debug.Log("Hit Target");
            hitTarget = true;
            pc.targetsLeft -= 1;
        }
    }
}


