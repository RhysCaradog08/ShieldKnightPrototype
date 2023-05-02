using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScrapBagController : MonoBehaviour
{
    public float rotateSpeed, suctionSpeed, suckRange;

    [Header("Suck Vortex")]
    [SerializeField] List <Rigidbody> inVortex = new List <Rigidbody>();
    [SerializeField] List <Rigidbody> inBag = new List <Rigidbody>();
    [SerializeField] int bagMaxCapacity;
    CapsuleCollider suckVortex;
    Rigidbody objectRB;

    [Header("Shoot Projectile")]
    public float shootForce, shotFrequency;
    private float shotDelay = 0;

    public bool enableVortex, canShootRepeatedly;

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

        enableVortex = false;
        canShootRepeatedly = false;
    }

    // Update is called once per frame
    void Update()
    {
        float rotateY = Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime;
       
        transform.Rotate(transform.up, rotateY);

        RepeatedShotCheck();

        if(Input.GetButton("Throw") && Time.time >= shotDelay)
        {
            if(inBag.Count > 0)
            {
                Debug.Log("Shoot");
                ShootProjectile();
                shotDelay = Time.time + 1f / shotFrequency;
            }
        }

        if (Input.GetButton("Guard"))
        {
            if (inBag.Count < bagMaxCapacity)
            {
                enableVortex = true;
            }
        }
        else enableVortex = false;
        
        if (inBag.Count >= bagMaxCapacity)
        {
            enableVortex = false;
        }

        if (enableVortex)
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
                objectRB.transform.parent = this.transform;
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

    void ShootProjectile()
    {
        objectRB = inBag[0].GetComponent<Rigidbody>();

        objectRB.gameObject.SetActive(true);
        objectRB.isKinematic = false;
        objectRB.transform.parent = null;


        objectRB.AddForce(transform.forward * shootForce, ForceMode.Impulse);

        if (inBag.Contains(objectRB))
        {
            inBag.Remove(objectRB);
        }
    }

    IEnumerator RepeatedShots()
    {
        Debug.Log("Keep Shooting");
        ShootProjectile();


        yield return new WaitForSeconds(shotDelay);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name);

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
        //Debug.Log("Remove " + other.name);
        if (other.GetComponent<Rigidbody>() != null)
        {
            if (inVortex.Contains(other.GetComponent<Rigidbody>()))
            {
                inVortex.Remove(other.GetComponent<Rigidbody>());
            }
        }
    }
}
