using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldKnightController : MonoBehaviour
{
    AnimationController animControl;
    ShieldController shield;

    Camera cam;
    Transform camPos;

    [Header("Movement")]
    CharacterController cc;
    public float speed, moveSpeed, rotateSpeed, stopTime;
    public Vector3 move, moveDir;
    public float turnSmoothTime;
    float turnSmoothVelocity;
    public bool canMove;

    [Header("Button Press Check")]
    const float minButtonHold = 0.25f;
    float buttonHeldTime;
    bool buttonHeld;

    [Header("Shield Booleans")]
    public bool hasShield;


    [Header("Animation Booleans")]
    public bool isJumping, isThrowing, isBarging, isGuarding;

    private void Awake()
    {
        animControl = FindObjectOfType<AnimationController>();

        cc = GetComponent<CharacterController>();        
        cam = Camera.main;
        camPos = cam.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Movement
        speed = moveSpeed;
        turnSmoothTime = 0.1f;
        canMove = true;

        //Action Booleans
        isJumping = false;
        isThrowing = false;
        isBarging = false;
        isGuarding = false; 
    }

    // Update is called once per frame
    void Update()
    {
        if(stopTime > 0) 
        {
            stopTime -= Time.deltaTime;
            canMove = false;
        }
        if(stopTime <= 0)
        {
            stopTime = 0;
            canMove = true;   
        }

        InputCheck();

        SetCurrentAnimation();

        if (isJumping || isThrowing || isBarging || isGuarding)
        {
            canMove = false;
        }
        else canMove = true;

        if (canMove)
        {
            if (move.magnitude >= Mathf.Epsilon) //Orients the player to have forward orientation relevant to camera rotation.
            {
                float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + camPos.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                cc.Move(moveDir.normalized * speed * Time.deltaTime);
                animControl.ChangeAnimationState(animControl.move);
            }
            else animControl.ChangeAnimationState(animControl.idle);
        }
    }

    void InputCheck()
    {

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        move = new Vector3(moveX, 0, moveZ).normalized;


        if (Input.GetButton("Jump"))
        {
            isJumping = true;
        }
        else isJumping = false;

        if (Input.GetButtonDown("Throw"))
        {
            if(!isThrowing) 
            {
                isThrowing = true;
                //stopTime = animControl.anim.GetCurrentAnimatorStateInfo(0).length;
            }
        }

        if (Input.GetButton("Barge"))
        {
            isBarging = true;
        }
        else isBarging = false;

        if (Input.GetButtonDown("Guard") && cc.isGrounded)//Button is pressed down. Need to check to see if it is "held".
        {
            buttonHeldTime = Time.timeSinceLevelLoad;
            buttonHeld = false;
        }
        else if (Input.GetButtonUp("Guard") && cc.isGrounded)
        {
            if (!buttonHeld)//If button is released without being held.
            {
                stopTime = 0.5f;
            }
            buttonHeld = false;
        }

        if (Input.GetButton("Guard") && cc.isGrounded)
        {
            if (Time.timeSinceLevelLoad - buttonHeldTime > minButtonHold)//Button is considered "held" if it is actually held down.
            {
                buttonHeld = true;
            }
        }
    }

    void SetCurrentAnimation()
    {
        if (isJumping)
        {
            animControl.ChangeAnimationState(animControl.jump);
        }

        if (isThrowing)
        {
            animControl.ChangeAnimationState(animControl.throwing);
        }

        if (isBarging)
        {
            animControl.ChangeAnimationState(animControl.barge);
        }

        if (isGuarding)
        {
            animControl.ChangeAnimationState(animControl.guard);
        }
    }
}
