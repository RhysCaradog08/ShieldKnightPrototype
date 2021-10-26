using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParriedObject : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<EnemyHealth>())
        {
            //Debug.Log("Hit by Parried Object");

            EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

            enemy.TakeDamage(25);
        }
    }
}
