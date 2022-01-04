using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;
using System;

public class TargetingSystem : MonoBehaviour
{
    PlayerController player;
    Camera cam;

    public MarkerCheck markerCheck;
    ShieldController shield;
    ProjectileShieldController projectile;
    CoilShieldController coil;

    Collider[] hitColliders;

    public List<GameObject> targetLocations = new List<GameObject>(), visibleTargets = new List<GameObject>();

    public GameObject closest;

    [SerializeField] private float range, targetFOV;

    [Header("UI")]
    public GameObject targetMarker, lockOnMarker;

    [Header("Booleans")]
    [SerializeField] bool canLockOn, canTarget, isVisible;
    public bool lockedOn;

    private void Awake()
    {
        player = GameObject.FindObjectOfType<PlayerController>();
        cam = Camera.main;

        shield = FindObjectOfType<ShieldController>();
        projectile = FindObjectOfType<ProjectileShieldController>();
        coil = FindObjectOfType<CoilShieldController>();
    }

    void Start()
    {
        canLockOn = true;
        canTarget = true;        
    }

    private void Update()
    {
        if (closest != null && lockedOn)
        {
            transform.LookAt(new Vector3(closest.transform.position.x, transform.position.y, closest.transform.position.z));
        }

        if (Input.GetButtonDown("Throw") && !shield.thrown)
        {
            if (!lockedOn)
            {
                GetTargets();
            }
        }

        if (Input.GetButton("Throw") && !shield.thrown) //Determine which targets fall within range and which is closest.
        {
            if (!lockedOn)
            {
                TargetsInView();
                FindClosestTarget();
            }
        }

        if (Input.GetButtonUp("Throw")) //Clears targetLocations for the next instance.
        {
            targetLocations.Clear();
        }

        if(player.hasProjectile)
        {
            if (Input.GetButtonDown("Barge"))
            {
                if (!lockedOn)
                {
                    GetTargets();
                }
            }

            if (Input.GetButton("Barge"))
            {
                if (!lockedOn)
                {
                    TargetsInView();
                    FindClosestTarget();
                }
            }

            if (Input.GetButtonUp("Barge"))
            {
                targetLocations.Clear();
            }
        }

        if(player.hasCoil)
        {
            if (Input.GetButtonDown("Barge"))
            {
                if(coil.canExtend && !coil.hasObject)
                {
                    if (!lockedOn)
                    {
                        GetTargets();
                    }
                }
            }

            if (Input.GetButton("Barge"))
            {
                if (!lockedOn)
                {
                    TargetsInView();
                    FindClosestTarget();
                }
            }

            if (Input.GetButtonUp("Barge"))
            {
                targetLocations.Clear();
            }

            if(!coil.extending && !coil.canExtend)
            {
                if(!lockedOn)
                {
                    if (visibleTargets.Count > 0)
                    {
                        foreach (GameObject target in visibleTargets)
                        {
                            markerCheck = target.GetComponent<MarkerCheck>();

                            if (markerCheck != null)
                            {
                                markerCheck.RemoveMarker();
                            }
                        }
                    }
                }
                closest = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            canTarget = false;

            if (!lockedOn)
            {
                if (player.hasProjectile)
                {
                    if (projectile.projectiles.Count > 0)
                    {
                        GetTargets();

                        TargetsInView();
                        FindClosestTarget();
                        CheckTargetDistance();
                    }
                }
                else
                {
                    GetTargets();

                    TargetsInView();
                    FindClosestTarget();
                    CheckTargetDistance();
                }

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
            canTarget = true;
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
            if(player.hasShield)
            {
                shield.target = closest;
            }

            if (player.hasProjectile)
            {
                projectile.target = closest;
            }

            if(player.hasCoil)
            {
                coil.target = closest;
            }
        }
        else
        {
            shield.target = null;
            projectile.target = null;
            coil.target = null;
        }
    }

    void FindClosestTarget()
    {
        visibleTargets.Sort(delegate (GameObject a, GameObject b) //Sorts targets by distance between player and object transforms.
        {
            return Vector3.Distance(transform.position, a.transform.position)///////
            .CompareTo(
              Vector3.Distance(transform.position, b.transform.position));
        });

        closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject target in visibleTargets) //Measures distance from player to targets and if it falls within an angle of focus then calculates which target is closest.
        {
            Debug.DrawLine(transform.position, target.transform.position, Color.red);

            markerCheck = target.GetComponent<MarkerCheck>();

            if(canTarget)
            {
                if (markerCheck != null)
                {
                    if (markerCheck.canAddMarker == true)
                    {
                        markerCheck.AddMarker();
                    }
                }
            }

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

    void GetTargets()
    {
        if (visibleTargets.Count > 0)
        {
            visibleTargets.Clear();
        }

        hitColliders = Physics.OverlapSphere(transform.position, 100);

        foreach (Collider col in hitColliders)
        {
            if (col.tag == "Target")
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

        Array.Clear(hitColliders, 0, hitColliders.Length);
    }

    void TargetsInView() //Adds and removes target objects from visibleTargets list if visible on screen or not.
    {
        for (int i = 0; i < targetLocations.Count; i++)
        {
            Vector3 targetPos = Camera.main.WorldToViewportPoint(targetLocations[i].transform.position);
            Vector3 playerPos = Camera.main.WorldToViewportPoint(transform.position);

            isVisible = (targetPos.z > playerPos.z && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1) ? true : false;

            if (isVisible && !visibleTargets.Contains(targetLocations[i]))
            {
                if (Vector3.Distance(transform.position, targetLocations[i].transform.position) < range)
                {
                    if (player.hasShield)
                    {
                        if (visibleTargets.Count < 3)
                        {
                            visibleTargets.Add(targetLocations[i]);
                        }
                    }
                    else if(player.hasProjectile)
                    {
                        if(visibleTargets.Count < projectile.projectiles.Count)
                        {
                            visibleTargets.Add(targetLocations[i]);
                        }
                    }
                    else if(player.hasCoil)
                    {
                        if (visibleTargets.Count < 1)
                        {
                            visibleTargets.Add(targetLocations[i]);
                        }
                    }

                    foreach (GameObject target in visibleTargets)
                    {
                        markerCheck = target.GetComponent<MarkerCheck>();//////
                        
                        if(canTarget)
                        {
                            if (markerCheck.canAddMarker == true)
                            {
                                markerCheck.AddMarker();
                            }
                        }
                    }
                }
            }
            else if (isVisible && visibleTargets.Contains(targetLocations[i]))
            {
                if (Vector3.Distance(transform.position, targetLocations[i].transform.position) > range)
                {
                    foreach (GameObject target in visibleTargets)
                    {
                        markerCheck = target.GetComponent<MarkerCheck>();///////
                        markerCheck.RemoveMarker();
                    }

                    visibleTargets.Remove(targetLocations[i]);
                }
            }
            else if (visibleTargets.Contains(targetLocations[i]) && !isVisible)
            {
                foreach (GameObject target in visibleTargets)
                {
                    markerCheck = target.GetComponent<MarkerCheck>();
                    markerCheck.RemoveMarker();
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

        lockOnMarker = ObjectPoolManager.instance.CallObject("LockOnMarker", closest.transform, new Vector3(markerPos.x, markerPos.y + (closest.transform.localScale.y + 1), markerPos.z), Quaternion.Euler(180, 0, 0));
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
        Gizmos.DrawWireSphere(transform.position, 100);
    }
}
