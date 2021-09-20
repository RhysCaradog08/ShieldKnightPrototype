using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class LockOnTarget : MonoBehaviour
{
    [SerializeField] List<GameObject> targetLocations = new List<GameObject>();
    [SerializeField] GameObject closest = null;

    [SerializeField] private bool lockedOn = false;

    [Header("UI")]
    GameObject lockOnMarker;

    // Update is called once per frame
    void Update()
    {
        if (closest != null && lockedOn)
        {
            transform.LookAt(new Vector3(closest.transform.position.x, transform.position.y, closest.transform.position.z));
        }
        

        if(Input.GetKeyDown(KeyCode.Z))
        {
            if (!lockedOn)
            {
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("Target"))
                {
                    targetLocations.Add(go);
                }

                FindClosestTarget();
                AddTargetMarker();

                lockedOn = !lockedOn;
            }
            else if(lockedOn)
            {
                lockedOn = false;
                RemoveTargetMarker();
            }
        }
        if(Input.GetKeyUp(KeyCode.Z))
        {
            targetLocations.Clear();
        }

        if (lockedOn)
        {
            if (!closest)
            {
                lockedOn = false;
                closest = null;
            }
        }
    }

    GameObject FindClosestTarget()
    {
        targetLocations.Sort(delegate (GameObject a, GameObject b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.transform.position).CompareTo(Vector3.Distance(transform.position, b.transform.position));
        });

        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach(GameObject target in targetLocations)
        {
            Vector3 diff = (target.transform.position - position);
            float currentDist = diff.sqrMagnitude;

            if(currentDist < distance)
            {
                closest = target;
                distance = currentDist;
            }
        }
        return closest;
    }

    void CheckTargetDistance()
    {
        //Check if in range to lock on to target.
    }

    void AddTargetMarker()
    {
        Vector3 markerPos = closest.transform.position;

        lockOnMarker = ObjectPoolManager.instance.CallObject("LockOnMarker", closest.transform, new Vector3(markerPos.x, markerPos.y +2, markerPos.z), Quaternion.Euler(180, 0, 0));
    }

    void RemoveTargetMarker()
    {
        ObjectPoolManager.instance.RecallObject(lockOnMarker);
    }
}
