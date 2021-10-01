using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Basics.ObjectPool;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    Animator anim;
    Transform cam;

    [Header("Shield")]
    [SerializeField] ShieldController shield;
    TargetingSystem ts;
    public bool hasShield;

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
    [SerializeField] bool stopped;

    [Header("Jumping")]
    public float jumpSpeed = 5;
    [SerializeField] float gravity = -9.81F;
    [SerializeField] Vector3 velocity;
    public float fallMultiplier = 2;
    public float lowJumpMultiplier = 2.5f;
    bool hasJumped;
    [SerializeField] bool canPressSpace = true;

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
    [SerializeField]Collider[] targets;
    [SerializeField] List<GameObject> bargeTargets = new List<GameObject>();
    [SerializeField] float bargeDist;
    public GameObject closest = null;
    GameObject marker;


    [Header("Slam")]
    public bool slamming;
    public float slamDelay;
    public float slamForce;
    [SerializeField] float waitTime;
    //public GameObject gpSphere;

    [Header("Dodging")]
    public bool canDodge;
    bool isDodging;
    public float dodgeTime;
    public float dodgeSpeed;
    float dodgeDelay;


    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        anim = GetComponent<Animator>();

        cam = Camera.main.transform;

        shield = GameObject.FindGameObjectWithTag("Shield").GetComponent<ShieldController>();

        ts = GetComponent<TargetingSystem>();
    }

    private void Start()
    {
        speed = moveSpeed;
    }

    private void Update()
    {
        if (Input.GetButtonUp("Jump") && !hasJumped) //Check to stop infinite jumping.
        {
            canPressSpace = true;
        }

        if (cc.isGrounded)
        {
            velocity.y = 0f;  //Reverts Players Y position to zero if grounded.

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

            if (waitTime <= 0)  //Resets player being immobile once grounded after Slam action is performed.
            {
                waitTime = 0;
                stopped = false;
                slamming = false;
            }
            else //Whilst waitTime > 0 player is immobile.
            {
                stopped = true;
                slamming = true;
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

        if (!cc.isGrounded && Input.GetButtonDown("Fire2"))  //Input to perform Slam action.
        {
            waitTime = 0.5f;
            slamming = true;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        move = new Vector3(horizontal, 0, vertical).normalized;

        if (move.magnitude >= Mathf.Epsilon) //Orients the player to have forward orientation relevant to camera rotation.
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

        bargeDelay -= Time.deltaTime;
        dodgeDelay -= Time.deltaTime;

        if (shield.transform.root == this.transform)
        {
            hasShield = true;
        }
        else hasShield = false;

        if (Input.GetButtonUp("Fire1") && !shield.thrown)  //Sets Throw/Catch animation.
        {
            anim.SetTrigger("Throw");
        }
        else if (Input.GetButtonDown("Fire1") && shield.thrown)
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

        if (stopped)  //Disables Character Controller to keep player in place. 
        {
            cc.enabled = false;
        }
        else cc.enabled = true;

        cc.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Fire3")) //Input to perform Barge/Dodge action.
        {
            if (hasShield)
            {
                if (canBarge && bargeDelay <= 0)
                {
                    StartCoroutine(Barge());
                }
            }
            else if(!hasShield)
            {
                if(canDodge && dodgeDelay <= 0)
                {
                    StartCoroutine(Dodge());
                }
            }
        }

        if (isBarging) //Sets Barge animation.
        {
            anim.SetBool("Barging", true);
            shield.isBarging = true;
        }
        else
        {
            anim.SetBool("Barging", false);
            shield.isBarging = false;
        }

        if(isDodging)
        {
            anim.SetTrigger("Dodge");
        }
        else
        {
            anim.ResetTrigger("Dodge");
        }

        speed = moveSpeed;
        canBarge = true;
        isBarging = false;

        canDodge = true;
        isDodging = false;

        if (slamming)
        {
            StartCoroutine(Slam());

            anim.SetBool("Slamming", true);

            //gpSphere.SetActive(true);
        }
        else
        {
            anim.SetBool("Slamming", false);

           // gpSphere.SetActive(false);
        } 
        if (cc.isGrounded && slamming)
        {
            waitTime -= Time.deltaTime;
        }

        if (!shield.thrown)
        {
            if (Input.GetButtonDown("Fire2") &&  cc.isGrounded)//Button is pressed down. Need to check to see if it is "held".
            {
                buttonHeldTime = Time.timeSinceLevelLoad;
                buttonHeld = false;
            }
            else if (Input.GetButtonUp("Fire2") && cc.isGrounded)
            {
                if (!buttonHeld)//If button is released without being held.
                {                  
                    anim.SetTrigger("Parry");
                }
                buttonHeld = false;
            }

            if (Input.GetButton("Fire2") && cc.isGrounded)
            {
                if (Time.timeSinceLevelLoad - buttonHeldTime > minButtonHold)//Button is considered "held" if it is actually held down.
                {
                    buttonHeld = true;
                }
            }
        }
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

    IEnumerator Barge()
    {
        float startTime = Time.time;

        new WaitForSeconds(1); //Prevents player from stacking Barges.

        if(closest != null)
        {
            Vector3 markerPos = closest.transform.position;
            marker = ObjectPoolManager.instance.CallObject("TargetMarker", closest.transform, new Vector3(markerPos.x, markerPos.y + 2, markerPos.z - 0.65f), Quaternion.identity);
        }

        while (Time.time < startTime + bargeTime)  //Player movement speed is disabled then moved by bargeSpeed over bargeTime;
        {
            isBarging = true;
            //trailEffect.SetActive(true);
            speed = 0;

            canBarge = false;

            if(ts.lockedOn)
            {
                Vector3 closestDir = (ts.closest.transform.position - transform.position).normalized;
                Vector3 closestRot = Vector3.RotateTowards(transform.forward, closestDir, 10, 0);
                transform.rotation = Quaternion.LookRotation(closestRot);

                cc.Move(closestDir * bargeSpeed * Time.deltaTime);
            }
            else cc.Move(moveDir * bargeSpeed * Time.deltaTime);

            bargeDelay = 0.5f;

            yield return null;
        }

        if(marker != null)
        {
            ObjectPoolManager.instance.RecallObject(marker);
        }

        closest = null;
    }

    IEnumerator Slam() //Player movement is frozen then directed down by slamForce.
    {
        stopped = true;

        yield return new WaitForSeconds(slamDelay);
        velocity.y = slamForce;

        stopped = false;
    }

    public GameObject FindClosestTarget()
    {
        targets = Physics.OverlapSphere(transform.position, bargeDist);
        
        foreach (Collider col in targets)
        {
            if(col.gameObject.tag == "Target")
            {
                if(!bargeTargets.Contains(col.gameObject))
                {
                    bargeTargets.Add(col.gameObject);
                }
            }
        }

        bargeTargets.Sort(delegate (GameObject a, GameObject b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(transform.position, b.transform.position));
        });

        closest = null;
        float distance = 100;
        Vector3 position = transform.position;

        foreach (GameObject go in bargeTargets)
        {
            Debug.DrawLine(transform.position, go.transform.position, Color.red);

            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                Debug.DrawLine(transform.position, go.transform.position, Color.green);

                closest = go;
                distance = curDistance;
            }
        }

        bargeTargets.Clear();
        Array.Clear(targets, 0, targets.Length);

        return closest;
    }

    IEnumerator Dodge()
    {
        float startTime = Time.time;

        new WaitForSeconds(1); //Prevents player from stacking Dodges.

        while (Time.time < startTime + bargeTime)  //Player movement speed is disabled then moved by dodgeSpeed over dodgeTime;
        {
            isDodging = true;
            //trailEffect.SetActive(true);
            speed = 0;

            canDodge = false;
            
            cc.Move(moveDir * dodgeSpeed * Time.deltaTime);

            dodgeDelay = 0.5f;

            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, bargeDist);
    }
}
