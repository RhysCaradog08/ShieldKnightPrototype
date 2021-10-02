using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    TargetingSystem ts;

    public float health;

    public bool squashed = false;

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

    public void Squash()
    {
        transform.localScale = new Vector3(transform.localScale.x, 0.1f, transform.localScale.z);

        transform.Translate(-Vector3.up * 10 *Time.deltaTime);

        squashed = true;

        Invoke("SquashDamage", 0.5f);
    }

    void SquashDamage()
    {
        TakeDamage(10);
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
