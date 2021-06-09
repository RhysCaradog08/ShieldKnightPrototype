using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    Animator anim;

    [Header("Movement")]
    Vector3 move;
    public float speed;
    public Transform pivot;
    public GameObject model;
    public float rotateSpeed;

    [Header("Shield")]
    ShieldController shield;

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        anim = GetComponent<Animator>();

        shield = GameObject.FindGameObjectWithTag("Shield").GetComponent<ShieldController>();
    }

    private void Update()
    {
        move = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));
        move = move.normalized * speed;

        cc.Move(move * Time.deltaTime);
        transform.LookAt(move + transform.position);

        if(move.magnitude >= 0.05f)
        {
            anim.SetBool("Moving", true);
        }
        else
        {
            anim.SetBool("Moving", false);
        }

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
            Quaternion newRotation = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if(!shield.thrown)
            {
                anim.SetTrigger("Throw");
            }
        }
    }
}
