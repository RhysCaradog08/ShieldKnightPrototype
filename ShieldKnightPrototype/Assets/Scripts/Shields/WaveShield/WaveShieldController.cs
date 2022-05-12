using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class WaveShieldController : MonoBehaviour
{
    PlayerController pc;
    CharacterController cc;
    ShieldSelect select;
    public GrindRail rail;

    [SerializeField] Collider[] grindObjects;

    [Header("Grinding")]
    [SerializeField] private int currentSegment;
    [SerializeField] private float transition, resetDelay;
    [SerializeField] List<Transform> grindPoints = new List<Transform>();
    [SerializeField] private Transform closest;
    [SerializeField] int index;
    public float speed;
    [SerializeField] float m;

    [Header("Wave Slam")]
    [SerializeField] float slamForce, slamPushBack, slamRadius, slamLift, slamDelay, damageDelay, slamWait;
    GameObject slamStars;
    public bool isSlamming;
    bool showSlamVFX;

    [Header("VFX")]
    public ParticleSystem waves, wavesFlipped;

    [Header("Booleans")]
    public bool isAttacking, isSurfing, isGrinding, isCompleted, canGrind, isReversed;
    [SerializeField] private bool isLooping, inRange, getDotProd;

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
        cc = FindObjectOfType<CharacterController>();
        select = FindObjectOfType<ShieldSelect>();
    }

    private void Start()
    {
        //Grinding
        resetDelay = 0;

        //Wave Slam
        isSlamming = false;
        showSlamVFX = false;

        //Booleans
        isSurfing = false;
        isAttacking = false;
        isGrinding = false;
    }

    private void Update()
    {
        if (resetDelay > 0)
        {
            resetDelay -= Time.deltaTime;
        }

        if (resetDelay <= 0)
        {
            resetDelay = 0;
        }

        if (slamDelay > 0)
        {
            slamDelay -= Time.deltaTime;
        }

        if (slamDelay <= 0)
        {
            slamDelay = 0;
            isSlamming = false;
        }

        if (cc.isGrounded)
        {
            if (slamWait <= 0)  //Resets player being immobile once grounded after Slam action is performed.
            {
                slamWait = 0;
                isSlamming = false;
            }
            else //Whilst waitTime > 0 player is immobile.
            {
                isSlamming = true;
            }
        }


        if (Input.GetButtonDown("Jump") && isGrinding) //Disables grind in order to Jump off.
        {
            //Debug.Log("Jump off Grind");
            isGrinding = false;
            ClearInformation();
            resetDelay = 1;
        }

        if (!cc.isGrounded && Input.GetButtonDown("Guard"))  //Input to perform Slam action.
        {
            if (!isSurfing || !isSlamming)
            {
                slamWait = 0.5f;
                isSlamming = true;
            }
        }

        if (isSlamming && !isSurfing)
        {
            pc.speed = 0;
            pc.slamming = true;

            pc.velocity.y = slamForce;

            RaycastHit hit;

            if (Physics.Raycast(pc.transform.position, -pc.transform.up * 10, out hit))
            {
                //Debug.DrawLine(pc.transform.position, -pc.transform.up * 10, Color.green);

                float distToGround = hit.distance;

                if (distToGround < 1)
                {
                    SlamImpact();

                    if (!showSlamVFX)
                    {
                        slamStars = ObjectPoolManager.instance.CallObject("SlamStars", null, transform.position, Quaternion.Euler(-90, transform.rotation.y, transform.rotation.z), 1);
                        showSlamVFX = true;
                    }

                    slamDelay = 0.5f;
                }
            }
        }
        else
        {
            pc.stopped = false;
            pc.slamming = false;
            showSlamVFX = false;
        }

        if (cc.isGrounded && isSlamming)
        {
            slamWait -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Barge") && pc.attackDelay <= 0 && !pc.waveGuarding) //Sets isSurfing bool;
        {
            if (!isSurfing)
            {
                isSurfing = true;
                pc.canSurf = true;
            }
            else
            {
                isSurfing = false;
                pc.canSurf = false;
            }
        }

        if(Input.GetButtonUp("Barge") && isSurfing)
        {
            ClearInformation();
        }

        if (isSurfing || pc.waveGuarding || pc.attackReset > 0)
        {
            waves.Play();
            wavesFlipped.Play();

            select.canChange = false;
        }
        else
        {
            waves.Stop();
            wavesFlipped.Stop();
            select.canChange = true;
        }

        if (inRange && isSurfing) //Get Rail information if in close enough range.
        {
            GetRail();
        }

        if (inRange && !canGrind) //Condition to stop calculation of Dot Product once grind has begun.
        {
            if (Vector3.Distance(closest.position, transform.position) < 2.5f)
            {
                getDotProd = false;
            }
            else getDotProd = true;
        }

        if (getDotProd)
        {
            CalculateDotProduct();
        }

        if (!canGrind && closest != null)
        {
            currentSegment = index;
            speed = pc.surfSpeed;
        }

        if (!rail)
            return;

        if (resetDelay <= 0 && canGrind)
        {
            Play(!isReversed);
        }
    }

    void Play(bool forward = true)  //Moves player transform through the array of nodes.
    {
        float m; //Calculate magnitude of the current segment of rail the player is on.

        if (currentSegment == rail.nodes.Length - 1)
        {
            m = (rail.nodes[currentSegment].position - rail.nodes[currentSegment - 1].position).magnitude;
        }
        else m = (rail.nodes[currentSegment + 1].position - rail.nodes[currentSegment].position).magnitude;
        float s = (Time.deltaTime * 1 / m) * speed; //Calculates speed of travel between nodes.
        transition += (forward) ? s : -s; //Determines if transform moves forward or back.

        if (transition > 1) //If transform has reached the end of the transition increment through the nodes.
        {
            isGrinding = true;
            transition = 0;
            currentSegment++;

            if (currentSegment == rail.nodes.Length - 1) //Reached end of nodes.
            {
                if (isLooping)
                {
                    currentSegment = 0; //Resets back to start of list of nodes.
                }
                else //Clear information and set resetDelay to stop unwanted continuous grinding.
                {
                    isGrinding = false;
                    ClearInformation();
                    ResetInts();
                    resetDelay = 1;
                    return;
                }
            }
        }
        else if (transition < 0) //Reverses direction of travel by decrementing backwards through nodes.
        {
            isGrinding = true;
            transition = 1;
            currentSegment--;

            if (currentSegment == -1) //Reached end of nodes.
            {
                if (isLooping)
                {
                    currentSegment = rail.nodes.Length - 2; //Resets back to start of list of nodes.
                }
                else //Clear information and set resetDelay to stop unwanted continuous grinding.
                {
                    isGrinding = false;
                    ClearInformation();
                    ResetInts();
                    resetDelay = 1;
                    return;
                }
            }
        }

        pc.transform.position = rail.LinearPosition(currentSegment, transition); //Monitors players current position on rail.
        if (!isReversed)
        {
            pc.transform.LookAt(rail.nodes[currentSegment + 1].position); //Sets player rotation relative to direction of next node.
        }
        else pc.transform.LookAt(rail.nodes[currentSegment - 1].position); ; //Reverses player rotation if tavelling back along rail.
    }

    void GetRail()  //Gets GrindRail component and array of nodes,finding the closest node and enabling the ability to grind.
    {
        Debug.Log("Find Rail");

        Collider[] grindObjects = Physics.OverlapSphere(transform.position, 10f);

        foreach (Collider col in grindObjects)
        {
            if (col.tag == "Grind")
            {
                rail = col.gameObject.GetComponent<GrindRail>();

                if (rail.isLoop)
                {
                    isLooping = true;
                }
                else isLooping = false;

                grindPoints = new List<Transform>(rail.nodes);
                GetClosestGrindPoint();

                foreach (Transform gp in grindPoints)
                {
                    Debug.DrawLine(transform.position, gp.position, Color.red);
                    Debug.DrawLine(transform.position, closest.position, Color.yellow);
                }

                if (Vector3.Distance(closest.position, transform.position) < 2f && resetDelay <= 0) //Once Player is close enough to closest node enable grind.
                {
                    canGrind = true;
                }
            }
        }
    }

    void GetClosestGrindPoint()  //Sorts through list by comparing distance between player and node transforms to determine the closest node.
    {
        grindPoints.Sort(delegate (Transform a, Transform b)
        {
            return Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(transform.position, b.transform.position));
        });

        closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (Transform gPoint in grindPoints)
        {
            Vector3 directionToClosest = gPoint.position - currentPosition;
            float dSqrToTarget = directionToClosest.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest = gPoint;

                index = System.Array.IndexOf(rail.nodes, closest);
                //Debug.Log("Index: " + index);
            }
        }
    }

    void CalculateDotProduct() //Use Dot Product to determine if player is in front or behind closest node. If behind the directiopn of travel is reversed.
    {
        Debug.Log("Calculating Dot Product");
        Vector3 nodeDirection = closest.transform.TransformDirection(closest.transform.position + closest.transform.forward);
        Vector3 toOther = transform.position - closest.transform.position;

        if (Vector3.Dot(nodeDirection, toOther) < 0)
        {
            isReversed = false;
        }
        else isReversed = true;
    }

    void ClearInformation() //Clears all reference information to receive fresh information for next grind.
    {
        closest = null;
        grindPoints.Clear();
        rail = null;
        getDotProd = false;
        isReversed = false;
        isLooping = false;
        canGrind = false;
        isGrinding = false;

        for (int i = 0; i < grindObjects.Length; i++)
        {
            grindObjects[i] = null;
        }
    }

    void ResetInts() //Resets currentSegment and index ints. Prevents inadvertantly looping back to start of grind.
    {
        currentSegment = 0;
        index = 0;
    }

    void SlamImpact()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, slamRadius);

        foreach (Collider col in colliders)
        {
            Rigidbody slamRB = col.GetComponent<Rigidbody>();

            if (slamRB != null)
            {
                slamRB.AddExplosionForce(slamPushBack, transform.position, slamRadius, slamLift, ForceMode.Impulse);
            }

            EnemyHealth enemy = col.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                if (damageDelay <= 0)
                {
                    enemy.TakeDamage(10);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Grind")
        {
            //Debug.Log("Rail in Trigger");
            inRange = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Water")
        {
            Debug.Log("Found Water");

            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.TransformDirection(-transform.up), out hit, 15)) //Directs a raycast down to detect distance between Wave Shield and the Surfable Surface.
            {
                //Debug.DrawRay(transform.position, transform.TransformDirection(-transform.up) * hit.distance, Color.red);
                if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Surfable Surface"))
                {
                    Debug.Log("Water Surface Point: " + hit.point);

                    if(hit.distance < 1 && pc.canSurf) //If Wave Shield is close enough to Surfable Surface, disable player velocity.y and set new transform to be on top of the Surfable Surface.
                    {
                        //Debug.DrawRay(transform.position, transform.TransformDirection(-transform.up) * hit.distance, Color.green);
                        pc.canSurf = false;
                        pc.transform.position = new Vector3(pc.transform.position.x, hit.point.y + 1, pc.transform.position.z);
                    }
                    else if(hit.distance >= 1 || !isSurfing)
                    {
                       // Debug.Log("Stop Surfing");
                        pc.canSurf = true;
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Grind")
        {
            inRange = false;
            ClearInformation();
            ResetInts();
        }

        if(other.tag == "Water")
        {
            if(!pc.canSurf)
            {
                pc.canSurf = true;
            }
        }
    }
}
