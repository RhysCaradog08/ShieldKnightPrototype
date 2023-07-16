using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;
using UnityEngine.UI;
using TMPro;

public class ScrapBagController : MonoBehaviour
{
    ShieldSelect select;
    AnimationController animControl;
    ShieldKnightController sk;
    ScrapBagAnimationController scrapBagAnim;
    TargetSelector ts;

    [Header("Vortex")]
    public float suctionSpeed, suctionRange;
    [SerializeField] List <Rigidbody> inVortex = new List <Rigidbody>();
    [SerializeField] List <Rigidbody> inBag = new List <Rigidbody>();
    [SerializeField] int bagMaxCapacity;
    CapsuleCollider vortex;
    Rigidbody objectRB;
    [SerializeField] GameObject vortexFX;
    public bool isAiming, enableVortex;

    [Header("Shoot Scrap")]
    public float shootForce, shotFrequency;
    [SerializeField] private float repeatShotDelay = 0;
    public Transform shootPoint;
    GameObject smokeBurst;
    public TextMeshProUGUI scrapCounter;
    public bool expellingScrap;

    [Header("Rolling")]
    public Transform holdParent;
    public GameObject model;
    [SerializeField] Vector3 holdPos = new Vector3(0, 0.05f, -1), rollPos, skRollPos;
    Quaternion holdRot = Quaternion.Euler(-90, 90, 0), rollRot = Quaternion.Euler(0, -90, 0);
    [SerializeField] SphereCollider rollCollider;
    public float rollSpeed;
    public bool isRolling;

    [Header("Parachute")]
    public float descentSpeed;
    Vector3 parachutePos = new Vector3(0, -0.25f, -2);
    Quaternion parachuteRot = Quaternion.Euler(12, 0, 0);
    public bool parachuteOpen;

    [Header("Bag Swipe")]
    [SerializeField] BoxCollider swipeTrigger;
    public bool swipingBag;

    [Header("Scale")]
    Vector3 bagEmptyScale = Vector3.one;

    [Header("Button Held Check")]
    const float minButtonHold = 0.1f;
    float buttonHeldTime;
    public bool buttonHeld;

    private void Awake()
    {
        select = FindObjectOfType<ShieldSelect>();
        animControl= FindObjectOfType<AnimationController>();
        sk = FindObjectOfType<ShieldKnightController>();
        scrapBagAnim = FindObjectOfType<ScrapBagAnimationController>();
        ts = FindObjectOfType<TargetSelector>();

        vortex = GetComponent<CapsuleCollider>();
        rollCollider = GetComponent<SphereCollider>();
        swipeTrigger = GetComponent<BoxCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Vortex
        vortex.height = suctionRange;
        vortex.center = new Vector3(0, 0, suctionRange/2 + 1);
        vortex.radius = 3;
        vortexFX.SetActive(false);
        enableVortex = false;
        isAiming = false;

        //Rolling
        transform.parent = holdParent; 
        transform.localPosition = holdPos;
        transform.localRotation = holdRot;
        isRolling = false;

        //Shooting Scrap
        expellingScrap = false;

        //Parachute
        parachuteOpen = false;

        //Swiping Bag
        swipingBag = false;
        swipeTrigger.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        ScaleControl();

        scrapCounter.text = inBag.Count.ToString();

        if (!isRolling)
        {
            ButtonHeldCheck();

            if (buttonHeld)
            {
                swipingBag = false;

                if (inBag.Count > 0 && !enableVortex)
                {
                    isAiming = true;
                    expellingScrap = true;

                    /*if (Time.time >= repeatShotDelay)
                    {
                        Debug.Log("Shoot");                       
                        //ShootProjectile();
                        repeatShotDelay = Time.time + 1 / shotFrequency;
                    }
                    else expellingScrap = false;*/
                }
                else if (inBag.Count < 1)
                {
                    new WaitForSeconds(1);

                    isAiming = false;
                    expellingScrap = false;
                }
            }

            if (sk.cc.isGrounded)
            {
                if (Input.GetButtonDown("Throw") && inBag.Count < 1)
                {
                    swipeTrigger.enabled = true;
                    swipingBag = true;
                }

                if (Input.GetButton("Guard"))
                {
                    if (inBag.Count < bagMaxCapacity && !expellingScrap)
                    {
                        isAiming = true;
                        enableVortex = true;
                    }
                }
                else enableVortex = false;
            }
        }

        if (Input.GetButtonDown("Barge") && !isAiming)
        {
            isRolling = !isRolling;
        }

        if (!sk.cc.isGrounded && Input.GetButton("Guard"))
        {
            if (inBag.Count < 1)
            {
                parachuteOpen = true;
            }           
        }
        else parachuteOpen = false;

        if (!sk.cc.isGrounded && Input.GetButtonDown("Guard"))
        {
            if(inBag.Count > 0)
            {
                sk.isSlamming = true;
            }
        }

        if (isAiming)
        {
            animControl.ChangeAnimationState(animControl.scrapBagAim);

            if (Input.GetButtonUp("Throw") || Input.GetButtonUp("Guard"))
            {
                isAiming = false;
            }
        }

        if(expellingScrap)
        {
            scrapBagAnim.ChangeAnimationState(scrapBagAnim.expel);
        }
        
        if (inBag.Count >= bagMaxCapacity)
        {
            enableVortex = false;
        }

        if (enableVortex)
        {
            vortex.enabled = true;
            vortexFX.SetActive(true);
            scrapBagAnim.ChangeAnimationState(scrapBagAnim.suck);
        }
        else
        {
            vortex.enabled = false;
            vortexFX.SetActive(false);
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

            sk.transform.position = sk.transform.position;
        }

        if (parachuteOpen)
        {
            Parachute();
        }

        if (swipingBag)
        {
            transform.rotation = Quaternion.Euler(0, -90, -180);

            if (inBag.Count > 0)
            {
                swipeTrigger.enabled = false;
            }
        }

        if (!expellingScrap && !enableVortex && !parachuteOpen && !swipingBag)
        {
            scrapBagAnim.ChangeAnimationState(scrapBagAnim.idle);

            if (!isRolling)
            {
                transform.localPosition = holdPos;
                transform.localRotation = holdRot;
            }
        }

        if (isAiming || isRolling || parachuteOpen || swipingBag)
        {
            select.canChange = false;
        }
        else select.canChange = true;
    }

    void ScaleControl()
    {
        if(inBag.Count < 1)
        {
           transform.localScale = bagEmptyScale;
           rollPos = new Vector3(0, -3, 0);
           skRollPos = new Vector3(sk.transform.position.x, 4.5f, sk.transform.position.z);
        }
        else if (inBag.Count > bagMaxCapacity / 2 && inBag.Count < bagMaxCapacity - 1)
        {
            transform.localScale = bagEmptyScale * 1.5f;
            rollPos = new Vector3(0, -3.5f, 0);
            skRollPos = new Vector3(sk.transform.position.x, 5.5f, sk.transform.position.z);
        }
        else if (inBag.Count > bagMaxCapacity - 1)
        {
            transform.localScale = bagEmptyScale * 2;
            rollPos = rollPos = new Vector3(0, -4, 0);
            skRollPos = new Vector3(sk.transform.position.x, 6.5f, sk.transform.position.z);
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

    public void ShootProjectile()
    {
        objectRB = inBag[0].GetComponent<Rigidbody>();

        objectRB.transform.position = shootPoint.position;
        objectRB.isKinematic = false;
        objectRB.transform.parent = null;
        objectRB.transform.localScale = Vector3.one;
        objectRB.gameObject.SetActive(true);


        Vector3 smokeDirection = transform.position - objectRB.transform.position;
        smokeBurst = ObjectPoolManager.instance.CallObject("SmokeBurst", objectRB.transform, objectRB.transform.position, Quaternion.LookRotation(smokeDirection, objectRB.transform.up), 1);

        objectRB.AddForce(transform.forward * shootForce, ForceMode.Impulse);

        if (inBag.Contains(objectRB))
        {
            inBag.Remove(objectRB);
        }

        if(!inBag.Contains(objectRB))
        {
            objectRB = null;
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

    void Parachute()
    {
        scrapBagAnim.ChangeAnimationState(scrapBagAnim.parachute);

        transform.localPosition = parachutePos;
        transform.localRotation = parachuteRot;
    }

    void BagSwipe()
    {
        swipeTrigger.enabled = true;

        transform.rotation = Quaternion.Euler(0, -90, -180);
    }

    void ButtonHeldCheck()
    {
        if (Input.GetButtonDown("Throw"))//Button is pressed down. Need to check to see if it is "held".
        {
            if (sk.cc.isGrounded && sk.stopTime <= 0)
            {
                buttonHeldTime = Time.timeSinceLevelLoad;
                buttonHeld = false;
            }
        }
        else if (Input.GetButtonUp("Throw") && sk.cc.isGrounded)
        {
            if (!buttonHeld)//If button is released without being held.
            {
                sk.stopTime = 0.5f;
            }
            buttonHeld = false;
        }

        if (Input.GetButton("Throw") && sk.cc.isGrounded)
        {
            if (Time.timeSinceLevelLoad - buttonHeldTime > minButtonHold)//Button is considered "held" if it is actually held down.
            {
                buttonHeld = true;
            }
        }
    }

    public void BagSlam()
    {
            //Have any rigibodies in bag disperse in random directions.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            if (swipeTrigger.enabled)
            {
                if(inBag.Contains(other.GetComponent<Rigidbody>()) && inBag.Count < 1)
                {
                    inBag.Add(other.GetComponent<Rigidbody>());
                }
            }
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
