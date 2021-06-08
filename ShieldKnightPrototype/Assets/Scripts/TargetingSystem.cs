using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TargetingSystem : MonoBehaviour
{
    ShieldController shield;

    public List<GameObject> targets = new List<GameObject>();

    public GameObject closest;

    CapsuleCollider triggerCapsule;

    private void Start()
    {
        shield = GameObject.FindGameObjectWithTag("Shield").GetComponent<ShieldController>();
        triggerCapsule = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            triggerCapsule.enabled = true;

            FindClosestTarget();
        }
        else triggerCapsule.enabled = false;

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
        targets.Sort(delegate (GameObject a, GameObject b)
        {
            return Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(transform.position, b.transform.position));
        });

        closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject target in targets) //Measures distance from player to targets and calculates which target is closest
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
}
