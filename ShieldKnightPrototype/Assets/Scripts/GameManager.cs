using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Text debugText;
    CoilShieldController coil;

    private void Awake()
    {
        coil = FindObjectOfType<CoilShieldController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        if(coil.isActiveAndEnabled)
        {
            debugText.text = "Has Coil";
        }
        else debugText.text = "";

        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if(Input.GetKeyDown(KeyCode.CapsLock))
        {
            if (Cursor.visible == false)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if(Cursor.visible == true)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }
}
