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

    CapsuleCollider triggerCapsule;

    [Header("UI")]
    public GameObject pMarker;
    GameObject one;
    GameObject two;
    GameObject three;

    private void Start()
    {
        shield = GameObject.FindObjectOfType<ShieldController>();
        player = GameObject.FindGameObjectWithTag("Player");

        triggerCapsule = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && !shield.thrown) //Enables trigger to determine which targets fall within it's area.
        {
            triggerCapsule.enabled = true;

            FindClosestTarget();

            if (pMarker == null || pMarker.transform.position == Vector3.zero)
            {
                AddTargetMarker();
            }
        }
        else triggerCapsule.enabled = false;

        if (targets.Count > 0) //If there are targets in list, shield.target will be the closest.
        {
            shield.target = closest;

            //AddTargetNumber();
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
        /*Vector3 firstPos = targets[0].transform.position;
        Vector3 secondPos = targets[1].transform.position;
        Vector3 thirdPos = targets[2].transform.position;

        Vector3 firstRot = new Vector3(targets[0].transform.rotation.x, 180, targets[0].transform.rotation.z);
        Vector3 secondRot = new Vector3(targets[1].transform.rotation.x, 180, targets[1].transform.rotation.z);
        Vector3 thirdRot = new Vector3(targets[2].transform.rotation.x, 180, targets[2].transform.rotation.z);

        one = ObjectPoolManager.instance.CallObject(("Number_1"), targets[0].transform, new Vector3(firstPos.x, firstPos.y + 3, firstPos.z - 0.65f), Quaternion.Euler(firstRot));
        two = ObjectPoolManager.instance.CallObject(("Number_2"), targets[1].transform, new Vector3(secondPos.x, secondPos.y + 3, secondPos.z - 0.65f), Quaternion.Euler(secondRot));
        three = ObjectPoolManager.instance.CallObject(("Number_3"), targets[2].transform, new Vector3(thirdPos.x, thirdPos.y + 3, thirdPos.z - 0.65f), Quaternion.Euler(thirdRot));*/

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 targetPos = targets[i].transform.position;

            pMarker = ObjectPoolManager.instance.CallObject("P_Marker", targets[i].transform, new Vector3(targetPos.x, targetPos.y + 2, targetPos.z - 0.65f), Quaternion.identity);
        }
    }
}
