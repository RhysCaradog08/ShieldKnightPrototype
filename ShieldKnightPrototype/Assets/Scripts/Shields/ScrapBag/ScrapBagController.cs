using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapBagController : MonoBehaviour
{
    Rigidbody objectRB;
    public float rotateSpeed, suctionSpeed, suckRange;

    CapsuleCollider suckVortex;
    [SerializeField] List <Rigidbody> inVortex = new List <Rigidbody>();
    [SerializeField] List <Rigidbody> inBag = new List <Rigidbody>();

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
        else
        {
            suckVortex.enabled = false;
            inVortex.Clear();
        }
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
        foreach (Rigidbody rb in inVortex)
        {
            Debug.DrawLine(transform.position, rb.transform.position, Color.yellow);

            objectRB = rb;
            objectRB.transform.parent = null;

            if(objectRB.isKinematic == true)
            {
               objectRB.isKinematic = false;
            }

            Vector3 suctionDirection = objectRB.transform.position - transform.position;
            float objectDistance = Vector3.Distance(transform.position, objectRB.transform.position);

            objectRB.transform.position = Vector3.Lerp(objectRB.transform.position, transform.position, suctionSpeed * Time.deltaTime);

            if (objectDistance < 3)
            {
                objectRB.transform.position = transform.position;
                objectRB.isKinematic = true;
                objectRB.gameObject.SetActive(false);

                if (!inBag.Contains(objectRB))
                {
                    inBag.Add(objectRB);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);

        if (other.GetComponent<Rigidbody>() != null)
        {
            if (!inVortex.Contains(other.GetComponent<Rigidbody>()))
            {
                inVortex.Add(other.GetComponent<Rigidbody>());               
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (inVortex.Count > 0)
        {     
            SuckUp();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Remove " + other.name);
        if (other.GetComponent<Rigidbody>() != null)
        {
            if (inVortex.Contains(other.GetComponent<Rigidbody>()))
            {
                inVortex.Remove(other.GetComponent<Rigidbody>());
            }
        }
    }
}
