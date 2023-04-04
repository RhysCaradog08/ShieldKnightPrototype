using Basics.ObjectPool;
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
    [SerializeField] List<Transform> targets = new List<Transform>();
    TrailRenderer trail;
    public bool thrown, canThrow, hasTarget;

    [Header("Recall")]
    Transform mushroomHoldPos;
    Quaternion mushroomHoldRot;
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

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
            if(hasTarget) 
            {
                sk.transform.LookAt(target.transform);
                StartCoroutine(TargetedThrow());
            }
            else NonTargetThrow();
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

        if ((Input.GetButton("Throw") && !thrown))
        {
            ts.FindTargets();
            ts.FindClosestTarget();
        }

        if(Input.GetButtonUp("Throw"))
        {
            targets.Clear();
        }

        if (hasTarget)
        {
            for (int i = 0; i < ts.targetLocations.Count; i++)
            {
                if (!targets.Contains(ts.targetLocations[i].transform))
                {
                    if (targets.Count < 3)
                    {
                        targets.Add(ts.targetLocations[i].transform);
                    }
                }
            }

            foreach (Transform t in targets)
            {
                Debug.DrawLine(sk.transform.position, t.position, Color.green);

                if (!t.Find("TargetMarker"))
                {
                    if (!marker)
                    {
                        marker = ObjectPoolManager.instance.CallObject("TargetMarker", t, new Vector3(t.transform.position.x, t.transform.position.y + 4.5f, t.transform.position.z - 0.5f), Quaternion.identity);
                    }
                }
            }
        }
    }

    void NonTargetThrow()  //Throws Shield in players forward vector if no targets are identified.
    {
        thrown = true;

        mushroomRB.isKinematic = false;
        mushroomRB.AddForce(sk.transform.forward * throwForce, ForceMode.Impulse);

        transform.parent = null;
    }

    IEnumerator TargetedThrow()  //Throws Shield towards any identified targets in range.
    {
        thrown = true;

        while (Vector3.Distance(target.transform.position, transform.position) > 0.1f)
        {
            transform.parent = null;

            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, throwForce * Time.deltaTime);

            yield return null;
        }

        if (Vector3.Distance(target.transform.position, transform.position) < 0.1f)
        {
            if (marker != null)
            {
                ObjectPoolManager.instance.RecallObject(marker);
                marker = null;
            }
        }

        target = null;  //Once all targets are reached return Shield to Player.
        StartCoroutine(RecallShield());
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

            meshCol.enabled = false; //Prevents from unecessary collisions upon return.

            yield return null;
        }

        mushroomRB.isKinematic = true;

        transform.parent = mushroomHoldPos;  //Sets Shields position and parent to stay attached to Player.
        transform.localPosition = Vector3.zero;
        transform.localRotation = mushroomHoldRot;

        meshCol.enabled = true;

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
