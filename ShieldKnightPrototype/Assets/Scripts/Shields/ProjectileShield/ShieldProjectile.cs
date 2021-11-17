using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ShieldProjectile : MonoBehaviour
{
    GameObject shieldP;
    Vector3 scale;
    Rigidbody rb;

    GameObject hitStars;

    public float interactDelay;

    public bool shot;

    private void Awake()
    {
        scale = transform.localScale;
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
            //Debug.Log("Interact Delay: " + interactDelay);
        }

        if(interactDelay <= 0)
        {
            interactDelay = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Hit Something!");
        if(interactDelay <= 0)
        {
            hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, transform.position, Quaternion.identity, 1);

            rb.isKinematic = true;
            shot = false;

            if (other.gameObject.GetComponent<MarkerCheck>() != null)
            {
                MarkerCheck markerCheck = other.gameObject.GetComponent<MarkerCheck>();

                markerCheck.RemoveMarker();
            }

            if (other.gameObject.GetComponent<EnemyHealth>() != null)
            {
                EnemyHealth enemy = other.gameObject.GetComponent<EnemyHealth>();

                enemy.TakeDamage(10);
            }

            ObjectPoolManager.instance.RecallObject(shieldP);
        }
    }
}
