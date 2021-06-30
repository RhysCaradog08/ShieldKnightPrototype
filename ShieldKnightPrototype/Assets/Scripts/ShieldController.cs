using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ShieldController : MonoBehaviour
{
    TargetingSystem ts;

    Rigidbody shieldRB;
    Transform shieldHoldPos;

    [Header("Throw")]
    public float throwForce;
    public GameObject target;
    public GameObject model;
    public float rotateSpeed;
    public bool thrown;
    public bool hasTarget;
    public bool canThrow;

    [Header("Recall")]
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

    [Header("Guard/Parry")]
    const float minButtonHold = 0.25f;
    float buttonHeldTime = 0f;
    bool buttonHeld = false;

    // Start is called before the first frame update
    void Start()
    {
        shieldRB = GetComponent<Rigidbody>();

        shieldHoldPos = transform.parent.transform;

        ts = transform.root.GetComponent<TargetingSystem>();

        meshCol = GetComponentInChildren<MeshCollider>();
    }

    private void Update()
    {
        if (canThrow && !thrown)
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
            canThrow = false;
        }
    }

    void NonTargetThrow()  //Throws Shield in players forward vector if no targets are identified.
    {
        thrown = true;

        shieldRB.isKinematic = false;
        shieldRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;
    }

    IEnumerator TargetedThrow()  //Throws Shield towards any identified targets in range.
    {
        thrown = true;

        foreach (GameObject nextTarget in ts.targets) //Sets nextTarget in list to be target and move shield towards target.
        {
            target = nextTarget;
            Vector3 nextTargetPos = nextTarget.transform.position;
            while (Vector3.Distance(transform.position, nextTargetPos) > 0.1f)
            {
                transform.parent = null;
                //thrown = true;

                transform.position = Vector3.MoveTowards(transform.position, nextTargetPos, throwForce * Time.deltaTime);

                yield return null;
            }
        }
        target = null;
        ts.targets.Clear();
        StartCoroutine(RecallShield());
    }

    IEnumerator RecallShield()  //Recalls Shield back to Shield Holder.
    {
        shieldRB.isKinematic = false;

        Vector3 startPos = transform.position; //Get position of both Shield's current location the Shield Holder.
        Vector3 endPos = shieldHoldPos.position;

        Vector3 startRot = transform.eulerAngles; // Get rotation of Shield and Shield Holder.
        Vector3 endRot = shieldHoldPos.eulerAngles;

        float t = 0f;
        while (t < lerpTime) //Returns Shield to Shield Holder over the course of 1 second.
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, shieldHoldPos.position, t / lerpTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, shieldHoldPos.rotation, t / lerpTime);

            meshCol.enabled = false;

            yield return null;
        }

        transform.parent = shieldHoldPos;
        transform.localPosition = Vector3.zero;

        meshCol.enabled = true;

        shieldRB.isKinematic = true;
        thrown = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (thrown)
        {
            if (other.CompareTag("Target"))
            {
                GameObject pMarker = other.gameObject.transform.GetChild(0).gameObject;

                ObjectPoolManager.instance.RecallObject(pMarker);
            }

            if (other.CompareTag("Sticky"))
            {
                shieldRB.isKinematic = true;
            }
        }
    }
}


