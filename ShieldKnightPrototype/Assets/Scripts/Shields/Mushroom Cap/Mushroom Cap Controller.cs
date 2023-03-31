using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomCapController : MonoBehaviour
{
    [SerializeField] ShieldKnightController sk;
    TargetSelector ts;

    public LayerMask ignoreLayer;

    [SerializeField] Rigidbody mushroomRB;
    [SerializeField] GameObject marker;

    [Header("Throw")]
    public float throwForce, dist;
    public GameObject target;
    TrailRenderer trail;
    public bool thrown, canThrow, hasTarget;

    [Header("Recall")]
    Transform mushroomHoldPos;
    Quaternion mushroomHoldRot;
    float lerpTime = 1f;
    //[SerializeField] MeshCollider meshCol;

    Vector3 startScale;

    private void Awake()
    {
        sk = FindObjectOfType<ShieldKnightController>();
        ts = FindObjectOfType<TargetSelector>();

        mushroomRB = GetComponentInChildren<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Throw
        thrown = false;
        target = null;

        //Recall
        mushroomHoldPos = transform.parent.transform;
        mushroomHoldRot = Quaternion.Euler(-90, 0, 0);

        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = startScale;

        if (target != null)
        {
            hasTarget = true;
        }
        else hasTarget = false;

        if(canThrow)
        {
            NonTargetThrow();
        }

        if (thrown)  //Stops Player repeatedly throwing the shield.
        {
            //trail.enabled = true;
            canThrow = false;

            dist = Vector3.Distance(transform.position, sk.transform.position);

            if (dist > 50)
            {
                StartCoroutine(RecallShield());
            }
        }
        //else trail.enabled = false;


    }

    void NonTargetThrow()  //Throws Shield in players forward vector if no targets are identified.
    {
        thrown = true;

        mushroomRB.isKinematic = false;
        mushroomRB.AddForce(sk.transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;
    }

    IEnumerator RecallShield()  //Recalls Shield back to Shield Holder.
    {
        float t = 0f;
        while (t < lerpTime) //Returns Shield to Shield Holder over the course of 1 second.
        {
            mushroomRB.isKinematic = false;

            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, mushroomHoldPos.position, t / lerpTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, mushroomHoldPos.rotation, t / lerpTime);

            //meshCol.enabled = false; //Prevents from unecessary collisions upon return.

            yield return null;
        }

        mushroomRB.isKinematic = true;

        transform.parent = mushroomHoldPos;  //Sets Shields position and parent to stay attached to Player.
        transform.localPosition = Vector3.zero;
        transform.localRotation = mushroomHoldRot;

        //meshCol.enabled = true;

        thrown = false;
    }

    private void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag != "Player")
        {
            Debug.Log(col.gameObject.name);
            StartCoroutine(RecallShield());
        }
    }
}
