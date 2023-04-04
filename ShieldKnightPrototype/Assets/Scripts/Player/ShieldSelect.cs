using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSelect : MonoBehaviour
{
    StandardShieldController shield;
    MushroomCapController mushroom;

    public int selectedShield = 0;
    public bool canChange;

    [Header("Shield Booleans")]
    public bool hasShield, hasMushroom;

    // Start is called before the first frame update
    void Start()
    {
        shield = FindObjectOfType<StandardShieldController>();
        mushroom = FindObjectOfType<MushroomCapController>();

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
            hasMushroom = true;
        }
        else hasMushroom = false;

        int previousWeapon = selectedShield;
        //int previousUI = SelectedWeaponUI;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (canChange)
            {
                if (selectedShield >= transform.childCount - 1)
                {
                    selectedShield = 0;
                    //SelectedWeaponUI = 0;
                }
                else
                {
                    selectedShield++;
                    //SelectedWeaponUI++;
                }
            }
        }

        if (previousWeapon != selectedShield)
        {
            SelectShield();
        }

        /*if(previousUI != SelectedWeaponUI)
        {
            SelectWeaponUI();
        }*/
    }

     public void SelectShield()
    {
        int i = 0;
        foreach (Transform shield in transform)
        {
            Debug.Log(shield.name);

            if (i == selectedShield)
                shield.gameObject.SetActive(true);
            else
                shield.gameObject.SetActive(false);
            i++;

        }
    }

    /*public void SelectWeaponUI()
    {
        int i = 0;
        foreach (GameObject weaponicon in weaponIcon)
        {
            if(i == SelectedWeaponUI)
                weaponIcon[i].SetActive(true);
            else
                weaponIcon[i].SetActive(false);
            i++;
        }
    }*/
}
