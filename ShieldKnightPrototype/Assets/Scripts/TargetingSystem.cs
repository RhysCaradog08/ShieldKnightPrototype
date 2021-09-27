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

        if(Input.GetButtonUp("Fire1")) //Clears targetLocations for the next instance.
        {
            targetLocations.Clear();
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
                    foreach (GameObject target in visibleTargets)
                    {
                        markerCheck = target.GetComponent<MarkerCheck>();

                        if (markerCheck.canAddMarker == false)
                        {
                            markerCheck.RemoveMarker();
                        }
                    }

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

    void GetTargets()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);

        foreach(Collider col in hitColliders)
        {
            if(col.tag == "Target")
            {
                if (!targetLocations.Contains(col.gameObject))
                {
                    targetLocations.Add(col.gameObject);

                    if (col.gameObject.GetComponent<MarkerCheck>() == null)
                    {
                        col.gameObject.AddComponent<MarkerCheck>();
                    }
                }
            }
        }
    }

    void TargetsInView() //Adds and removes target objects from visibleTargets list if visible on screen or not.
    {
        for (int i = 0; i < targetLocations.Count; i++)
        {
            Vector3 targetPos = Camera.main.WorldToViewportPoint(targetLocations[i].transform.position);
            Vector3 playerPos = Camera.main.WorldToViewportPoint(transform.position);

            bool isVisible = (targetPos.z > playerPos.z && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

            if (isVisible && !visibleTargets.Contains(targetLocations[i]))
            {
                visibleTargets.Add(targetLocations[i]);

                foreach (GameObject target in visibleTargets)
                {
                    markerCheck = target.GetComponent<MarkerCheck>();

                    if (markerCheck.canAddMarker == true)
                    {
                        markerCheck.AddMarker();
                    }
                }
            }
            else if (visibleTargets.Contains(targetLocations[i]) && !isVisible)
            {
                foreach (GameObject target in visibleTargets)
                {
                    markerCheck = target.GetComponent<MarkerCheck>();

                    if (markerCheck.canAddMarker == false)
                    {
                        markerCheck.RemoveMarker();
                    }
                }

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
