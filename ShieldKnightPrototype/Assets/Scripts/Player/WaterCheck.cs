using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCheck : MonoBehaviour
{
    PlayerController pc;

    // Start is called before the first frame update
    void Start()
    {
        pc = FindObjectOfType<PlayerController>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!pc.inWater)
            {
                pc.inWater = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (pc.inWater)
            {
                pc.inWater = false;
            }
        }
    }
}
