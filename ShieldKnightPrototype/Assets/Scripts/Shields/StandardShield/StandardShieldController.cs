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
        shieldHoldRot = Quaternion.Euler(-90, 90, 0); ;

        shieldScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = shieldScale;

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
}
