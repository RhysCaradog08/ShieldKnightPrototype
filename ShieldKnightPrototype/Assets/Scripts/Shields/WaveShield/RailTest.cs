using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RailTest : MonoBehaviour
{
    public GrindRail rail;

    [SerializeField] Collider[] grindObjects;

    [SerializeField] private int currentSegment;
    [SerializeField] private float transition;
    public float speed = 5;

    public bool isCompleted;
    [SerializeField] private bool isReversed;
    [SerializeField] private bool isLooping;

    private void Update()
    {
        GetRail();

        if (!rail)
            return;

        if (!isCompleted)
            Play(!isReversed);
    }

    void Play(bool forward = true)
    {
        float m = (rail.nodes[currentSegment + 1].position - rail.nodes[currentSegment].position).magnitude;
        float s = (Time.deltaTime * 1 / m) * speed;
        transition += (forward)? s : -s ;

        if(transition > 1)
        {
            transition = 0;
            currentSegment++;

            if(currentSegment == rail.nodes.Length -1)
            {
                if(isLooping)
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
        else if(transition < 0)
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
            Debug.DrawLine(transform.position, col.transform.position, Color.red);
            Debug.LogFormat(col.name);

            if (col.tag == "Grind")
            {
                Debug.DrawLine(transform.position, col.transform.position, Color.yellow);
                float distance = Vector3.Distance(col.transform.position, transform.position);

                if (distance < 2f)
                {
                    Debug.Log("Grind Object:  " + col.name);
                    Debug.DrawLine(transform.position, col.transform.position, Color.green);

                    rail = col.gameObject.GetComponent<GrindRail>();
                }
            }
        }
    }

   void FindNearestNode()
   {

   }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}
