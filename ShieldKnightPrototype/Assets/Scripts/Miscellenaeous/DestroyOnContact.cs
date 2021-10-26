using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class DestroyOnContact : MonoBehaviour
{
    GameObject projectile;
    private void Start()
    {
        projectile = gameObject;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            ObjectPoolManager.instance.RecallObject(projectile);
        }
    }
}
