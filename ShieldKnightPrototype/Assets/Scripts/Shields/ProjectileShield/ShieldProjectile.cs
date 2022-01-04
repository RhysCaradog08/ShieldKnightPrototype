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

    public bool shot, hit;

    private void Awake()
    {
        shieldP = this.gameObject;
        scale = transform.localScale;
        shot = false;
        hit = false;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Physics.IgnoreLayerCollision(10, 3);
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

        if (hit && interactDelay <= 0)
        {
            hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, transform.position, Quaternion.identity, 1);

            rb.isKinematic = true;
            shot = false;
            hit = false;

            ObjectPoolManager.instance.RecallObject(shieldP);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(shot)
        {
            hit = true;
            //Debug.Log("Hit Something!");
        }

        if(interactDelay <= 0)
        {
            /*hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, transform.position, Quaternion.identity, 1);

            rb.isKinematic = true;
            shot = false;*/

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

            if (other.transform.gameObject.GetComponent<Lever>())
            {
                Lever lever = other.transform.gameObject.GetComponent<Lever>();

                if (lever.canChange)
                {
                    lever.ChangeLever();
                }
            }

        }
            //ObjectPoolManager.instance.RecallObject(shieldP);
    }
}
