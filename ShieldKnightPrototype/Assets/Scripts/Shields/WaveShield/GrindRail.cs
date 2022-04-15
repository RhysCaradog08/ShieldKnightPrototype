using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GrindRail : MonoBehaviour
{    
    public Transform[] nodes;

    //RailTest rail;

    public bool isLoop;

    [ExecuteInEditMode]
    private void Awake()
    {
        //rail = FindObjectOfType<RailTest>();
    }

    private void Start()
    {
        nodes = new Transform[transform.childCount];
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i] = transform.GetChild(i);
        }
    }

    public Vector3 LinearPosition(int segment, float ratio)
    {
        Vector3 p1 = nodes[segment].position;
        Vector3 p2 = nodes[segment + 1].position;

        return Vector3.Lerp(p1, p2, ratio);
    }

    /*public Quaternion Orientation(int segment, float ratio)
    {
        Quaternion q1 = nodes[segment].rotation;
        Quaternion q2 = nodes[segment + 1].rotation;

        return Quaternion.Lerp(q1, q2, ratio);
    }*/

    private void OnDrawGizmos()
    {
       for (int i = 0; i < nodes.Length - 1; i++)
       {
            Handles.DrawDottedLine(nodes[i].position, nodes[i + 1].position, 3f);

            Debug.DrawLine(nodes[i].transform.position, nodes[i].transform.position + nodes[i].transform.forward * 10, Color.green);
       }
    }
}
