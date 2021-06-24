using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    Animator anim;

    [Header("Movement")]
    public float speed;
    public Transform pivot;
    public GameObject model;
    public float rotateSpeed;
    Vector3 move;

    [Header("Jumping")]
    public float jumpSpeed = 5;
    [SerializeField] float gravity = -9.81F;
    [SerializeField] Vector3 velocity;
    public float fallMultiplier = 2;
    public float lowJumpMultiplier = 2.5f;
    bool hasJumped;
    bool canPressSpace = true;

    [Header("Shield")]
    [SerializeField] ShieldController shield;

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        anim = GetComponent<Animator>();

        shield = GameObject.FindGameObjectWithTag("Shield").GetComponent<ShieldController>();
    }

    private void Update()
    {
        if (Input.GetButtonUp("Jump") && !hasJumped)
        {
            //Debug.Log("Jump");

            canPressSpace = true;
        }

        if (cc.isGrounded)
        {
            velocity.y = 0f;

            if (Input.GetButton("Jump") && canPressSpace)
            {
                velocity.y = jumpSpeed;
                hasJumped = true;
            }

            if (hasJumped)
            {
                canPressSpace = false;
                hasJumped = false;
            }

            /*if (waitTime <= 0)
            {
                waitTime = 0;
                stopped = false;
                groundPounding = false;
            }
            else
            {
                stopped = true;
                groundPounding = true;
            }*/
        }

        if (velocity.y < 0)
        {
            velocity.y += gravity * (fallMultiplier + 1) * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            velocity.y += gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
        }

        move = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));
        move = move.normalized * speed;

        /*cc.Move(move * Time.deltaTime);
        transform.LookAt(move + transform.position);*/

        if(move.magnitude >= 0.05f)
        {
            cc.Move(move * Time.deltaTime);
            transform.LookAt(move + transform.position);
            anim.SetBool("Moving", true);
        }
        else
        {
            anim.SetBool("Moving", false);
        }

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

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
