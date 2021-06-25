using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parry : MonoBehaviour
{
    public float parryForce;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Projectile"))
        {
            Debug.Log("Parry!!!");

            Rigidbody projRb = other.GetComponent<Rigidbody>();

            projRb.AddForce(transform.forward * parryForce, ForceMode.Impulse);
        }
    }
}
