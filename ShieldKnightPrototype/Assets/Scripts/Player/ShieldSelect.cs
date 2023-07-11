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
