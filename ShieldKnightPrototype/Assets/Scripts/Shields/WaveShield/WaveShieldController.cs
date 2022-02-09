using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShieldController : MonoBehaviour
{
    PlayerController pc;

    [SerializeField] int attackCount;
    [SerializeField] float attackDelay, attackReset, surfSpeed;

    public bool surfing;

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        attackCount = 3;

        surfing = false;
    }

    private void Update()
    {
        if(Input.GetButtonDown("Throw") && attackDelay <= 0)
        {
            if(attackCount > 0)
            {
                attackCount--;
                attackDelay = 0.5f;
                attackReset = 0.75f;
            }

            if(attackCount == 2) //&& attackReset > 0)
            {
                Debug.Log("Attack Left");
                pc.anim.SetTrigger("WaveAttackLeft");
            }

            if(attackCount == 1) //&& attackReset > 0)
            {
                Debug.Log("Attack Right");
                pc.anim.SetTrigger("WaveAttackRight");
                pc.anim.ResetTrigger("WaveAttackLeft");
            }

            if (attackCount == 0) //&& attackReset > 0)
            {
                Debug.Log("Attack Overhead");
                pc.anim.ResetTrigger("WaveAttackRight");
                pc.anim.SetTrigger("WaveAttackOverhead");
            }
        }

        if (Input.GetButton("Barge") && attackDelay <= 0)
        {
            surfing = true;

            pc.anim.SetBool("Surfing", true);
            pc.speed = surfSpeed;
        }
        else
        {
            surfing = false;

            pc.anim.SetBool("Surfing", false);
            pc.speed = pc.moveSpeed;
        }

        if (attackDelay > 0)
        {
            attackDelay -= Time.deltaTime;
            pc.enabled = false;
        }

        if(attackDelay <= 0)
        {
            attackDelay = 0;
            pc.enabled = true;
        }

        if(attackReset > 0)
        {
            attackReset -= Time.deltaTime;
            pc.anim.SetBool("WaveAttacking", true);
        }

        if(attackReset <= 0)
        {
            attackReset = 0;
            attackCount = 3;

            pc.anim.SetBool("WaveAttacking", false);
            pc.anim.ResetTrigger("WaveAttackLeft");
            pc.anim.ResetTrigger("WaveAttackRight");
            pc.anim.ResetTrigger("WaveAttackOverhead");
        }
    }
}
