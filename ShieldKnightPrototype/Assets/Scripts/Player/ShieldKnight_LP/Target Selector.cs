using Basics.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [SerializeField] StandardShieldController shield;
    [SerializeField] MushroomCapController mushroom;
    
    [SerializeField] Camera cam;
    public Transform player, aimTarget;


    Collider[] hitColliders;
    public List<GameObject> targetLocations = new List<GameObject>();
    [SerializeField] private float range;
    public float targetAngle;
    public GameObject closest;
    public LayerMask ignoreLayers;

    private void Awake()
    {
        shield = FindObjectOfType<StandardShieldController>();
        mushroom = FindObjectOfType<MushroomCapController>();

        player = GameObject.FindWithTag("Player").transform;

        cam = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonUp("Throw"))
        {
            ClearTargets();
        }

        if(closest != null)
        {
            if(shield.isActiveAndEnabled)
            {
                shield.target = closest;
            }
        }
    }

    public void FindTargets()
    {
        hitColliders = Physics.OverlapSphere(transform.position, range);

        foreach (Collider collider in hitColliders)
        {
            if (collider.tag == "Target")
            {
                Debug.DrawLine(transform.position, collider.transform.position, Color.red);

                Vector3 dir = collider.transform.position - transform.position;
                float angle = Vector3.Angle(dir, cam.transform.forward);

                ShowTargetAngle();

                if (angle < targetAngle && angle > -targetAngle)
                {
                    Debug.DrawLine(transform.position, collider.transform.position, Color.green);
                    if (!targetLocations.Contains(collider.gameObject))
                    {
                        targetLocations.Add(collider.gameObject);
                    }
                }
                else if (targetLocations.Contains(collider.gameObject))
                {
                    targetLocations.Remove(collider.gameObject);
                }
            }
            else if (targetLocations.Contains(collider.gameObject))
            {
                targetLocations.Remove(collider.gameObject);
            }
        }      
    }

    public void FindClosestTarget()
    {
        targetLocations.Sort(delegate (GameObject a, GameObject b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.transform.position)///////
            .CompareTo(
              Vector3.Distance(transform.position, b.transform.position));
        });

        closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject target in targetLocations) //Measures distance from player to targets and if it falls within an angle of focus then calculates which target is closest.
        {

            //Distance
            Vector3 directionToTarget = target.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest = target;
            }
        }
    }

    public void ClearTargets()
    {
        targetLocations.Clear();
        closest = null;
    }

    void ShowTargetAngle()
    {
        Vector3 line = transform.position + (cam.transform.forward * range);
        Vector3 plusRotatedLine = Quaternion.AngleAxis(targetAngle, cam.transform.up) * line;
        Vector3 minusRotatedLine = Quaternion.AngleAxis(-targetAngle, cam.transform.up) * line;

        Debug.DrawLine(transform.position, plusRotatedLine, Color.yellow);
        Debug.DrawLine(transform.position, minusRotatedLine, Color.yellow);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
