using Basics.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldKnightController : MonoBehaviour
{
    PlayerManager pm;
    public AnimationController animControl;

    Camera cam;
    Transform camPos;

    [Header("Movement")]
    public CharacterController cc;
    public float speed, moveSpeed, rotateSpeed, stopTime;
    public Vector3 move, moveDir;
    public float turnSmoothTime;
    float turnSmoothVelocity;
    public bool canMove;

    [Header("Jumping")]
    [SerializeField] float gravity;
    public Vector3 velocity;
    public float jumpHeight, jumpSpeed, timeToJumpApex, lowJumpMultiplier;
    public bool hasJumped, isJumping, canPressSpace;

    [Header("Button Held Check")]
    const float minButtonHold = 0.25f;
    float buttonHeldTime;
    public bool buttonHeld;

    [Header("Slam")]
    public float slamForce, distToGround, slamPushBack, slamRadius, slamLift, damageDelay;
    public LayerMask ignoreLayer;
    GameObject slamStars;
    Transform squashedObject;
    [SerializeField] List<Transform> slamObjects = new List<Transform>();
    public bool showSlamVFX;

    [Header("Action Booleans")]
    public bool isMoving, isThrowing, isBarging, isGuarding, isParrying, isSlamming, parachuteOpen;

    private void Awake()
    {
        pm = FindObjectOfType<PlayerManager>();
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

        //Jumping
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpSpeed = Mathf.Abs(gravity) * timeToJumpApex;
        hasJumped = false;
        isJumping = false;
        canPressSpace = true;
        isJumping = false;

        //Action Booleans
        isThrowing = false;
        isBarging = false;
        isGuarding = false;
        isParrying = false;
        isSlamming = false;
        parachuteOpen = false;
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

            if(isThrowing)
            {
                isThrowing = false;
            }
            
            if(isParrying)
            {
                isParrying = false;
            }

            if(isSlamming && cc.isGrounded)
            {
                isSlamming = false;
            }

            if(pm.shield.shieldSlamming && cc.isGrounded)
            {
                pm.shield.shieldSlamming = false;
            }
        }

        InputCheck();

        SetCurrentAnimation();

        if (velocity.y > 0 && !Input.GetButton("Jump"))  //Allows for a brief Jump action to be performed.
        {
            velocity.y += gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
        }

        if (cc.isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }
        else velocity.y += gravity * Time.deltaTime;

        if (velocity.y > 0)
        {
            isJumping = true;
        }
        else if (velocity.y < 0)
        {
            isJumping = false;
        }

        if(pm.scrapBag.parachuteOpen)
        {
            velocity.y = pm.scrapBag.descentSpeed;
            animControl.ChangeAnimationState(animControl.parachute);
        }

        if (isThrowing || isBarging || isGuarding || isParrying || isSlamming)
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

                if (!isJumping && !pm.scrapBag.isAiming && !pm.scrapBag.parachuteOpen)
                {
                    animControl.ChangeAnimationState(animControl.move);
                }
            }
            else if (!isJumping && cc.isGrounded)
            {
                if (!pm.scrapBag.isAiming && !pm.shield.shieldSlamming)
                {
                    animControl.ChangeAnimationState(animControl.idle);
                }
            }
        }

        if (isSlamming)
        {
            canPressSpace = false;
            Slam();
        }
        else
        {
            showSlamVFX = false;

            if (slamObjects.Count > 0)
            {
                foreach (Transform so in slamObjects)
                {
                    so.transform.localScale = Vector3.one;
                }

                slamObjects.Clear();
            }

            canPressSpace = true;
        }

        cc.Move(velocity * Time.deltaTime);

        speed = moveSpeed;
    }

    void InputCheck()
    {
        if (Input.GetButtonUp("Jump") && !hasJumped) //Check to stop infinite jumping.
        {
            canPressSpace = true;
        }

        if (cc.isGrounded)
        {
            if (Input.GetButtonDown("Jump") && canPressSpace)
            {
                Jump();
            }

            if (hasJumped)  //Sets Jump animation and prevents player from additional jumps once the Jump action is performed.
            {
                SetJumpBoolsToFalse();
            }
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        move = new Vector3(moveX, 0, moveZ).normalized;


        if (Input.GetButtonUp("Throw"))
        {
            if (!pm.hasScrapBag)
            {
                if (!isBarging || !isGuarding || !isParrying || !isSlamming)
                {
                    if (!isThrowing)
                    {
                        isThrowing = true;
                        stopTime = 0.5f;
                    }
                }
            }

            if(pm.hasScrapBag)
            {
                if(pm.scrapBag.swipingBag && !isThrowing)
                {
                    Debug.Log("Swipe Bag");
                    isThrowing = true;
                    stopTime = 0.5f;
                }
            }
        }

        if (!isSlamming)
        {
            if (Input.GetButtonDown("Guard")) //&& cc.isGrounded)//Button is pressed down. Need to check to see if it is "held".
            {
                if (cc.isGrounded && stopTime <= 0)
                {
                    buttonHeldTime = Time.timeSinceLevelLoad;
                    buttonHeld = false;
                }
            }
            else if (Input.GetButtonUp("Guard")) // && cc.isGrounded)
            {
                if (!buttonHeld && cc.isGrounded)//If button is released without being held.
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
    }

    void Jump()
    {
        velocity.y = jumpSpeed;
        hasJumped = true;
    }

    void SetJumpBoolsToFalse()
    {
        canPressSpace = false;
        hasJumped = false;
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

        if (isParrying) 
        {
            animControl.ChangeAnimationState(animControl.parry);
        }

        if(isSlamming)
        {
            animControl.ChangeAnimationState(animControl.slam);
        }
    }

    void Slam()
    {
        velocity.y = -slamForce;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity, ignoreLayer))
        {
            Debug.Log(hit.transform.name);
            distToGround = hit.distance;

            Debug.DrawLine(transform.position, -transform.up * 10, Color.red);
            //Debug.Log("Distance to Ground: " + distToGround);

            if (distToGround < 5)
            {
                Debug.DrawLine(transform.position, -transform.up * 10, Color.green);

                if (!showSlamVFX)
                {
                    slamStars = ObjectPoolManager.instance.CallObject("SlamStars", null, hit.point, Quaternion.Euler(-90, transform.rotation.y, transform.rotation.z), 1);
                    showSlamVFX = true;
                }

                Debug.Log("Hit Ground");
                SlamImpact();

                if (pm.hasShield && stopTime < 0.01f)
                {
                    stopTime = 0.5f;
                }

                if (pm.hasMushroomCap)
                {
                    pm.mushroomCap.Bounce();
                }

                if(pm.hasScrapBag && stopTime < 0.01f)
                {
                    stopTime = 0.5f;
                    //pm.scrapBag.BagSlam();
                    pm.scrapBag.inBag.Clear();
                }
            }
        }
    }

    void SlamImpact()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, slamRadius);

        foreach (Collider col in colliders)
        {
            if (col.gameObject.layer == 15)
            {
                if (!slamObjects.Contains(col.transform))
                {
                    slamObjects.Add(col.transform.GetChild(0));
                }

                Vector3 squashedSize = new Vector3(col.transform.GetChild(0).localScale.x, 0.25f, col.transform.localScale.z);
                col.transform.GetChild(0).localScale = squashedSize;

                if (col.GetComponent<Rigidbody>() != null)
                {
                    Rigidbody slamRB = col.GetComponent<Rigidbody>();
                    slamRB.AddExplosionForce(slamPushBack, transform.position, slamRadius, slamLift, ForceMode.Impulse);
                }

                /*EnemyHealth enemy = col.GetComponent<EnemyHealth>();

                if (enemy != null)
                {
                    if (damageDelay <= 0)
                    {
                        enemy.TakeDamage(10);
                    }
                }*/
            }
        }
    }
}
