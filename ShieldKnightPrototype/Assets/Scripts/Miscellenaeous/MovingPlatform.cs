using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] GameObject player = null;

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {            
            player = other.gameObject;

            player.transform.parent = transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.transform.parent = null;

            player = null;
        }
    }
}
