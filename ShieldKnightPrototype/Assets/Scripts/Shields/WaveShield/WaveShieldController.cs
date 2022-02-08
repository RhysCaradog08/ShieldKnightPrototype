using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShieldController : MonoBehaviour
{
    [SerializeField] int attackCount;
    [SerializeField] float attackDelay, attackReset;

    private void Start()
    {
        attackCount = 3;
    }

    private void Update()
    {
        if(Input.GetButtonDown("Throw") && attackDelay <= 0)
        {
            if(attackCount > 0)
            {
                attackCount--;
                attackDelay = 1f;
                attackReset = 2f;
            }
        }

        /*if(attackCount <= 0)
        {
            attackCount = 0;
        }*/

        if(attackDelay > 0)
        {
            attackDelay -= Time.deltaTime;
        }

        if(attackDelay <= 0)
        {
            attackDelay = 0;
        }

        if(attackReset > 0)
        {
            attackReset -= Time.deltaTime;
        }

        if(attackReset <= 0)
        {
            attackReset = 0;
            attackCount = 3;
        }
    }
}
