using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    ShieldController shield;

    public List<GameObject> targets = new List<GameObject>();

    [SerializeField]
    GameObject closest;

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
        }
        else triggerCapsule.enabled = false;

        if (targets.Count > 0)
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

            closest = targets[0];
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            targets.Remove(other.gameObject);
        }
    }
}
