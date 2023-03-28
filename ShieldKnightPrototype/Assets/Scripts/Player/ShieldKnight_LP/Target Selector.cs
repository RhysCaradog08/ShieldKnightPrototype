using Basics.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [SerializeField] Camera cam;

    ShieldKnightController sk;
    public MarkerCheck markerCheck;
    StandardShieldController shield;

    Collider[] hitColliders;
    public List<GameObject> targetLocations = new List<GameObject>();

    public GameObject closest;

    Ray centreRay;

    [SerializeField] private float range;

    private void Awake()
    {
        cam = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton("Throw"))
        {
            FindTargets();

            Debug.DrawLine(transform.position, cam.transform.forward, Color.yellow);
        }

        if(Input.GetButtonUp("Throw"))
        {
            targetLocations.Clear();
        }
    }

    public void FindTargets()
    {
        hitColliders = Physics.OverlapSphere(transform.position, range);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Target"))
            {
                Debug.DrawLine(transform.position, collider.transform.position, Color.green);
                if (!targetLocations.Contains(collider.gameObject))
                {
                    targetLocations.Add(collider.gameObject);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
