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
            ThrowShield();
        }
    }

    void ThrowShield()
    {
        thrown = true;

        Rigidbody shieldRB = shield.GetComponent<Rigidbody>();

        shieldRB.isKinematic = false;
        shieldRB.AddForce(transform.forward * 100, ForceMode.Impulse);
    }
}
