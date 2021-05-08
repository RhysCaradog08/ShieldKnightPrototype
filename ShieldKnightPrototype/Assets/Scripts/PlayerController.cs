using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;

    Vector3 move;

    public float speed;

    public GameObject shield;
    public Transform shieldHoldPos;
    public float throwForce;
    public float recallSpeed;

    bool thrown;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        cc.Move(move * Time.deltaTime * speed);
        transform.LookAt(move + transform.position);

        if (Input.GetButtonDown("Fire1"))
        {
            if(thrown)
            {
                if(CheckDist() >= .1f)
                {
                    RecallShield();
                }
                else if(CheckDist() <= .1f)
                {

                }
                
            }
            else ThrowShield();
        }
    }

    public float CheckDist()
    {
        float dist = Vector3.Distance(shield.transform.position, shieldHoldPos.transform.position);
        return dist;
    }

    void ThrowShield()
    {
        Rigidbody shieldRB = shield.GetComponent<Rigidbody>();

        shieldRB.isKinematic = false;
        shieldRB.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        shield.transform.parent = null;

        thrown = true;
    }

    void RecallShield()
    {
        shield.transform.position = Vector3.Lerp(shield.transform.position, shieldHoldPos.position, recallSpeed);
        shield.transform.rotation = shieldHoldPos.rotation;

        shield.transform.parent = shieldHoldPos;

        Rigidbody shieldRB = shield.GetComponent<Rigidbody>();
        shieldRB.isKinematic = true;

        thrown = false;
    }
}
