using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;
using System;

public class TargetingSystem : MonoBehaviour
{
    MarkerCheck markerCheck;
    ShieldController shield;

    Camera cam;

    //[SerializeField] List<GameObject> targetsInRange = new List<GameObject>();
    [SerializeField] GameObject [] taggedTargets;
    public List<GameObject> targetLocations = new List<GameObject>();
    public List<GameObject> visibleTargets = new List<GameObject>();

    public GameObject closest;

    [SerializeField] private float range;
    [SerializeField] private float targetFOV;

    bool canLockOn;
    public bool lockedOn;

    [Header("UI")]
    public GameObject targetMarker;
    public GameObject lockOnMarker;

    private void Start()
    {
        shield = GameObject.FindObjectOfType<ShieldController>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (closest != null && lockedOn)
        {
            transform.LookAt(new Vector3(closest.transform.position.x, transform.position.y, closest.transform.position.z));
        }

        if (Input.GetButtonDown("Fire1") && !shield.thrown)
        {
            GetTargets();
        }

        if (Input.GetButton("Fire1") && !shield.thrown) //Determine which targets fall within range and which is closest.
        {
            TargetsInView();
            FindClosestTarget();
        }

        if(Input.GetButtonUp("Fire1")) //Clears taggedTargets and targetLocations for next instance.
        {
            targetLocations.Clear();

            Array.Clear(taggedTargets, 0, taggedTargets.Length);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!lockedOn)
            {
                GetTargets();

                TargetsInView();
                FindClosestTarget();
                CheckTargetDistance();

                if (canLockOn)
                {
                    AddLockOnMarker();
                    lockedOn = !lockedOn;
                }

            }
            else if (lockedOn)
            {
                lockedOn = false;
                closest = null;
                visibleTargets.Clear();
                RemoveLockOnMarker();
            }
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            targetLocations.Clear();

            Array.Clear(taggedTargets, 0, taggedTargets.Length);
        }

        if (lockedOn && Vector3.Distance(transform.position, closest.transform.position) > range)
        {
            canLockOn = false;
            lockedOn = false;
            RemoveLockOnMarker();
            closest = null;
        }


        if (visibleTargets.Count > 0) //If there are targets in list, shield.target will be the closest.
        {
            shield.target = closest;            
        }
        else closest = null;


        if (closest != null)
        {
            shield.hasTarget = true;
        }
        else shield.hasTarget = false;
    }

    void FindClosestTarget()
    {
        visibleTargets.Sort(delegate (GameObject a, GameObject b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(transform.position, b.transform.position));
        });

        closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject target in visibleTargets) //Measures distance from player to targets and if it falls within an angle of focus then calculates which target is closest.
        {
            Debug.DrawLine(transform.position, target.transform.position, Color.red);

            //Distance
            Vector3 directionToTarget = target.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            //Angle
            float cosAngle = Vector3.Dot((target.transform.position - transform.position).normalized, cam.transform.forward);
            float angle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;

            if (dSqrToTarget < closestDistanceSqr && angle < targetFOV)
            {
                Debug.DrawLine(transform.position, target.transform.position, Color.green);
                closestDistanceSqr = dSqrToTarget;
                closest = target;
            }
        }
    }

    void AddTargetMarker()
    {
        foreach(GameObject target in visibleTargets)
        {
            if(markerCheck != null)
            {
                markerCheck = target.GetComponent<MarkerCheck>();
            }

            if(markerCheck.canAddMarker == true)
            {
                Vector3 markerPos = target.transform.position;

                targetMarker = ObjectPoolManager.instance.CallObject("TargetMarker", target.transform, new Vector3(markerPos.x, markerPos.y + 2, markerPos.z - 0.65f), Quaternion.identity);
            }
        }

        /*for (int i = 0; i < visibleTargets.Count; i++)
        {
            Vector3 markerPos = visibleTargets[i].transform.position;

            targetMarker = ObjectPoolManager.instance.CallObject("TargetMarker", visibleTargets[i].transform, new Vector3(markerPos.x, markerPos.y + 2, markerPos.z - 0.65f), Quaternion.identity);  
        }*/
    }

    void RemoveTargetMarker()
    {
        ObjectPoolManager.instance.RecallObject(targetMarker);
        /*foreach (GameObject target in visibleTargets)
        {
            if (markerCheck != null)
            {
                markerCheck = target.GetComponent<MarkerCheck>();
            }

            if (markerCheck.canAddMarker == false)
            {
                ObjectPoolManager.instance.RecallObject(targetMarker);
            }
        }*/
    }

    void GetTargets()
    {
        taggedTargets = GameObject.FindGameObjectsWithTag("Target");

        foreach (GameObject target in taggedTargets)
        {
            if (!targetLocations.Contains(target))
            {
                targetLocations.Add(target);
            }
        }

        foreach (GameObject go in targetLocations)
        {
            markerCheck = go.AddComponent<MarkerCheck>();
        }
        /*taggedTargets = GameObject.FindGameObjectsWithTag("Target");

        foreach (GameObject go in taggedTargets)
        {
            Debug.DrawLine(transform.position, go.transform.position, Color.red);

            float cosAngle = Vector3.Dot((go.transform.position - transform.position).normalized, cam.transform.forward);
            float angle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;

            if((angle < targetFOV) && (Vector3.Distance(transform.position, go.transform.position) < range))
            {
                Debug.DrawLine(transform.position, go.transform.position, Color.green);
                if (!targetsInRange.Contains(go))
                {
                    targetsInRange.Add(go);
                }

                AddTargetMarker();
            }
            else if ((angle > targetFOV) || Vector3.Distance(transform.position, go.transform.position) > range)
            {
                targetsInRange.Remove(go);

                if (targetMarker != null)
                {
                    ObjectPoolManager.instance.RecallObject(targetMarker);
                }

                if(markerCheck != null)
                {
                    Destroy(markerCheck);
                }
            }
        }*/
    }

    void TargetsInView() //Adds and removes target objects from visibleTargets list if visible on screen or not.
    {
        for (int i = 0; i < targetLocations.Count; i++)
        {
            Vector3 targetPos = Camera.main.WorldToViewportPoint(targetLocations[i].transform.position);

            bool isVisible = (targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

            if (isVisible && !visibleTargets.Contains(targetLocations[i]))
            {
                visibleTargets.Add(targetLocations[i]);

            }
            else if (visibleTargets.Contains(targetLocations[i]) && !isVisible)
            {
                visibleTargets.Remove(targetLocations[i]);
            }
        }
    }

    void CheckTargetDistance()
    {
        //Check if in range to lock on to target.
        if (closest != null)
        {
            if (Vector3.Distance(transform.position, closest.transform.position) < range)
            {
                canLockOn = true;
            }
            else
            {
                canLockOn = false;
            }
        }
    }

    void AddLockOnMarker()
    {
        Vector3 markerPos = closest.transform.position;

        lockOnMarker = ObjectPoolManager.instance.CallObject("LockOnMarker", closest.transform, new Vector3(markerPos.x, markerPos.y + 2, markerPos.z), Quaternion.Euler(180, 0, 0));
    }

    void RemoveLockOnMarker()
    {
        ObjectPoolManager.instance.RecallObject(lockOnMarker);
    }
}
