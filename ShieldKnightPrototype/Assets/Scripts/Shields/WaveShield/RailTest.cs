using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RailTest : MonoBehaviour
{
    public GrindRail rail;

    [SerializeField] Collider[] grindObjects;

    [SerializeField] private int currentSegment;
    [SerializeField] private float transition, activateDistance;
    [SerializeField] List<Transform> grindPoints = new List<Transform>();
    [SerializeField] private Transform closest;
    [SerializeField] int index;
    public float speed = 5;

    [Header("Activation Bools")]
    public bool isCompleted, canGrind, isGrinding;

    [Header("Control Bools")]
    [SerializeField] private bool isReversed;
    [SerializeField] private bool isLooping;

    private void Start()
    {
        isCompleted = false;
        canGrind = false;
        isGrinding = false;
    }

    private void Update()
    {
        GetRail();

        if (!rail)
            return;

        if (activateDistance < 2f && !isGrinding)
        {
            canGrind = true;
        }
        else if(isGrinding)
        {
            canGrind = false;
        }

        if (!isCompleted && canGrind)
            Play();

        else if(isCompleted)
        {
            isGrinding = false;
        }
    }

    void Play(bool forward = true)
    {
        currentSegment = index;

        float m = (rail.nodes[currentSegment + 1].position - rail.nodes[currentSegment].position).magnitude;
        float s = (Time.deltaTime * 1 / m) * speed;
        transition += (forward)? s : -s ;

        if (transition > 1)
        {
            transition = 0;
            currentSegment++;

            if (currentSegment == rail.nodes.Length - 1)
            {
                if (isLooping)
                {
                    currentSegment = 0;
                }
                else
                {
                    isCompleted = true;
                    return;
                }
            }
        }
        else if (transition < 0)
        {
            transition = 1;
            currentSegment--;

            if (currentSegment == -1)
            {
                if (isLooping)
                {
                    currentSegment = rail.nodes.Length - 2;
                }
                else
                {
                    isCompleted = true;
                    return;
                }
            }
        }

        transform.position = rail.LinearPosition(currentSegment, transition);
    }

    void GetRail()
    {
        Debug.Log("Find Rail");

        Collider[] grindObjects = Physics.OverlapSphere(transform.position, 10f);

        foreach (Collider col in grindObjects)
        {
            if (col.tag == "Grind")
            {
                rail = col.gameObject.GetComponent<GrindRail>();

                grindPoints = new List<Transform>(rail.nodes);
                GetClosestGrindPoint();
                
                foreach(Transform gp in grindPoints)
                {
                    Debug.DrawLine(transform.position, gp.position, Color.red);
                    Debug.DrawLine(transform.position, closest.position, Color.yellow);
                }
                activateDistance = Vector3.Distance(closest.position, transform.position);
            }
        }
    }

    void GetClosestGrindPoint()
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

        foreach(Transform gPoint in grindPoints)
        {
            Vector3 directionToClosest = gPoint.position - currentPosition;
            float dSqrToTarget = directionToClosest.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest = gPoint;

                index = System.Array.IndexOf(rail.nodes, closest);
                Debug.Log("Index: " + index);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}
