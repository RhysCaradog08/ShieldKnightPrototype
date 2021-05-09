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
    public ShieldController shield;
    public float throwRange;
    GameObject[] shieldTargets;
    public List<Transform> targets = new List<Transform>();

    private void Start()
    {
        cc = GetComponent<CharacterController>();

        anim = GetComponent<Animator>();
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

        FindTargets();


        float distance = Vector3.Distance(transform.position, shield.target.transform.position);

        if (distance < throwRange)
        {
            shield.hasTarget = true;
        }
        else shield.hasTarget = false;
    }

    void FindTargets()
    {
        shieldTargets = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];

        foreach (GameObject shieldTarget in shieldTargets)
        {
            if (shieldTarget.layer == 8)
            {
                Debug.Log(shieldTarget.name);
                targets.Add(shieldTarget.transform);
                shield.target = shieldTarget.transform;
            }
        }
    }
}
