using Basics.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [SerializeField] PlayerManager pm;
    
    [SerializeField] Camera cam;
    public Transform player, aimTarget;

    [SerializeField] GameObject lockOnMarker;

    Collider[] hitColliders;
    public List<GameObject> targetLocations = new List<GameObject>();
    [SerializeField] private float range;
    public float targetAngle;
    public GameObject closest;
    public LayerMask ignoreLayers;

    public bool canTarget, canLockOn, lockedOn;

    private void Awake()
    {
        pm = FindObjectOfType<PlayerManager>();

        player = GameObject.FindWithTag("Player").transform;

        cam = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        canTarget = true;
        canLockOn = true;
        lockedOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (closest != null && lockedOn)
        {
            transform.LookAt(closest.transform.position);
            canTarget = false;
            canLockOn = false;
        }
        else if (!lockedOn && !closest)
        {
            canTarget = true;
            canLockOn = true;
        }

        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            if (!lockedOn)
            {
                FindTargets();
                FindClosestTarget();

                if (canLockOn)
                {
                    Debug.Log("Lock On Target");
                    AddLockOnMarker();
                    lockedOn = true;
                }
            }
            else if (lockedOn)
            {
                lockedOn = false;
                ClearTargets();
                RemoveLockOnMarker();
            }
        }

        if (Input.GetKeyUp(KeyCode.Z))
        {
            targetLocations.Clear();
        }

        if (lockedOn && Vector3.Distance(transform.position, closest.transform.position) > range)
        {
            canLockOn = true;
            lockedOn = false;
            closest = null;
            RemoveLockOnMarker();
        }

        if (Input.GetButtonUp("Throw"))
        {
            if (!lockedOn)
            {
                ClearTargets();
            }
        }

        if (closest != null)
        {
            if(pm.hasShield)
            {
                pm.shield.target = closest;
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

    void AddLockOnMarker()
    {
        Vector3 markerPos = closest.transform.position;

        lockOnMarker = ObjectPoolManager.instance.CallObject("LockOnMarker", closest.transform, new Vector3(markerPos.x, markerPos.y + (closest.transform.localScale.y + 3.5f), markerPos.z), Quaternion.Euler(180, 0, 0));
    }

    void RemoveLockOnMarker()
    {
        ObjectPoolManager.instance.RecallObject(lockOnMarker);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
