using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    GameObject player;

    Rigidbody shieldRB;

    Transform shieldHoldPos;

    public float throwForce;

    float lerpTime = 1f;

    public bool thrown;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.gameObject;

        shieldRB = GetComponent<Rigidbody>();

        shieldHoldPos = transform.parent.transform;
    }

    // Update is called once per frame
    void Update()
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
        shieldRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);

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
}

