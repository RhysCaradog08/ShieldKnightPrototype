using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ShieldProjectile : MonoBehaviour
{
    GameObject shieldP;
    Vector3 scale;
    Rigidbody rb;

    float interactDelay;

    public bool shot;

    private void Awake()
    {
        scale = transform.localScale;
        interactDelay = 0.5f;
        shot = false;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        shieldP = this.gameObject;
        Physics.IgnoreLayerCollision(10, 10);
    }

    private void Update()
    {
        transform.localScale = scale;

        if(shot)
        {
            interactDelay -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(interactDelay <= 0)
        {
            rb.isKinematic = true;

            ObjectPoolManager.instance.RecallObject(shieldP);
        }
    }
}
