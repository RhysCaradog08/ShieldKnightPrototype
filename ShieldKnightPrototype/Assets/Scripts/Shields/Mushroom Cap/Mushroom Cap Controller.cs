using Basics.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomCapController : MonoBehaviour
{
    [SerializeField] ShieldKnightController sk;
    [SerializeField] MushroomCapAnimationController mcAnim;
    TargetSelector ts;
    ShieldSelect select;
    MarkerCheck markerCheck;

    Camera cam;

    public LayerMask ignoreLayer;

    [SerializeField] Rigidbody mushroomRB;
    //[SerializeField] GameObject sporeTrail;
    [SerializeField] GameObject hitStars;

    [Header("Throw")]
    Vector3 startScale;
    public float throwForce, dist;
    [SerializeField] float hitTime;
    public Transform target;
    [SerializeField] List<Transform> targets = new List<Transform>();
    TrailRenderer trail;
    public bool thrown, canThrow, hasTarget;

    [Header("Recall")]
    Transform mushroomHoldPos;
    Quaternion mushroomHoldRot;
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

    [Header("Bounce")]
    public float bounceHeight, bounceTime;
    public bool isBouncing;

    [Header("Bounce Pad")]
    [SerializeField] float bouncePadRange, bouncePadDelay;
    Vector3 bouncePadPos;
    public bool isThrowingBP, isBouncePad;

    private void Awake()
    {
        sk = FindObjectOfType<ShieldKnightController>();
        mcAnim = FindObjectOfType<MushroomCapAnimationController>();
        ts = FindObjectOfType<TargetSelector>();
        select = FindObjectOfType<ShieldSelect>();

        cam = Camera.main;

        mushroomRB = GetComponentInChildren<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Throw
        hitTime = 0;
        thrown = false;
        target = null;
        startScale = transform.localScale;

        //Recall
        mushroomHoldPos = transform.parent.transform;
        mushroomHoldRot = Quaternion.Euler(-90, 0, 0);

        //Bounce Pad
        isBouncePad = false;
        isBouncing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBouncePad)
        {
            transform.localScale = startScale;
        }

        if (hitTime > 0)
        {
            hitTime -= Time.deltaTime;
            mcAnim.ChangeAnimationState(mcAnim.hit);
        }
        else if (hitTime <= 0)
        {
            hitTime = 0;
        }

        if(bounceTime > 0)
        {
            bounceTime -= Time.deltaTime;
            mcAnim.ChangeAnimationState(mcAnim.bounce);
        }
        else if (bounceTime <= 0)
        {
            bounceTime = 0;
        }

        if(bouncePadDelay > 0) 
        {
            bouncePadDelay -= Time.deltaTime;  
        }
        else if (bouncePadDelay <= 0)
        {
            bouncePadDelay = 0;
        }

        if(isBouncing && !sk.cc.isGrounded)
        {
            sk.canPressSpace = false;
        }
        
        if(isBouncing)
        {
            if(sk.cc.isGrounded)
            {
                isBouncing = false;
                sk.canPressSpace = true;
            }
        }

        if (target != null)
        {
            hasTarget = true;
        }
        else hasTarget = false;

        if(hitTime <= 0 && bounceTime <= 0 && !sk.isGuarding)
        {
            mcAnim.ChangeAnimationState(mcAnim.idle);
        }

        if (!isBouncing)
        {
            if (canThrow)
            {
                if (hasTarget)
                {
                    sk.transform.LookAt(target.transform);
                    StartCoroutine(TargetedThrow());
                }
                else NonTargetThrow();
            }

            if (thrown)  //Stops Player repeatedly throwing the shield.
            {
                select.canChange = false;

                //sporeTrail.SetActive(true);
                canThrow = false;

                dist = Vector3.Distance(transform.position, sk.transform.position);

                if (dist > 50 && !target)
                {
                    StartCoroutine(RecallShield());
                }

            }
            else
            {
                select.canChange = true;
                dist = 0;
                //sporeTrail.SetActive(false);
            }

            if ((Input.GetButton("Throw") && !thrown))
            {
                if (ts.canTarget)
                {
                    ts.FindTargets();
                    Transform targetToAdd;

                    for (int i = 0; i < ts.targetLocations.Count; i++)
                    {
                        targetToAdd = ts.targetLocations[i].transform;

                        if (!targets.Contains(targetToAdd))
                        {
                            if (targets.Count < 3)
                            {
                                targets.Add(targetToAdd);
                            }
                            else if (targets.Count == 3 && !targets.Contains(targetToAdd))
                            {
                                if (targets[2].GetComponent<MarkerCheck>())
                                {
                                    markerCheck = targets[2].GetComponent<MarkerCheck>();
                                    markerCheck.RemoveMarker();
                                }
                                targets.Remove(targets[2]);

                                targets.Add(targetToAdd);
                            }
                        }
                    }
                }

                if (targets.Count > 0)
                {
                    SortTargetsByDistance();

                    target = targets[0];

                    foreach (Transform t in targets)
                    {
                        if (t.GetComponent<MarkerCheck>() == null)
                        {
                            t.gameObject.AddComponent<MarkerCheck>();
                        }

                        markerCheck = t.GetComponent<MarkerCheck>();

                        if (markerCheck.canAddMarker == true)
                        {
                            markerCheck.AddMarker();
                        }
                    }
                }
            }

            if (Input.GetButtonUp("Barge") && !isBouncePad)
            {
                if (bouncePadDelay <= 0)
                {
                    bouncePadPos = sk.transform.position + sk.transform.forward * bouncePadRange;

                    StartCoroutine(SetBouncePad());
                    isBouncePad = true;
                }
            }

            if (sk.cc.isGrounded && sk.buttonHeld)
            {
                if (!thrown && !sk.isSlamming && hitTime <= 0)
                {
                    sk.isGuarding = true;
                }
            }
            else sk.isGuarding = false;

            if (sk.isGuarding)
            {
                mcAnim.ChangeAnimationState(mcAnim.guard);
            }
        }

        if (isBouncePad)
        {
            BouncePad();

            if(Input.GetButtonDown("Throw") || Input.GetButtonDown("Barge") || Input.GetButtonDown("Guard"))
            {
                isBouncePad = false;
                bouncePadDelay = 1;
                StartCoroutine(RecallShield());
            }
        }

        if (!sk.cc.isGrounded && Input.GetButtonDown("Guard"))
        {
            sk.isSlamming = true;
        }

        if (sk.isSlamming)
        {
            mcAnim.ChangeAnimationState(mcAnim.slam);
        }

        if (thrown || isThrowingBP || isBouncePad || sk.isGuarding || sk.isSlamming) 
        {
            select.canChange = false;
        }
        else select.canChange = true;
    }

    void SortTargetsByDistance()
    {
        targets.Sort(delegate (Transform a, Transform b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.position)///////
            .CompareTo(
              Vector3.Distance(transform.position, b.position));
        });
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

        foreach (Transform nextTarget in targets)
        {
            while (transform.position != nextTarget.position)
            {
                transform.parent = null;

                transform.position = Vector3.MoveTowards(transform.position, nextTarget.position, throwForce * Time.deltaTime);

                yield return null;
            }

            if (Vector3.Distance(nextTarget.position, transform.position) < 0.1f)
            {
                hitTime = 0.2f;

                hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, nextTarget.position, Quaternion.identity, 1);

                if (nextTarget.GetComponent<MarkerCheck>())
                {
                    Debug.Log("Remove Marker");
                    markerCheck = nextTarget.GetComponent<MarkerCheck>();
                    markerCheck.RemoveMarker();
                }
            }
        }

        target = null;  //Once all targets are reached return Shield to Player.
        targets.Clear();
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

        if(isBouncePad)
        {
            isBouncePad = false;
        }
    }

    public void Bounce()
    {
        Debug.Log("Bounce");
        sk.isSlamming = false;
        bounceTime = 0.2f;

        sk.velocity.y = sk.jumpSpeed * bounceHeight;
    }

    Vector3 CalculateParaBolicCurve(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = t * 2 - 1;
        if (Mathf.Abs(start.y - end.y) < 0.1f)
        {
            //start and end are roughly level, pretend they are - simpler solution with less steps
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += (-parabolicT * parabolicT + 1) * height;
            return result;
        }
        else
        {
            //start and end are not level, gets more complicated
            Vector3 travelDirection = end - start;
            Vector3 levelDirecteion = end - new Vector3(start.x, end.y, start.z);
            Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
            Vector3 up = Vector3.Cross(right, travelDirection);
            if (end.y > start.y) up = -up;
            Vector3 result = start + t * travelDirection;
            result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
            return result;
        }
    }

    IEnumerator SetBouncePad()
    {
        transform.parent = null;

        while (Vector3.Distance(bouncePadPos, transform.position) > 1)
        {
            isThrowingBP = true;
            transform.parent = null;

            float travelSpeed = 5 * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, bouncePadPos, travelSpeed);//CalculateParaBolicCurve(transform.position, bouncePadPos, 2, travelSpeed);

            yield return null;
        }


        if (Vector3.Distance(bouncePadPos, transform.position) < 1)
        {
            transform.position = bouncePadPos;

            isThrowingBP = false;
            isBouncePad = true;
        }
    }

    void BouncePad()
    {
        transform.position = transform.position;
        transform.rotation = Quaternion.Euler(-90, 0, 0);
        transform.localScale = new Vector3(3, 3, 2);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag != "Player")
        {
            hitTime = 0.1f;

            if (thrown)
            {
                StartCoroutine(RecallShield());
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (isBouncePad)
            {
                hitTime = 0.1f;
                isBouncing = true;
                sk.velocity.y = sk.jumpSpeed * bounceHeight;
            }
        }
    }
}
