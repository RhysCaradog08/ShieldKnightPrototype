using Basics.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StandardShieldController : MonoBehaviour
{
    [SerializeField] ShieldKnightController sk;
    AnimationController animControl;
    [SerializeField] Rigidbody shieldRB;

    [Header("Throw")]
    public float throwForce;
    //public GameObject target;
    TrailRenderer trail;
    public bool thrown, /*hasTarget,*/ canThrow;

    [Header("Recall")]
    Transform shieldHoldPos;
    Quaternion shieldHoldRot;
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

    [Header("Barge")]
    [SerializeField] float bargeDelay;
    public float bargeTime, bargeSpeed;
    public GameObject closest;
    GameObject marker;
    public bool canBarge, isBarging;

    Vector3 shieldScale;

    private void Awake()
    {
        sk = FindObjectOfType<ShieldKnightController>();
        shieldRB = GetComponentInChildren<Rigidbody>();

        trail = GetComponent<TrailRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Throw
        thrown = false;

        //Recall
        shieldHoldPos = transform.parent.transform;
        shieldHoldRot = Quaternion.Euler(-90, 90, 0);

        //Barge
        canBarge = true;
        isBarging = false;
        closest = null;

        shieldScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = shieldScale;

        if (bargeDelay > 0)
        {
            bargeDelay -= Time.deltaTime;
        }
        if (bargeDelay <= 0)
        {
            bargeDelay = 0;

            if (!canBarge)
            {
                canBarge = true;
            }

            if(sk.isBarging)
            {
                sk.isBarging = false;
            }  
        }

        if (canThrow)
        {
            NonTargetThrow();
        }

        if (thrown)  //Stops Player repeatedly throwing the shield.
        {
            trail.enabled = true;
            canThrow = false;
            sk.isThrowing = false;
        }
        else trail.enabled = false;

        if (Input.GetButtonDown("Throw") && thrown) //If Player doesn't have possession of Shield it gets recalled to player.
        {
            StartCoroutine(RecallShield());
        }

        if (Input.GetButtonDown("Barge")) //Input to perform Barge/Dodge action.
        {
            if (!thrown)
            {
                if (canBarge && bargeDelay <= 0)
                {
                    StartCoroutine(Barge());
                    trail.enabled = true;
                }
            }
        }

        //canBarge = true;
        //sk.isBarging = false;

        if (sk.cc.isGrounded && sk.buttonHeld)
        {
            if(!thrown)
            {
                sk.isGuarding = true;
            }
        }
        else sk.isGuarding = false;
    }

    void NonTargetThrow()  //Throws Shield in players forward vector if no targets are identified.
    {
        thrown = true;

        shieldRB.isKinematic = false;
        shieldRB.AddForce(sk.transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;
    }

    IEnumerator RecallShield()  //Recalls Shield back to Shield Holder.
    {
        float t = 0f;
        while (t < lerpTime) //Returns Shield to Shield Holder over the course of 1 second.
        {
            shieldRB.isKinematic = false;

            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, shieldHoldPos.position, t / lerpTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, shieldHoldPos.rotation, t / lerpTime);
            
            meshCol.enabled = false; //Prevents from unecessary collisions upon return.

            yield return null;
        }

        shieldRB.isKinematic = true;

        transform.parent = shieldHoldPos;  //Sets Shields position and parent to stay attached to Player.
        transform.localPosition = Vector3.zero;
        transform.localRotation = shieldHoldRot;

        meshCol.enabled = true;

        thrown = false;
    }

    IEnumerator Barge()
    {
        float startTime = Time.time;

        new WaitForSeconds(1); //Prevents player from stacking Barges.

        while (Time.time < startTime + bargeTime)  //Player movement speed is disabled then moved by bargeSpeed over bargeTime;
        {
            sk.isBarging = true;
            //trailEffect.SetActive(true);
            sk.speed = 0;

            canBarge = false;

            /*if (ts.lockedOn)
            {
                Vector3 closestDir = (ts.closest.transform.position - transform.position).normalized;

                cc.Move(closestDir * bargeSpeed * Time.deltaTime);
            }
            else*/ 
            sk.cc.Move(sk.moveDir * bargeSpeed * Time.deltaTime);

            bargeDelay = 0.25f;

            yield return null;
        }

        /*if (marker != null)
        {
            ObjectPoolManager.instance.RecallObject(marker);
        }*/
    }
}
