using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class TargetingSystem : MonoBehaviour
{
    ShieldController shield;
    GameObject player;

    public List<GameObject> targets = new List<GameObject>();

    public GameObject closest;

    CapsuleCollider triggerCollider;

    [Header("UI")]
    public GameObject pMarker;
    GameObject one;
    GameObject two;
    GameObject three;

    private void Start()
    {
        shield = GameObject.FindObjectOfType<ShieldController>();
        player = GameObject.FindGameObjectWithTag("Player");

        triggerCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && !shield.thrown) //Enables trigger to determine which targets fall within it's area.
        {
            triggerCollider.enabled = true;

            FindClosestTarget();

            if (pMarker == null || pMarker.transform.position == Vector3.zero)
            {
                AddTargetMarker();
            }
        }
        else triggerCollider.enabled = false;

        if (targets.Count > 0) //If there are targets in list, shield.target will be the closest.
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("In Trigger");

        if (other.gameObject.CompareTag("Target"))
        {
            targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            targets.Remove(other.gameObject);
        }        
    }

    void FindClosestTarget()
    {
        targets.Sort(delegate (GameObject a, GameObject b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(transform.position, b.transform.position));
        });

        closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject target in targets) //Measures distance from player to targets and calculates which target is closest.
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
        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 targetPos = targets[i].transform.position;

            pMarker = ObjectPoolManager.instance.CallObject("P_Marker", targets[i].transform, new Vector3(targetPos.x, targetPos.y + 2, targetPos.z - 0.65f), Quaternion.identity);
        }
    }
}
