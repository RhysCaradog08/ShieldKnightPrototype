using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    Animator anim;
    Transform cam;

    [Header("Movement")]
    float speed;
    public float moveSpeed;
    public Transform pivot;
    public GameObject model;
    public float rotateSpeed;
    Vector3 move;
    Vector3 moveDir;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    bool stopped;

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

    [Header("Guard/Parry")]
    const float minButtonHold = 0.25f;
    float buttonHeldTime = 0f;
    bool buttonHeld = false;
    public GameObject parryBox;

    [Header("Barging")]
    public bool canBarge;
    [SerializeField] bool isBarging;
    public float bargeTime;
    public float bargeSpeed;
    float bargeDelay;

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        anim = GetComponent<Animator>();

        cam = Camera.main.transform;

        shield = GameObject.FindGameObjectWithTag("Shield").GetComponent<ShieldController>();

        speed = moveSpeed;
    }

    private void Update()
    {
        if (Input.GetButtonUp("Jump") && !hasJumped)
        {
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

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        move = new Vector3(horizontal, 0, vertical).normalized;

        if (move.magnitude >= Mathf.Epsilon)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            cc.Move(moveDir.normalized * speed * Time.deltaTime);

            anim.SetBool("Moving", true);
        }
        else
        {
            anim.SetBool("Moving", false);
        }

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        bargeDelay -= Time.deltaTime;

        /*if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
            Quaternion newRotation = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
        }*/

        if (Input.GetButtonUp("Fire1") && !shield.thrown)
        {
            anim.SetTrigger("Throw");
        }
        else if (Input.GetButtonDown("Fire1") && shield.thrown)
        {
            anim.SetTrigger("Catch");
        }

        if (buttonHeld)
        {
            stopped = true;
            anim.SetBool("Guarding", true);
        }
        else
        {
            stopped = false;
            anim.SetBool("Guarding", false);
        }

        if (stopped)
        {
            cc.enabled = false;
        }
        else cc.enabled = true;

        if(Input.GetButtonDown("Fire3"))
        {
            Debug.Log("Pressed Middle Mouse Button");

            if (canBarge && bargeDelay <= 0)
            {
                StartCoroutine(Barge());
            }
        }

        if (isBarging)
        {
            anim.SetBool("Barging", true);
        }
        else anim.SetBool("Barging", false);

        speed = moveSpeed;
        canBarge = true;
        isBarging = false;

        if (!shield.thrown)
        {
            if (Input.GetButtonDown("Fire2"))//Button is pressed down. Need to check to see if it is "held".
            {
                buttonHeldTime = Time.timeSinceLevelLoad;
                buttonHeld = false;
            }
            else if (Input.GetButtonUp("Fire2"))
            {
                if (!buttonHeld)//If button is released without being held.
                {                  
                    anim.SetTrigger("Parry");
                }
                buttonHeld = false;
            }

            if (Input.GetButton("Fire2"))
            {
                if (Time.timeSinceLevelLoad - buttonHeldTime > minButtonHold)//Button is considered "held" if it is actually held down.
                {
                    buttonHeld = true;
                }
            }
        }
    }

    public void EnableParry()
    {
        parryBox.SetActive(true);
    }

    public void DisableParry()
    {
        parryBox.SetActive(false);
    }

    public void EnableThrow()
    {
        shield.canThrow = true;
    }

    IEnumerator Barge()
    {
        float startTime = Time.time;

        new WaitForSeconds(1);

        while (Time.time < startTime + bargeTime)
        {
            isBarging = true;
            //trailEffect.SetActive(true);
            speed = 0;

            canBarge = false;
            cc.Move(moveDir * bargeSpeed * Time.deltaTime);
            bargeDelay = 0.5f;

            yield return null;
        }
    }
}
