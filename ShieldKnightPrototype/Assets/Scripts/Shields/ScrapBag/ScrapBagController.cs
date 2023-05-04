using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScrapBagController : MonoBehaviour
{
    ShieldSelect select;
    AnimationController animControl;

    [Header("Vortex")]
    public float suctionSpeed, suctionRange;
    [SerializeField] List <Rigidbody> inVortex = new List <Rigidbody>();
    [SerializeField] List <Rigidbody> inBag = new List <Rigidbody>();
    [SerializeField] int bagMaxCapacity;
    CapsuleCollider vortex;
    Rigidbody objectRB;

    [Header("Shoot Projectile")]
    public float shootForce, shotFrequency;
    [SerializeField] private float repeatShotDelay = 0, canShootDelay;

    [Header("Scale")]
    Vector3 bagEmptyScale = Vector3.one;

    public bool isAiming, enableVortex, canShoot;

    private void Awake()
    {
        select = FindObjectOfType<ShieldSelect>();
        animControl= FindObjectOfType<AnimationController>();

        vortex = GetComponent<CapsuleCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //SuckVortex
        vortex.height = suctionRange;
        vortex.center = new Vector3(0, 0, suctionRange/2 + 1);
        vortex.radius = 3;

        //Shoot Projectile
        canShootDelay = 0;

        isAiming = false;
        enableVortex = false;
        canShoot = false;
    }

    // Update is called once per frame
    void Update()
    {
        ScaleControl();

        if (Input.GetButton("Throw"))
        {
            if (inBag.Count > 0)
            {
                isAiming = true;

                if (Time.time >= repeatShotDelay)
                {
                    Debug.Log("Shoot");
                    ShootProjectile();
                    repeatShotDelay = Time.time + 1 / shotFrequency;
                }
            }
            else if(inBag.Count < 1)
            {
                isAiming = false;
            }
        }

        if (Input.GetButton("Guard"))
        {
            if (inBag.Count < bagMaxCapacity)
            {
                isAiming = true;
                enableVortex = true;
            }
        }
        else enableVortex = false;

        if (isAiming)
        {
            select.canChange = false;

            animControl.ChangeAnimationState(animControl.scrapBagAim);

            if (Input.GetButtonUp("Throw") || Input.GetButtonUp("Guard"))
            {
                isAiming = false;
            }
        }
        else select.canChange = true;
        
        if (inBag.Count >= bagMaxCapacity)
        {
            enableVortex = false;
        }

        if (enableVortex)
        {
            vortex.enabled = true;

        }
        else
        {
            vortex.enabled = false;
            inVortex.Clear();
        }
    }

    void ScaleControl()
    {
        if(inBag.Count < 1)
        {
           transform.localScale = bagEmptyScale;
        }
        else if (inBag.Count > bagMaxCapacity / 2 && inBag.Count < bagMaxCapacity - 1)
        {
            transform.localScale = bagEmptyScale * 1.5f;
        }
        else if (inBag.Count > bagMaxCapacity - 1)
        {
            transform.localScale = bagEmptyScale * 2;
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

        objectRB.isKinematic = false;
        objectRB.transform.parent = null;
        objectRB.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1.5f);
        objectRB.gameObject.SetActive(true);

        objectRB.AddForce(transform.forward * shootForce, ForceMode.Impulse);

        if (inBag.Contains(objectRB))
        {
            inBag.Remove(objectRB);
        }
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
