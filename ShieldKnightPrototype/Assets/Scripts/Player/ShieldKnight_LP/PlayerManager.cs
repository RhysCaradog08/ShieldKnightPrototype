using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public StandardShieldController shield;
    public MushroomCapController mushroom;
    public ScrapBagController scrapBag;

    public bool hasShield, hasMushroom, hasScrap;

    // Start is called before the first frame update
    void Start()
    {
        shield = FindObjectOfType<StandardShieldController>();
        mushroom = FindObjectOfType<MushroomCapController>();
        scrapBag = FindObjectOfType<ScrapBagController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(shield.isActiveAndEnabled)
        {
            hasShield = true;
        }
        else hasShield = false;

        if (mushroom.isActiveAndEnabled)
        {
            hasMushroom = true;
        }
        else hasMushroom = false;

        if (scrapBag.isActiveAndEnabled)
        {
            hasScrap = true;
        }
        else hasScrap= false;
    }
}
