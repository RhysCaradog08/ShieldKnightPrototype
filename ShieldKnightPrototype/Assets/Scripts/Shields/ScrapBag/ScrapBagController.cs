using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapBagController : MonoBehaviour
{
    ShieldSelect select;
    AnimationController animControl;
    ShieldKnightController sk;

    [Header("Vortex")]
    public float suctionSpeed, suctionRange;
    [SerializeField] List <Rigidbody> inVortex = new List <Rigidbody>();
    [SerializeField] List <Rigidbody> inBag = new List <Rigidbody>();
    [SerializeField] int bagMaxCapacity;
    CapsuleCollider vortex;
    Rigidbody objectRB;

    [Header("Shoot Projectile")]
    public float shootForce, shotFrequency;
    [SerializeField] private float repeatShotDelay = 0;
    public Transform shootPoint;

    [Header("Rolling")]
    public Transform holdParent;
    public GameObject model;
    Vector3 holdPos = new Vector3(0, 0.05f, -1), rollPos, skRollPos;
    Quaternion holdRot = Quaternion.Euler(-90, 90, 0), rollRot = Quaternion.Euler(0, -90, 0);
    [SerializeField] SphereCollider rollCollider;
    public float rollSpeed;

    [Header("Scale")]
    Vector3 bagEmptyScale = Vector3.one;

    public bool isAiming, enableVortex, isRolling;

    private void Awake()
    {
        select = FindObjectOfType<ShieldSelect>();
        animControl= FindObjectOfType<AnimationController>();
        sk = FindObjectOfType<ShieldKnightController>(); 

        vortex = GetComponent<CapsuleCollider>();
        rollCollider = GetComponent<SphereCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //SuckVortex
        vortex.height = suctionRange;
        vortex.center = new Vector3(0, 0, suctionRange/2 + 1);
        vortex.radius = 3;

        //Rolling
        transform.parent = holdParent; 
        transform.localPosition = holdPos;
        transform.localRotation = holdRot;

        isAiming = false;
        enableVortex = false;
        isRolling = false;
    }

    // Update is called once per frame
    void Update()
    {
        ScaleControl();

        if (!isRolling)
        {
            if (Input.GetButtonDown("Throw") && inBag.Count > 0)
            {
                repeatShotDelay = Time.time + 0.25f / shotFrequency;
            }

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
                else if (inBag.Count < 1)
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
        }

        if (Input.GetButtonDown("Barge") && !isAiming)
        {
            isRolling = !isRolling;
        }

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

        if (isRolling)
        {
            rollCollider.enabled = true;
            Rolling();
        }
        else
        {
            rollCollider.enabled = false;

            transform.parent = holdParent;
            transform.localPosition = holdPos;
            transform.localRotation = holdRot;

            sk.transform.position = sk.transform.position;
        }
    }

    void ScaleControl()
    {
        if(inBag.Count < 1)
        {
           transform.localScale = bagEmptyScale;
           rollPos = new Vector3(0, -3, 0);
           skRollPos = new Vector3(transform.position.x, 4.5f, transform.position.z);
        }
        else if (inBag.Count > bagMaxCapacity / 2 && inBag.Count < bagMaxCapacity - 1)
        {
            transform.localScale = bagEmptyScale * 1.5f;
            rollPos = new Vector3(0, -3.5f, 0);
            skRollPos = new Vector3(transform.position.x, 5.5f, transform.position.z);
        }
        else if (inBag.Count > bagMaxCapacity - 1)
        {
            transform.localScale = bagEmptyScale * 2;
            rollPos = rollPos = new Vector3(0, -4, 0);
            skRollPos = new Vector3(transform.position.x, 6.5f, transform.position.z);
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

        objectRB.transform.position = shootPoint.position;
        objectRB.isKinematic = false;
        objectRB.transform.parent = null;
        objectRB.transform.localScale = Vector3.one;
        objectRB.gameObject.SetActive(true);

        objectRB.AddForce(transform.forward * shootForce, ForceMode.Impulse);

        if (inBag.Contains(objectRB))
        {
            inBag.Remove(objectRB);
        }
    }

    void Rolling()
    {
        transform.parent = transform.root;
        transform.localPosition = rollPos;
        transform.localRotation = rollRot; 

        sk.transform.position = skRollPos;

        if (sk.move.magnitude > 0)
        {
            model.transform.Rotate(Vector3.up, rollSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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
        if (other.GetComponent<Rigidbody>() != null)
        {
            if (inVortex.Contains(other.GetComponent<Rigidbody>()))
            {
                inVortex.Remove(other.GetComponent<Rigidbody>());
            }
        }
    }
}
