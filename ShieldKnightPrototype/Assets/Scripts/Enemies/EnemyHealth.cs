using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class EnemyHealth : MonoBehaviour
{
    TargetingSystem ts;

    GameObject cloud;

    Rigidbody rb;

    public float health;

    Vector3 fullSize;

    public bool squashed = false;

    private void Start()
    {
        ts = FindObjectOfType<TargetingSystem>();
        fullSize = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        rb = GetComponent<Rigidbody>();
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

        //Debug.Log(gameObject.name + "Damage Taken: " + damage);
    }

    public void Squash()
    {
        squashed = true;

        rb.isKinematic = true;

        transform.localScale = new Vector3(fullSize.x, 0.1f, fullSize.z);

        transform.Translate(Vector3.down * 9.81f *Time.deltaTime);

        Invoke("SquashDamage", 0.5f);
    }

    void SquashDamage()
    {
        TakeDamage(10);
        UnSquash();
    }

    void UnSquash()
    {
        transform.localScale = new Vector3(fullSize.x, fullSize.y, fullSize.z);
        rb.isKinematic = false;

        squashed = false;
    }

    void Die()
    {
        if(ts.lockedOn)
        {
            ts.lockedOn = false;
            ts.visibleTargets.Clear();
        }

        cloud = ObjectPoolManager.instance.CallObject("Cloud", null, transform.position, Quaternion.identity, 1);

        Destroy(this.gameObject);
    }
}
