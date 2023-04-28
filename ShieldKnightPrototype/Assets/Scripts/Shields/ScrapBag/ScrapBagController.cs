using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapBagController : MonoBehaviour
{
    Rigidbody objectRB;
    public float rotateSpeed, suctionSpeed, suckRange;

    CapsuleCollider suckVortex;

    public bool isSucking;

    private void Awake()
    {
        suckVortex = GetComponent<CapsuleCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //SuckVortex
        suckVortex.height = suckRange;
        suckVortex.center = new Vector3(0, 0, suckRange/2 + 1);
        suckVortex.radius = 3;
        suckVortex.enabled = false; 

        isSucking = false;
    }

    // Update is called once per frame
    void Update()
    {
        float rotateY = Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime;
       
        transform.Rotate(transform.up, rotateY);

        if(Input.GetButton("Throw"))
        {
            isSucking = true;
        }
        else isSucking = false;

        if (isSucking)
        {
            suckVortex.enabled = true;
        }
        else suckVortex.enabled = false;
    }

    void SuckUp()
    {
        /*objectRB.isKinematic = false;

        Vector3 suctionDirection = objectRB.transform.position - transform.position;
        float objectDistance = suctionDirection.magnitude;
        Debug.Log(objectRB.name + " Distance " +  objectDistance);

        if(objectDistance > 0) 
        {
            objectRB.MovePosition(suctionDirection * suctionSpeed * Time.deltaTime);

        }*/

        Debug.DrawLine(transform.position, objectRB.transform.position, Color.yellow);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);

        if (other.GetComponent<Rigidbody>() != null)
        {
            Debug.Log("Object Rb: " + other.name);
            objectRB = other.GetComponent<Rigidbody>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (objectRB != null)
        {     
            SuckUp();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() == objectRB)
        {
            Debug.Log("Object Rb: " + other.name);

            objectRB = null;
        }
    }
}
