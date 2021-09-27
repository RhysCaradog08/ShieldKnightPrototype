using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    TargetingSystem ts;

    public float health;

    private void Start()
    {
        ts = FindObjectOfType<TargetingSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            health = 0;

            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    void Die()
    {
        if(ts.lockedOn)
        {
            ts.lockedOn = false;
            ts.visibleTargets.Clear();
        }
        Destroy(this.gameObject);
    }
}
