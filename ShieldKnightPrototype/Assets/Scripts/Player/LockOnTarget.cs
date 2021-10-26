using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;
using System;

public class LockOnTarget : MonoBehaviour
{
    [SerializeField] List<GameObject> targetLocations = new List<GameObject>();
    [SerializeField]List<GameObject> visibleTargets = new List<GameObject>();
    GameObject[] taggedTargets;
    [SerializeField] GameObject closest = null;

    [SerializeField] float range;
    [SerializeField] private bool lockedOn = false;
    bool canLockOn;

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

                TargetsInView();
                FindClosestTarget();
                CheckTargetDistance();

                if (canLockOn)
                {
                    AddTargetMarker();
                    lockedOn = !lockedOn;
                }

            }
            else if(lockedOn)
            {
                lockedOn = false;
                RemoveTargetMarker();
            }
        }
        if(Input.GetKeyUp(KeyCode.Z))
        {
            visibleTargets.Clear();
            targetLocations.Clear();

            Array.Clear(taggedTargets, 0, taggedTargets.Length);
        }

        if(lockedOn && Vector3.Distance(transform.position, closest.transform.position) > range)
        {
            canLockOn = false;
            lockedOn = false;
            RemoveTargetMarker();
            closest = null;
        }

        if(Input.GetKey(KeyCode.X))
        {
            TargetsInView();
            //CheckTargetDistance();

            if(canLockOn)
            {
                Debug.Log("Can Lock On");
            }
            else Debug.Log("Cannot Lock On");
        }
        if(Input.GetKeyUp(KeyCode.X))
        {
            visibleTargets.Clear();
            targetLocations.Clear();

            Array.Clear(taggedTargets, 0, taggedTargets.Length);
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
        visibleTargets.Sort(delegate (GameObject a, GameObject b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.transform.position).CompareTo(Vector3.Distance(transform.position, b.transform.position));
        });

        float distance = Mathf.Infinity;
        Vector3 playerForward = transform.forward;

        foreach(GameObject target in visibleTargets)
        {
            Vector3 diff = (target.transform.position - playerForward);
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
        if(closest != null)
        {
            if (Vector3.Distance(transform.position, closest.transform.position) < range)
            {
                canLockOn =true;
            }
            else
            {
                canLockOn = false;
            }
        }
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

    void TargetsInView()
    {
        for(int i = 0; i < targetLocations.Count; i ++)
        {
            Vector3 targetPos = Camera.main.WorldToViewportPoint(targetLocations[i].transform.position);

            bool isVisible = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

            if(isVisible && !visibleTargets.Contains(targetLocations[i]))
            {
                visibleTargets.Add(targetLocations[i]);
            }
            else if(visibleTargets.Contains(targetLocations[i]) && !isVisible)
            {
                visibleTargets.Remove(targetLocations[i]);
            }
        }
    }
}
