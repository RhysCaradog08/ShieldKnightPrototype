using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;

    Animator anim;

    Vector3 move;

    public float speed;

    public ShieldController shield;

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        cc.Move(move * Time.deltaTime * speed);
        transform.LookAt(move + transform.position);

        if (Input.GetButtonDown("Fire1"))
        {
            if(!shield.thrown)
            {
                //anim.SetTrigger("Throw");
            }
        }
    }

}
