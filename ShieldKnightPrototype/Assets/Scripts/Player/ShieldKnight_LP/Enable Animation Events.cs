using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAnimationEvents : MonoBehaviour
{
    [SerializeField] StandardShieldController shield;
    [SerializeField] MushroomCapController mushroom;
    [SerializeField] ScrapBagController scrapBag;
    public GameObject parryBox;

    private void Awake()
    {
        shield = FindObjectOfType<StandardShieldController>();
        mushroom = FindObjectOfType<MushroomCapController>();
        scrapBag = FindObjectOfType<ScrapBagController>();
    }

    public void EnableThrowShield()  //Sets canThrow bool in ShieldController for animation event in Throw animation.
    {
        if (shield.isActiveAndEnabled)
        {
            shield.canThrow = true;
        }

        if(mushroom.isActiveAndEnabled) 
        {
            if (!mushroom.thrown)
            {
                mushroom.canThrow = true;
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
        scrapBag.ShootProjectile();
    }
}
