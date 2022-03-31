using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailTest : MonoBehaviour
{
    public GrindRail rail;

    [SerializeField] private int currentSegment;
    [SerializeField] private float transition;
    [SerializeField] private bool isCompleted;

    private void Update()
    {
        if (!rail)
            return;

        if (!isCompleted)
            Play();
    }

    void Play()
    {
        transition += Time.deltaTime * 1 / 2.5f;

        if(transition > 1)
        {
            transition = 0;
            currentSegment++;
        }
        else if(transition < 0)
        {
            transition = 1;
            currentSegment--;
        }

        transform.position = rail.LinearPosition(currentSegment, transition);
    }
}
