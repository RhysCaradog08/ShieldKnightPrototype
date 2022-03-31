using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailTest : MonoBehaviour
{
    public GrindRail rail;

    [SerializeField] private int currentSegment;
    [SerializeField] private float transition;
    public bool isCompleted;

    public float speed = 5;
    [SerializeField] private bool isReversed;
    [SerializeField] private bool isLooping;

    private void Update()
    {
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
}
