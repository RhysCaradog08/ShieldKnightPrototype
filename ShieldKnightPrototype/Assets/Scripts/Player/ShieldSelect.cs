using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSelect : MonoBehaviour
{
    public int selectedShield = 0;
    public bool canChange;

    // Start is called before the first frame update
    void Start()
    {
        SelectShield();
        canChange = true;     
    }

    // Update is called once per frame
    void Update()
    {
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
        foreach (Transform weapon in transform)
        {
            if (i == selectedShield)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
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
