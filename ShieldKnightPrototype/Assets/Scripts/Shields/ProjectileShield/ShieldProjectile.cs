using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ShieldProjectile : MonoBehaviour
{
    GameObject shieldP;

    private void Start()
    {
        shieldP = this.gameObject;
        Physics.IgnoreLayerCollision(10, 10);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        ObjectPoolManager.instance.RecallObject(shieldP);

    }
}
