using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShieldController : MonoBehaviour
{
    PlayerController pc;
    ShieldSelect select;
    WaveShieldController wave;
    public GrindRail rail;

    [SerializeField] Collider[] grindObjects;

    [SerializeField] private int currentSegment;
    [SerializeField] private float transition, resetDelay;
    [SerializeField] List<Transform> grindPoints = new List<Transform>();
    [SerializeField] private Transform closest;
    [SerializeField] int index;
    public float speed;
    [SerializeField] float m;

    public ParticleSystem waves, wavesFlipped;

    public bool isAttacking, isSurfing, isGrinding, isCompleted, canGrind, isReversed;
    [SerializeField] private bool isLooping, inRange, getDotProd;

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
        select = FindObjectOfType<ShieldSelect>();
        wave = FindObjectOfType<WaveShieldController>();
    }

    private void Start()
    {
        isSurfing = false;
        isAttacking = false;
        isGrinding = false;

        resetDelay = 0;
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

        if(Input.GetButtonDown("Jump") && isGrinding) //Disables grind in order to Jump off.
        {
            //Debug.Log("Jump off Grind");
            isGrinding = false;
            ClearInformation();
            resetDelay = 1;
        }

        if (Input.GetButtonDown("Barge") && pc.attackDelay <= 0 && !pc.waveGuarding) //Sets isSurfing bool;
        {
            if(!isSurfing)
            {
                isSurfing = true;
            }
            else isSurfing = false;
        }

        if(Input.GetButtonUp("Barge") && isSurfing)
        {
            ClearInformation();
        }

        if (isSurfing || pc.waveGuarding)
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
        Vector3 forward = closest.transform.TransformDirection(Vector3.forward);
        Vector3 toOther = transform.position - closest.transform.position;

        if (Vector3.Dot(forward, toOther) < 0)
        {
            isReversed = true;
        }
        else isReversed = false;
    }

    void ClearInformation() //Clears all reference information to receive fresh information for next grind.
    {
        closest = null;
        grindPoints.Clear();
        rail = null;
        getDotProd = false;
        isReversed = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Grind")
        {
            //Debug.Log("Rail in Trigger");
            currentSegment = 0;
            index = 0;
            inRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Grind")
        {
            inRange = false;
            ClearInformation();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Use the same vars you use to draw your Overlap Sphere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}
