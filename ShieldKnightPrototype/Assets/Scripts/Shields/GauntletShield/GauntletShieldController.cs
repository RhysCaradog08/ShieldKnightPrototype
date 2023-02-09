using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GauntletShieldController : MonoBehaviour
{
    [SerializeField] PlayerController pc;
    [SerializeField] CharacterController cc;
    [SerializeField] ShieldSelect select;

    [Header("Attacking")]
    [SerializeField] float attackTime;
    public int attackInt;
    public bool isAttacking;

    [Header("Dodge")]
    public float dodgeTime, dodgeSpeed;
    [SerializeField] float dodgeDelay;
    public bool canDodge, isDodging;

    [Header("Uppercut")]
    public float uppercutHeight, uppercutForce, uppercutDelay;
    public bool canUppercut; 

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
        cc = FindObjectOfType<CharacterController>();
        select = FindObjectOfType<ShieldSelect>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Attack
        attackTime = 0;
        attackInt = 0;
        isAttacking = false;

        //Dodge
        isDodging = false;

        //Uppercut
        canUppercut = true;
        pc.isUppercutting = false;
        uppercutDelay = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(attackTime > 0)
        {
            attackTime -= Time.deltaTime;
        }
        else if(attackTime <= 0)
        {
            attackTime = 0;
            attackInt = 0;
            isAttacking = false;
        }

        if(dodgeDelay > 0)
        {
            dodgeDelay -= Time.deltaTime;
        }
        else if (dodgeDelay <= 0)
        {
            dodgeDelay = 0;
        }

        if(uppercutDelay > 0)
        {
            uppercutDelay -= Time.deltaTime;
        }
        else if(uppercutDelay <= 0)
        {
            uppercutDelay = 0;
            canUppercut = true;
        }

        if(isAttacking || isDodging || pc.isUppercutting)
        {
            if (select.canChange)
            {
                select.canChange = false;
            }
        }
        else
        {
            if (!select.canChange)
            {
                select.canChange = true;
            }
        }

        if(pc.isUppercutting)
        {
            Debug.Log("Is Uppercutting");
        }

        if (Input.GetButtonDown("Throw"))
        {
            if (!isAttacking && attackTime <= 0)
            {
                attackTime = 1;
                isAttacking = true;
            }

            if(isAttacking && attackTime <= 0.5f)
            {
                attackTime = 1;
            }

            attackInt++;

            if(attackInt > 2)

            {
                attackInt = 1;
            }
        }

        if(Input.GetButtonDown("Guard"))
        {
            if(isAttacking)
            {
                isAttacking = false;
            }

            attackTime = 0;

            if(pc.velocity.y > 0)
            {
                Debug.Log("Stop Jumping!");
                pc.jumpSpeed = 0;
                pc.velocity.y = 0;
            }

            if(canUppercut && uppercutDelay <= 0)
            {
                canUppercut = false;
                pc.isUppercutting = true;
            }
        }

        if(Input.GetButtonDown("Barge"))
        {
            if (isAttacking)
            {
                isAttacking = false;
            }

            attackTime = 0;

            if (canDodge && dodgeDelay <= 0)
            {
                StartCoroutine(Dodge());
            }
        }

        if (isDodging)
        {
            pc.dodging = true;
        }
        else pc.dodging = false;

        canDodge = true;
        isDodging = false;
    }

    IEnumerator Dodge()
    {
        float startTime = Time.time;

        new WaitForSeconds(1); //Prevents player from stacking Dodges.

        while (Time.time < startTime + dodgeTime)  //Player movement speed is disabled then moved by dodgeSpeed over dodgeTime;
        {
            isDodging = true;
            canDodge = false;
            pc.speed = 0;

            cc.Move(pc.moveDir * dodgeSpeed * Time.deltaTime);

            dodgeDelay = 0.5f;

            yield return null;
        }
    }
}
