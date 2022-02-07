using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class ShieldProjectile : MonoBehaviour
{
    ProjectileShieldController psc;

    Transform player;
    public GameObject target;
    GameObject shieldP, hitStars;
    Vector3 scale;
    Rigidbody rb;

    public float shotForce, interactDelay;

    public bool shot;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>().transform;
        shieldP = this.gameObject;
        scale = transform.localScale;
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
        }

        if(interactDelay <= 0)
        {
            interactDelay = 0;
        }
    }

    private void FixedUpdate()
    {
        if(shot)
        {
            rb.isKinematic = false;

            if (target != null)
            {
                Vector3 shootDir = (target.transform.position - transform.position).normalized;

                rb.MovePosition(transform.position + shootDir * (shotForce * 3) * Time.deltaTime);
            }
            else rb.AddForce(player.forward * (shotForce), ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null)
        {
            if(shot) //&& other == target)
            {
                //hit = true;
                shot = false;
                target = null;

                rb.isKinematic = true;

                Debug.Log("Hit Target!");
            }
        }
        else if(!target)
        {
            if (shot)
            {
                //hit = true;
                shot = false;

                rb.isKinematic = true;

                Debug.Log("Hit Something!");
            }
        }


        if (interactDelay == 0)
        {
            if (other.gameObject.GetComponent<MarkerCheck>() != null)
            {
                MarkerCheck markerCheck = other.gameObject.GetComponent<MarkerCheck>();

                if (!markerCheck.canAddMarker)
                {
                    markerCheck.RemoveMarker();
                }
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

            hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, transform.position, Quaternion.identity, 1);
            ObjectPoolManager.instance.RecallObject(shieldP);
        }
    }
}
