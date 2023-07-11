using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public StandardShieldController shield;
    public MushroomCapController mushroomCap;
    public ScrapBagController scrapBag;

    public bool hasShield, hasMushroomCap, hasScrapBag;

    // Start is called before the first frame update
    void Start()
    {
        /*shield = FindObjectOfType<StandardShieldController>();
        mushroom = FindObjectOfType<MushroomCapController>();
        scrapBag = FindObjectOfType<ScrapBagController>();*/
    }

    // Update is called once per frame
    void Update()
    {
       ShieldCheck();
    }

    void ShieldCheck()
    {
        if (shield.isActiveAndEnabled)
        {
            hasShield = true;
        }
        else hasShield = false;

        if (mushroomCap.isActiveAndEnabled)
        {
            hasMushroomCap = true;
        }
        else hasMushroomCap = false;

        if (scrapBag.isActiveAndEnabled)
        {
            hasScrapBag = true;
        }
        else hasScrapBag = false;
    }
}
