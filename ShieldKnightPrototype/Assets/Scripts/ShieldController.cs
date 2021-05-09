using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    GameObject player;
    Rigidbody shieldRB;
    Transform shieldHoldPos;

    [Header("Throw")]
    public float throwForce;
    public Transform target;
    public float throwRange;
    GameObject[] gos;

    [Header("Recall")]
    float lerpTime = 1f;

    [Header("Booleans")]
    public bool thrown;
    bool hasTarget;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.gameObject;

        shieldRB = GetComponent<Rigidbody>();

        shieldHoldPos = transform.parent.transform;
    }

    private void Update()
    {
        FindTargets();

        Debug.Log("Has Target " + hasTarget);

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance < throwRange)
        {
            hasTarget = true;
        }
        else hasTarget = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
    }

    void FindTargets()
    {
        gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];

        foreach (GameObject go in gos)
        {
            if (go.layer == 8)
            {
                Debug.Log(go.name);
                target = go.transform;
            }
        }
    }
}


