using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAnimationEvents : MonoBehaviour
{
    PlayerManager pm;
    public GameObject parryBox;

    private void Awake()
    {
        pm = FindObjectOfType<PlayerManager>();
    }

    public void EnableThrowShield()  //Sets canThrow bool in ShieldController for animation event in Throw animation.
    {
        if (pm.hasShield)
        {
            pm.shield.canThrow = true;
        }

        if(pm.hasMushroomCap) 
        {
            if (!pm.mushroomCap.thrown)
            {
                pm.mushroomCap.canThrow = true;
            }
        }
    }

    public void EnableParry()  ///Sets ParryBox.
    {
        parryBox.SetActive(true);
    }

    public void DisableParry()
    {
        parryBox.SetActive(false);
    }

    public void ExpelScrap()
    {
        pm.scrapBag.ShootProjectile();
    }
}
