using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldIconsController : MonoBehaviour
{
    ShieldSelect select;

    [SerializeField] GameObject shieldIcon, mushroomIcon, scrapIcon;

    void Start()
    {
        select = FindObjectOfType<ShieldSelect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (select.hasShield)
        {
            shieldIcon.SetActive(true);
            mushroomIcon.SetActive(false);
            scrapIcon.SetActive(false);
        }

        if (select.hasMushroomCap)
        {
            shieldIcon.SetActive(false);
            mushroomIcon.SetActive(true);
            scrapIcon.SetActive(false);
        }

        if (select.hasScrapBag)
        {
            shieldIcon.SetActive(false);
            mushroomIcon.SetActive(false);
            scrapIcon.SetActive(true);
        }
    }
}
