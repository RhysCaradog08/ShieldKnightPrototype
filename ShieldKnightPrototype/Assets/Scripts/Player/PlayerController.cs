using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Wave Shield Actions")]
    [SerializeField] int attackCount;
    [SerializeField] float attackReset;
    public float surfSpeed;
    public float attackDelay;
    public bool canSurf;

    [Header("Shield Booleans")]
    public bool hasShield, hasProjectile, hasCoil, hasWave;

    [Header("Shield Animation Booleans")]
    public bool barging, dodging, slamming;

    [Header("Projectile Animation Booleans")]
    public bool aiming;

    [Header("Coil Animation Booleans")]
    public bool grappling, canSpring;

    [Header("Wave Animation Booleans")]
    public bool waveGuarding;

    [Header("Environmental Actions")]
    public bool inWater;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        ts = GetComponent<TargetingSystem>();

        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }

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

        //Wave Shield Actions
        attackCount = 3;
        surfSpeed = 25;

        //Shield Animation Booleans
        barging = false;
        dodging = false;
        slamming = false;

        //Projectile Animation Booleans
        aiming = false;

        //Coil Animation Booleans
        grappling = false;

        //Wave Animation Booleans
        waveGuarding = false;

        //Environmental Actions
        inWater = false;
    }

    private void Update()
    {
        //Debug.Log("Is  Grounded " + cc.isGrounded);

        if (shield.gameObject.activeInHierarchy) //Player is using Standard Shield.
        {
            hasShield = true;
        }
        else hasShield = false;

        if (projectile.gameObject.activeInHierarchy) //Player is using Projectile Shield.
        {
            hasProjectile = true;
        }
        else hasProjectile = false;

        if (coil.gameObject.activeInHierarchy) //Player is using Coil Shield.
        {
            hasCoil = true;
        }
        else hasCoil = false;

        if (wave.gameObject.activeInHierarchy) //Player is using Wave Shield.
        {
            hasWave = true;
        }
        else hasWave = false;

        if (stopTime > 0) //Decrease stopTime whilst stopping player in place.
        {
            stopTime -= Time.deltaTime;
            stopped = true;
        }
        else if(stopTime <= 0) //Resume players ability to move.
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

        if(wave.isGrinding)
        {
            if (Input.GetButton("Jump") && canPressSpace)  //Sets Y position to match jumpSpeed identifies that player has performed the Jump action.
            {
                velocity.y = jumpSpeed;
                hasJumped = true;
            }

            if(hasJumped)
            {
                canPressSpace = false;
                hasJumped = false;
            }
        }

        if(!canSurf && wave.isSurfing && Input.GetButtonDown("Jump")) //Allows for jumping while surfing on Surfable Surfaces.
        {
            canSurf = true;
        }

        if(canSurf && wave.isSurfing)
        {
            if (Input.GetButton("Jump") && canPressSpace)  //Sets Y position to match jumpSpeed identifies that player has performed the Jump action.
            {
                velocity.y = jumpSpeed;
                hasJumped = true;
            }

            if (hasJumped)
            {
                canPressSpace = false;
                hasJumped = false;
            }
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

            if(wave.isSurfing) //Add constant forward motion whilst player is surfing on Wave Shield.
            {
                if (!wave.isGrinding)
                {
                    cc.Move(transform.forward.normalized * surfSpeed * Time.deltaTime);
                    transform.Rotate(0, move.x * rotateSpeed, 0);
                }
            }
        }

        if(!canSurf && wave.isSurfing) //Stops player from having gravity applied while surfing on Surfable Surfaces.
        {
            velocity.y = 0;
        }
        else velocity.y += gravity * Time.deltaTime;

        if (cc.isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }

        if (hasShield)
        {
            if (Input.GetButtonUp("Throw") && !shield.thrown)  //Sets Throw/Catch animation.
            {
                if(shield.hasTarget)
                {
                    transform.LookAt(shield.target.transform);
                }
                anim.SetTrigger("Throw");

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
            if(Input.GetButton("Throw")) //Set aim animation.
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
            if (cc.isGrounded)
            {
                canSpring = true;
            }
            else canSpring = false;

            if (Input.GetButtonUp("Throw") && coil.canExtend || (Input.GetButtonUp("Barge") && coil.enableTether))
            {
                anim.SetTrigger("Whip");
            }

            if(Input.GetButtonUp("Throw") && coil.hasObject)
            {
                anim.SetTrigger("Throw");
            }

            if (!cc.isGrounded && !coil.isSpringing)
            {
                anim.SetBool("Is Falling", true);
            }
            else anim.SetBool("Is Falling", false);

            if (slamming)
            {
                anim.SetBool("Slamming", true);
            }
            else
            {
                anim.SetBool("Slamming", false);
            }
        }

        if(hasWave)
        {
            if(wave.isGrinding)
            {
                rotateSpeed = 0; //Stops player from turning whilst grinding;
            }
            else
            {
                rotateSpeed = 2.5f;
            }

            if (wave.isSurfing)
            {
                anim.SetBool("Surfing", true);
            }
            else if (!wave.isSurfing)
            {
                anim.SetBool("Surfing", false);
                
                if (Input.GetButtonDown("Throw") && attackDelay <= 0)
                {
                    if (attackCount > 0)
                    {
                        attackCount--;
                        attackDelay = 0.5f;
                        attackReset = 0.75f;
                    }

                    if (attackCount == 2) //&& attackReset > 0)
                    {
                        Debug.Log("Attack Left");
                        anim.SetTrigger("WaveAttackLeft");
                    }

                    if (attackCount == 1) //&& attackReset > 0)
                    {
                        Debug.Log("Attack Right");
                        anim.SetTrigger("WaveAttackRight");
                        anim.ResetTrigger("WaveAttackLeft");
                    }

                    if (attackCount == 0) //&& attackReset > 0)
                    {
                        Debug.Log("Attack Overhead");
                        anim.ResetTrigger("WaveAttackRight");
                        anim.SetTrigger("WaveAttackOverhead");
                    }
                }

            }

            if (attackDelay > 0)
            {
                attackDelay -= Time.deltaTime;
                stopTime = attackDelay;
            }

            if (attackDelay <= 0 && !waveGuarding)
            {
                attackDelay = 0;
                stopTime = 0;
            }

            if (attackReset > 0)
            {
                attackReset -= Time.deltaTime;
                anim.SetBool("WaveAttacking", true);
            }

            if (attackReset <= 0)
            {
                attackReset = 0;
                attackCount = 3;

                anim.SetBool("WaveAttacking", false);
                anim.ResetTrigger("WaveAttackLeft");
                anim.ResetTrigger("WaveAttackRight");
                anim.ResetTrigger("WaveAttackOverhead");
            }
            
            if (Input.GetButton("Guard") && cc.isGrounded) //Sets Guarding animation.
            {
                if (!wave.isSurfing)
                {
                    waveGuarding = true;
                    anim.SetBool("WaveGuard", true);
                    stopped = true;
                }
            }
            else
            {
                waveGuarding = false;
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

    public void EnableThrowShield()  //Sets canThrow bool in ShieldController for animation event in Throw animation.
    {
        if(hasShield)
        {
            shield.canThrow = true;
        }
    }

    public void EnableCoilExtension()
    {
        if(hasCoil)
        {
            coil.isExtending = true;
        }
    }

    void EnableThrowObject()
    {
        if(hasCoil && coil.hasObject)
        {
            coil.throwObject = true;
        }
    }

    public void WaveAttackOn() //Sets isAttacking bool in WaveShieldController to enable collision when attacking.
    {
        wave.isAttacking = true;
    }

    public void WaveAttackOff()
    {
        wave.isAttacking = false;
    }

    /*void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, bargeDist);
    }*/
}
