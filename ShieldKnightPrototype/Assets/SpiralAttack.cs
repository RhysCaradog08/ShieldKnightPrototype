using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class SpiralAttack : MonoBehaviour
{
    ProjectileShieldController ps;

    Rigidbody rb;

    public float delay;

    public bool shot;

    private void Start()
    {
        ps = FindObjectOfType<ProjectileShieldController>();

        rb = GetComponent<Rigidbody>();

        delay = 0.25f;
    }

    private void Update()
    {
        if (shot)
        {
            delay -= Time.deltaTime;
        }

        if(delay <=  0)
        {
            delay = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if(delay <= 0)
        {
        Debug.Log("Hit something!");
            ObjectPoolManager.instance.RecallObject(ps.shieldProjectile1);
            ObjectPoolManager.instance.RecallObject(ps.shieldProjectile2);
            ObjectPoolManager.instance.RecallObject(ps.shieldProjectile3);

            ps.shieldProjectile1 = null;
            ps.shieldProjectile2 = null;
            ps.shieldProjectile3 = null;

            ps.projectiles.Clear();

            transform.position = ps.transform.position;
            shot = false;
            delay = 0.25f;
            rb.isKinematic = true;
        }
    }
}
