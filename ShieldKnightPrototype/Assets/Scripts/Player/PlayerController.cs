using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Basics.ObjectPool;

public class PlayerController : MonoBehaviour
{
    public CharacterController cc;
    public Animator anim;
    Transform cam;

    [Header("Shield")]
    [SerializeField] ShieldController shield;
    [SerializeField] ProjectileShieldController projectile;
    [SerializeField] CoilShieldController coil;
    [SerializeField] WaveShieldController wave;
    TargetingSystem ts;

    [Header("Movement")]
    public float speed, moveSpeed, rotateSpeed;
    public Vector3 move, moveDir; //velocityMomentum;
    public float turnSmoothTime;
    float turnSmoothVelocity;
    public bool stopped;

    [Header("Jumping")]
    [SerializeField] float gravity, jumpSpeed;
    public Vector3 velocity;
    public float jumpHeight, timeToJumpApex, fallMultiplier, lowJumpMultiplier;
    bool hasJumped;
    [SerializeField] bool canPressSpace;

    [Header("Guard/Parry")]
    const float minButtonHold = 0.25f;
    float buttonHeldTime, stopTime;
    bool buttonHeld;
    public GameObject parryBox;

    [Header("Shield Booleans")]
    public bool hasShield, hasProjectile, hasCoil, hasWave;

    [Header("Shield Animation Booleans")]
    public bool barging, dodging, slamming;

    [Header("Projectile Animation Booleans")]
    public bool aiming;

    [Header("Coil Animation Booleans")]
    public bool grappling;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        ts = GetComponent<TargetingSystem>();

        anim = GetComponent<Animator>();

        cam = Camera.main.transform;

        shield = FindObjectOfType<ShieldController>();
        projectile = FindObjectOfType<ProjectileShieldController>();
        coil = FindObjectOfType<CoilShieldController>();
        wave = FindObjectOfType<WaveShieldController>();
    }

    private void Start()
    {
        //Movement
        speed = moveSpeed;
        turnSmoothTime = 0.1f;
        stopped = false;

        //Jumping
        gravity = -(2* jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpSpeed = Mathf.Abs(gravity) * timeToJumpApex;
        canPressSpace = true;

        //Guard/Parry
        buttonHeldTime = 0;
        buttonHeld = false;

        //Animation Booleans
        barging = false;
        dodging = false;
        slamming = false;
    }

    private void Update()
    {
        //Debug.Log("Is  Grounded " + cc.isGrounded);
        Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);

        if (shield.gameObject.activeInHierarchy)
        {
            hasShield = true;
        }
        else hasShield = false;

        if (projectile.gameObject.activeInHierarchy)
        {
            hasProjectile = true;
        }
        else hasProjectile = false;

        if (coil.gameObject.activeInHierarchy)
        {
            hasCoil = true;
        }
        else hasCoil = false;

        if (wave.gameObject.activeInHierarchy)
        {
            hasWave = true;
        }
        else hasWave = false;

        if (stopTime > 0)
        {
            stopTime -= Time.deltaTime;
            stopped = true;
        }
        else if(stopTime <= 0)
        {
            stopTime = 0;
            stopped = false;
        }

        if (Input.GetButtonUp("Jump") && !hasJumped) //Check to stop infinite jumping.
        {
            canPressSpace = true;
        }

        if (cc.isGrounded)
        {
            if (Input.GetButton("Jump") && canPressSpace)  //Sets Y position to match jumpSpeed identifies that player has performed the Jump action.
            {
                velocity.y = jumpSpeed;
                hasJumped = true;
            }

            if (hasJumped)  //Sets Jump animation and prevents player from additional jumps once the Jump action is performed.
            {
                anim.SetBool("Jumping", true);
                canPressSpace = false;
                hasJumped = false;
            }
            else anim.SetBool("Jumping", false);
        }

        if (velocity.y < 0)  //Allows for greater height to be achieved if Jump input is held.
        {
            velocity.y += gravity * (fallMultiplier + 1) * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetKey(KeyCode.Space))  //Allows for a brief Jump action to be performed.
        {
            velocity.y += gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
        }

        if (!cc.isGrounded && Input.GetButton("Guard"))
        {
            if (hasProjectile)
            {
                Debug.LogFormat("Slow Descent");
                anim.SetBool("Helicopter", true);
                velocity.y = -2;
            }
        }
        else anim.SetBool("Helicopter", false);


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        move = new Vector3(horizontal, 0, vertical).normalized;

        if (!stopped)
        {
            if (move.magnitude >= Mathf.Epsilon && !wave.isSurfing) //Orients the player to have forward orientation relevant to camera rotation.
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

            if(wave.isSurfing)
            {
                cc.Move(transform.forward.normalized * speed * (Time.deltaTime));
                transform.Rotate(0, move.x * rotateSpeed, 0);
            }
        }

        velocity.y += gravity * Time.deltaTime;

        if(cc.isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }

        /*velocity += velocityMomentum; // Applying momentum to current velocity;

        if (velocityMomentum.magnitude >= 0f) //Dampen velocity momentum.
        {
            float momentumDrag = 2.5f;

            velocityMomentum -= velocityMomentum * momentumDrag * Time.deltaTime;
        }
        if (velocityMomentum.magnitude < 0.0f)
        {
            velocityMomentum = Vector3.zero;
        }*/

        if (hasShield)
        {
            if (Input.GetButtonUp("Throw") && !shield.thrown)  //Sets Throw/Catch animation.
            {
                anim.SetTrigger("Throw");
            }
            else if (Input.GetButtonDown("Throw") && shield.thrown)
            {
                anim.SetTrigger("Catch");
            }

            if (buttonHeld) //Sets Guarding animation.
            {
                anim.SetBool("Guarding", true);
                stopped = true;
            }
            else
            {
                stopped = false;
                anim.SetBool("Guarding", false);
            }

            if (barging) //Sets Barge animation.
            {
                //Debug.Log("Is Barging");
                anim.SetBool("Barging", true);
            }
            else
            {
                anim.SetBool("Barging", false);
            }

            if (dodging)
            {
                //Debug.Log("Shield Dodging");
                anim.SetTrigger("Dodge");
            }
            else
            {
                anim.ResetTrigger("Dodge");
            }

            if (!shield.thrown)
            {
                if (Input.GetButtonDown("Guard") && cc.isGrounded)//Button is pressed down. Need to check to see if it is "held".
                {
                    buttonHeldTime = Time.timeSinceLevelLoad;
                    buttonHeld = false;
                }
                else if (Input.GetButtonUp("Guard") && cc.isGrounded)
                {
                    if (!buttonHeld && hasShield)//If button is released without being held.
                    {
                        anim.SetTrigger("Parry");
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

                if (slamming)
                {
                    anim.SetBool("Slamming", true);
                }
                else
                {
                    anim.SetBool("Slamming", false);
                }
            }
        }

        if(hasProjectile)
        {
            if(Input.GetButton("Throw"))
            {
                aiming = true;
            }

            if (aiming)
            {
                anim.SetBool("Aiming", true);
            }
            else anim.SetBool("Aiming", false);

            if (Input.GetButton("Guard") && cc.isGrounded) //Sets Guarding animation.
            {
                anim.SetBool("Guarding", true);
                stopped = true;
            }
            else
            {
                stopped = false;
                anim.SetBool("Guarding", false);
            }
        }

        if(hasCoil)
        {
            if (slamming)
            {
                Debug.Log("Coil Slamming");
                anim.SetBool("Slamming", true);
            }
            else
            {
                anim.SetBool("Slamming", false);
            }
        }

        if(hasWave)
        {
            //Perform Wave Shield features. 
            if (Input.GetButton("Guard") && cc.isGrounded) //Sets Guarding animation.
            {
                if(!wave.isSurfing)
                {
                    anim.SetBool("WaveGuard", true);
                    stopped = true;
                }
            }
            else
            {
                stopped = false;
                anim.SetBool("WaveGuard", false);
            }
        }

        if (stopped)  //Disables Character Controller to keep player in place. 
        {
            cc.enabled = false;
            speed = 0;
        }
        else
        {
            cc.enabled = true;
            speed = moveSpeed;
        }

        cc.Move(velocity * Time.deltaTime);

        speed = moveSpeed;
    }

    public void EnableParry()  ///Sets ParryBox.
    {
        parryBox.SetActive(true);
    }

    public void DisableParry()
    {
        parryBox.SetActive(false);
    }

    public void EnableThrow()  //Sets canThrow bool in ShieldController for animation event in Throw animation.
    {
        shield.canThrow = true;
    }

    /*void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, bargeDist);
    }*/
}
