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
    public float speed, moveSpeed, rotateSpeed;
    public Vector3 move, moveDir;
    public float turnSmoothTime;
    float turnSmoothVelocity;
    public bool canMove;

    //Action Booleans
    [SerializeField] bool isJumping, isThrowing, isBarging, isGuarding;

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
        InputCheck();

        if (isJumping || isThrowing || isBarging || isGuarding)
        {
            canMove = false;
        }
        else canMove = true;

        if(isJumping)
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

        if (Input.GetButton("Throw"))
        {
            isThrowing = true;
        }
        else isThrowing = false;   

        if (Input.GetButton("Barge"))
        {
            isBarging = true;
        }
        else isBarging = false;

        if (Input.GetButton("Guard"))
        {
            isGuarding = true;
        }
        else isGuarding = false;
    }
}
