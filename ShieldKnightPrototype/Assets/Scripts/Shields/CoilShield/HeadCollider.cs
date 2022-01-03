using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class HeadCollider : MonoBehaviour
{
    CoilShieldController coil;
    public bool grappleFixed, grappleLoose;

    GameObject hitStars;

    // Start is called before the first frame update
    void Start()
    {
        coil = FindObjectOfType<CoilShieldController>();
        grappleFixed = false;
        grappleLoose = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Target")
        {
            if(coil.canTether)
            {
                coil.tethered = true;
                coil.tetherPoint = other.gameObject.transform;

                if (other.gameObject.layer == 11)//GrappleFixed
                {
                    grappleFixed = true;
                }

                if (other.gameObject.layer == 12)//GrappleLoose
                {
                    grappleLoose = true;
                    coil.tetheredObject = other.gameObject;
                }
            }
        }

        if (coil.whipping && !coil.canTether)
        {
            if (other.gameObject.GetComponent<EnemyHealth>())
            {
                EnemyHealth enemy = other.gameObject.GetComponent<EnemyHealth>();

                enemy.TakeDamage(10);
            }

            hitStars = ObjectPoolManager.instance.CallObject("HitStars", null, other.transform.position, Quaternion.identity, 1);
        }
    }
}