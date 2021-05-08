using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    GameObject player;

    Rigidbody shieldRB;

    Transform shieldHoldPos;

    public float throwForce;
    public float recallSpeed;

    bool thrown;

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
                RecallShield();

            }
            else ThrowShield();
        }
    }

    void ThrowShield()
    {
        shieldRB.isKinematic = false;
        shieldRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;

        thrown = true;
    }

    void RecallShield()
    {
        transform.position = Vector3.Lerp(transform.position, shieldHoldPos.position, recallSpeed);
        transform.rotation = shieldHoldPos.rotation;

        transform.parent = shieldHoldPos;

        shieldRB.isKinematic = true;

        thrown = false;
    }
}
