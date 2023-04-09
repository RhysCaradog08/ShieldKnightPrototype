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

    [Header("Slam")]
    public float slamForce,  slamRadius, damageDelay;
    GameObject slamStars;
    Transform squashedObject;
    [SerializeField] List<Transform> slamObjects = new List<Transform>();
    bool showSlamVFX;

    [Header("Bounce")]
    public float bounceHeight, bounceTime;

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

        //Recall
        mushroomHoldPos = transform.parent.transform;
        mushroomHoldRot = Quaternion.Euler(-90, 0, 0);

        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = startScale;

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

        if (target != null)
        {
            hasTarget = true;
        }
        else hasTarget = false;

        if(hitTime <= 0 && bounceTime <= 0 && !sk.isGuarding)
        {
            mcAnim.ChangeAnimationState(mcAnim.idle);
        }

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

        if (sk.isSlamming)
        {
            sk.velocity.y = -slamForce;

            RaycastHit hit;

            if (Physics.Raycast(transform.position, -sk.transform.up, out hit))
            {
                float distToGround = hit.distance;

                Debug.DrawLine(sk.transform.position, -sk.transform.up * 10, Color.red);
                //Debug.Log("Distance to Ground: " + distToGround);

                if (distToGround < 3)
                {
                    Bounce();
                    /*Debug.DrawLine(sk.transform.position, -sk.transform.up * 10, Color.green);

                    if (!showSlamVFX)
                    {
                        slamStars = ObjectPoolManager.instance.CallObject("SlamStars", null, hit.point, Quaternion.Euler(-90, transform.rotation.y, transform.rotation.z), 1);
                        showSlamVFX = true;
                    }

                    Debug.Log("Hit Ground");
                    SlamImpact();*/
                }
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

                Vector3 squashedSize = new Vector3(col.transform.GetChild(0).localScale.x, 0.75f, col.transform.localScale.z);
                col.transform.GetChild(0).localScale = squashedSize;

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

    void Bounce()
    {
        Debug.Log("Bounce");
        sk.isSlamming = false;
        bounceTime = 0.2f;

        sk.velocity.y = bounceHeight;
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag != "Player")
        {
            Debug.Log(col.gameObject.name);
            hitTime = 0.1f;

            if (thrown)
            {
                StartCoroutine(RecallShield());
            }
        }
    }
}
