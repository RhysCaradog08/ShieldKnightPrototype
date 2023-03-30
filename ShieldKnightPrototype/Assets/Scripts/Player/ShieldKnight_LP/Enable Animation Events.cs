using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAnimationEvents : MonoBehaviour
{
    [SerializeField] StandardShieldController shield;
    public GameObject parryBox;

    private void Awake()
    {
        shield = FindObjectOfType<StandardShieldController>();
    }

    public void EnableThrowShield()  //Sets canThrow bool in ShieldController for animation event in Throw animation.
    {
        shield.canThrow = true;
    }

    public void EnableParry()  ///Sets ParryBox.
    {
        parryBox.SetActive(true);
    }

    public void DisableParry()
    {
        parryBox.SetActive(false);
    }
}
