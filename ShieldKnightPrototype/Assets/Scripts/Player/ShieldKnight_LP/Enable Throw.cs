using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableThrow : MonoBehaviour
{
    [SerializeField] StandardShieldController shield;

    private void Awake()
    {
        shield = FindObjectOfType<StandardShieldController>();
    }

    public void EnableThrowShield()  //Sets canThrow bool in ShieldController for animation event in Throw animation.
    {
        shield.canThrow = true;
    }
}
