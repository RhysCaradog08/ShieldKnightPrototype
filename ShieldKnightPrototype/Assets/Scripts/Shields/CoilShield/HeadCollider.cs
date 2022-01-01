using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCollider : MonoBehaviour
{
    CoilShieldController coil;

    // Start is called before the first frame update
    void Start()
    {
        coil = FindObjectOfType<CoilShieldController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Target")
        {
            if(coil.canTether)
            {
                coil.tethered = true;
                coil.tetherPoint = other.gameObject.transform;
            }
        }
    }
}
