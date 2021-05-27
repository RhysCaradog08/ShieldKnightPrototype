using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    public int targetsInRange;
    public List<GameObject> targets = new List<GameObject>();

    //public GameObject closestTarget;
    //bool hasTarget;

    void Update()
    {
      
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);

        if(other.gameObject.layer == 8) //If ShieldTarget is within trigger, add to list & increment int.
        {
            targets.Add(other.gameObject);

            ++targetsInRange;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8) //If ShieldTarget is outside trigger, remove from list & decrement int.
        {
            targets.Remove(other.gameObject);

            --targetsInRange;
        }
    }
}
