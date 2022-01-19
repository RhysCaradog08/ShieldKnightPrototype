using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectIsHeld : MonoBehaviour
{
    public bool isHeld, isThrown;

    private void Start()
    {
        isHeld = false;
        isThrown = false;

        Physics.IgnoreLayerCollision(12, 3);
    }

    private void OnCollisionEnter(Collision col)
    {
        if(isThrown)
        {
            Debug.Log("Has collided with: " + col.gameObject.name);

            if(col.gameObject.GetComponent<MarkerCheck>() != null)
            {
                Debug.Log("Hit Marker");
                MarkerCheck markerCheck = col.gameObject.GetComponent<MarkerCheck>();

                if(!markerCheck.canAddMarker)
                {
                    markerCheck.RemoveMarker();
                }
            }

            isThrown = false;
        }
    }
}
