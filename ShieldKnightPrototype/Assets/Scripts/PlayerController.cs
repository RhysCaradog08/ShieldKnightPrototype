using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController cc;
    Animator anim;

    [Header("Movement")]
    Vector3 move;
    public float speed;

    [Header("Shield")]
    ShieldController shield;
    public float throwRange;
    float distance;
    public Transform[] shieldTargets;
    List<GameObject> targets = new List<GameObject>();
    GameObject closestTarget;
    public LayerMask targetMask;
    public int targetsLeft;

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        anim = GetComponent<Animator>();

        shield = GameObject.FindGameObjectWithTag("Shield").GetComponent<ShieldController>();
    }

    private void Update()
    {
        move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        cc.Move(move * Time.deltaTime * speed);
        transform.LookAt(move + transform.position);

        if(move.magnitude >= 0.05f)
        {
            anim.SetBool("Moving", true);
        }
        else
        {
            anim.SetBool("Moving", false);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if(!shield.thrown)
            {
                //anim.SetTrigger("Throw");
            }
        }

        //FindTargets();

        //float distance = Vector3.Distance(transform.position, shield.target.position);

        if (distance < throwRange)
        {
            FindTargets();
            //shield.hasTarget = true;
        }
        else shield.hasTarget = false;
    }

    void FindTargets()
    {
        /*shieldTargets = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];

        foreach (GameObject shieldTarget in shieldTargets)
        {
            if (shieldTarget.layer == 8)
            {
                Debug.Log(shieldTarget.name);

                shield.target = shieldTarget.transform;

                targetsLeft = shieldTargets.Length;
            }
        }*/

        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.forward, throwRange, targetMask);
        if(hits.Length > 0)
        {
            shield.hasTarget = true;


            /*shieldTargets = new Transform[hits.Length];

            for(int i = 0; i < hits.Length; i++)
            {
                shieldTargets[i] = hits[i].collider.gameObject.transform;
            }

            foreach(Transform shieldTarget in shieldTargets)
            {
                distance = Vector3.Distance(transform.position, shieldTarget.position);

                if (distance <= throwRange)
                {
                    shield.target = shieldTarget;
                    targetsLeft = shieldTargets.Length;

                    shield.hasTarget = true;
                }
                else if(distance > throwRange)
                {
                    targetsLeft = 0;

                    shield.hasTarget =false;
                }
            }*/
        }

        
    }
}
