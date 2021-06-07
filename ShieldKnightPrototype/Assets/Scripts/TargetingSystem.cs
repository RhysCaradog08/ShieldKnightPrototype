using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    ShieldController shield;

    public List<GameObject> targets = new List<GameObject>();

    [SerializeField]
    GameObject closest;

    private void Start()
    {
        shield = GameObject.FindGameObjectWithTag("Shield").GetComponent<ShieldController>();
    }

    private void Update()
    {
        if (targets.Count > 0)
        {
            shield.target = closest.transform;
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

            closest = targets[0];
            /*foreach (GameObject target in GameObject.FindGameObjectsWithTag("Target"))
            {
                targets.Add(target);
            }*/
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            targets.Remove(other.gameObject);
            /*foreach (GameObject target in GameObject.FindGameObjectsWithTag("Target"))
            {
                targets.Remove(target);
            }*/
        }
    }
}
