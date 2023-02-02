using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GauntletShieldController : MonoBehaviour
{
    [SerializeField] float attackTime;
    public int attackInt;
    public bool isAttacking;

    // Start is called before the first frame update
    void Start()
    {
        attackTime = 0;
        attackInt = 0;
        isAttacking = false;
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

        if (Input.GetButtonDown("Throw"))
        {
            if (!isAttacking && attackTime <= 0)
            {
                attackTime = 1;
                isAttacking = true;
            }

            if(isAttacking && attackInt <= 0.5f)
            {
                attackTime = 1;
            }

            attackInt++;

            if(attackInt > 2)

            {
                attackInt = 1;
            }
        }

        if(Input.GetButtonDown("Barge"))
        {
            if(isAttacking)
            {
                isAttacking = false;
            }

            attackTime = 0;
        }
    }
}
