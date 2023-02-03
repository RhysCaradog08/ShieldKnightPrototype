using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GauntletShieldController : MonoBehaviour
{
    [SerializeField] PlayerController pc;
    [SerializeField]CharacterController cc;

    [Header("Attacking")]
    [SerializeField] float attackTime;
    public int attackInt;
    public bool isAttacking;

    [Header("Dodge")]
    public bool canDodge, isDodging;
    public float dodgeTime, dodgeSpeed;
    [SerializeField] float dodgeDelay;

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
        cc = FindObjectOfType<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        attackTime = 0;
        attackInt = 0;
        isAttacking = false;
        isDodging = false;
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

        if (dodgeDelay <= 0)
        {
            dodgeDelay = 0;
        }

        dodgeDelay -= Time.deltaTime;

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
