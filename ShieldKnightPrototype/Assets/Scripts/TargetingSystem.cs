using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;
using System;

public class TargetingSystem : MonoBehaviour
{
    ShieldController shield;
    Camera cam;

    public List<GameObject> targetsInRange = new List<GameObject>();

    [SerializeField] GameObject [] taggedTargets;

    public GameObject closest;

    [SerializeField] private float range;
    [SerializeField] private float targetFOV;

    [Header("UI")]
    public GameObject targetMarker;

    private void Start()
    {
        shield = GameObject.FindObjectOfType<ShieldController>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && !shield.thrown) //Determine which targets fall within range and which is closest.
        {
            GetTargets();

            FindClosestTarget();
        }

        if(Input.GetButtonUp("Fire1"))
        {
            Array.Clear(taggedTargets, 0, taggedTargets.Length);
        }


        if (targetsInRange.Count > 0) //If there are targets in list, shield.target will be the closest.
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
        targetsInRange.Sort(delegate (GameObject a, GameObject b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(transform.position, b.transform.position));
        });

        closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject target in targetsInRange) //Measures distance from player to targets and calculates which target is closest.
        {
            Vector3 directionToTarget = target.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest = target;
            }
        }
    }

    void AddTargetMarker()
    {
        for (int i = 0; i < targetsInRange.Count; i++)
        {
            Vector3 markerPos = targetsInRange[i].transform.position;

            targetMarker = ObjectPoolManager.instance.CallObject("TargetMarker", targetsInRange[i].transform, new Vector3(markerPos.x, markerPos.y + 2, markerPos.z - 0.65f), Quaternion.identity);  
        }
    }

    void GetTargets()
    {
        taggedTargets = GameObject.FindGameObjectsWithTag("Target");

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

                    /*if (targetMarker == null || targetMarker.transform.position == Vector3.zero)
                    {
                        AddTargetMarker();
                    }*/
                }            
            }
            else if ((angle > targetFOV) || Vector3.Distance(transform.position, go.transform.position) > range)
            {
                targetsInRange.Remove(go);

                if (targetMarker != null)
                {
                    ObjectPoolManager.instance.RecallObject(targetMarker);
                }
            }
        }
    }
}
