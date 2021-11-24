using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ProjectileGuard : MonoBehaviour
{
    ProjectileShieldController PS;

    GameObject enemyProjectile;
    GameObject shieldProjectile;

    private void Start()
    {
        PS = FindObjectOfType<ProjectileShieldController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            enemyProjectile = other.gameObject;
            ObjectPoolManager.instance.RecallObject(enemyProjectile);

            shieldProjectile = PS.projectiles[0];
            PS.projectiles.Remove(shieldProjectile);

            if(shieldProjectile == PS.shieldProjectile1)
            {
                PS.shieldProjectile1 = null;
            }

            if (shieldProjectile == PS.shieldProjectile2)
            {
                PS.shieldProjectile2 = null;
            }

            if (shieldProjectile == PS.shieldProjectile3)
            {
                PS.shieldProjectile3 = null;
            }

            ObjectPoolManager.instance.RecallObject(shieldProjectile);
        }
    }
}
