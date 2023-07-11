using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldIconsController : MonoBehaviour
{
    PlayerManager pm;

    [SerializeField] GameObject shieldIcon, mushroomIcon, scrapIcon;

    void Start()
    {
        pm = FindObjectOfType<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pm.hasShield)
        {
            shieldIcon.SetActive(true);
            mushroomIcon.SetActive(false);
            scrapIcon.SetActive(false);
        }

        if (pm.hasMushroomCap)
        {
            shieldIcon.SetActive(false);
            mushroomIcon.SetActive(true);
            scrapIcon.SetActive(false);
        }

        if (pm.hasScrapBag)
        {
            shieldIcon.SetActive(false);
            mushroomIcon.SetActive(false);
            scrapIcon.SetActive(true);
        }
    }
}
