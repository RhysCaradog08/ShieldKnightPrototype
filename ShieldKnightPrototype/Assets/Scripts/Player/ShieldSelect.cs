using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSelect : MonoBehaviour
{
    StandardShieldController shield;
    MushroomCapController mushroom;
    ScrapBagController scrapBag;

    public int selectedShield = 0;
    public bool canChange;

    [Header("Shield Booleans")]
    public bool hasShield, hasMushroomCap, hasScrapBag;

    // Start is called before the first frame update
    void Start()
    {
        shield = FindObjectOfType<StandardShieldController>();
        mushroom = FindObjectOfType<MushroomCapController>();
        scrapBag = FindObjectOfType<ScrapBagController>();

        SelectShield();
        canChange = true;     
    }

    // Update is called once per frame
    void Update()
    {
        if (shield.gameObject.activeInHierarchy)
        {
            hasShield = true;
        }
        else hasShield = false;

        if (mushroom.gameObject.activeInHierarchy)
        {
            hasMushroomCap = true;
        }
        else hasMushroomCap = false;

        if (scrapBag.gameObject.activeInHierarchy)
        {
            hasScrapBag = true;
        }
        else hasScrapBag = false;

        int previousWeapon = selectedShield;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (canChange)
            {
                if (selectedShield >= transform.childCount - 1)
                {
                    selectedShield = 0;
                }
                else
                {
                    selectedShield++;
                }
            }
        }

        if (previousWeapon != selectedShield)
        {
            SelectShield();
        }
    }

     public void SelectShield()
    {
        int i = 0;
        foreach (Transform shield in transform)
        {
            if (i == selectedShield)
                shield.gameObject.SetActive(true);
            else
                shield.gameObject.SetActive(false);
            i++;

        }
    }
}
