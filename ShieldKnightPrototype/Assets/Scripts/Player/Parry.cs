using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parry : MonoBehaviour
{
    public float parryForce;
    Vector3 parryDirection;

    private void Update()
    {
        //Debug.DrawLine(transform.position, parryDirection * parryForce, Color.red);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Projectile"))
        {
            Debug.Log("Parry!!!");

            if (!other.gameObject.GetComponent<ParriedObject>())
            {
                other.gameObject.AddComponent<ParriedObject>();
            }

            Rigidbody projRB = other.GetComponent<Rigidbody>();

            /*if(projRB != null) 
            {
                Debug.Log(projRB.name);
            }*/

            parryDirection = projRB.transform.position + transform.position;

            projRB.AddForce(parryDirection * parryForce, ForceMode.Impulse);
        }
    }
}
