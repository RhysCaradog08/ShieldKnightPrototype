using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    public ShieldController shield;

    public float throwRange;
    float distance;
    public GameObject[] shieldTargets;
    List<GameObject> targets = new List<GameObject>();
    GameObject closestTarget;
    public LayerMask targetMask;
    public int targetsLeft;

    private void Start()
    {
        //shield = GameObject.FindGameObjectWithTag("Shield").GetComponent<ShieldController>();
    }

    void Update()
    {
        //FindTargets();

        float distance = Vector3.Distance(transform.position, shield.target.position);

        if (distance < throwRange)
        {
            FindTargets();
            shield.hasTarget = true;
        }
        else shield.hasTarget = false;
    }

    void FindTargets()
    {
        shieldTargets = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];

        foreach (GameObject shieldTarget in shieldTargets)
        {
            if (shieldTarget.layer == targetMask)
            {
                Debug.Log(shieldTarget.name);

                shield.target = shieldTarget.transform;

                targetsLeft = shieldTargets.Length;
            }
        }
    }
}
