using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class HeadCollider : MonoBehaviour
{
    CoilShieldController coil;
    public bool grappleFixed, grappleLoose;

    GameObject hitStars;

    void Awake()
    {
        coil = FindObjectOfType<CoilShieldController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        grappleFixed = false;
        grappleLoose = false;

        Physics.IgnoreLayerCollision(13, 3);
    }

    void Update()
    {
        //Debug.DrawLine(transform.position, transform.forward * 10, Color.blue);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Target")
        {
            //Debug.Log("Target: " + other.name);

            if (coil.canTether)
            {
                if (other.transform.gameObject.GetComponent<Lever>())
                {
                    Lever lever = other.transform.gameObject.GetComponent<Lever>();

                    if (lever.canChange)
                    {
                        lever.ChangeLever();
                    }
                }
                else
                {
                    coil.isTethered = true;
                    coil.tetherPoint = other.gameObject.transform;
                }

                if (other.gameObject.layer == 11)//GrappleFixed
                {
                    //Debug.Log("Grapple to Target");
                    grappleFixed = true;
                }

                if (other.gameObject.layer == 12)//GrappleLoose
                {
                    grappleLoose = true;
                    coil.tetheredObject = other.gameObject;
                }
            }
        }

        if (coil.isExtending && !coil.isSpringing && !coil.canTether)
        {

            if (other.gameObject.GetComponent<EnemyHealth>())
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

            hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, other.transform.position, Quaternion.identity, 1);
        }

    }
}
