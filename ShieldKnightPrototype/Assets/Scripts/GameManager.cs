using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject hud, PauseMenu;
    [SerializeField] bool isPaused;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        Application.targetFrameRate = 60;

        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            isPaused = !isPaused;
        }

        if (isPaused)
        {
            Time.timeScale = Mathf.Epsilon;

            PauseMenu.SetActive(true);
            hud.SetActive(false);
        }
        else 
        {
            Time.timeScale = 1;

            PauseMenu.SetActive(false);
            hud.SetActive(true);
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

    private void OnApplicationQuit()
    {
        Application.Quit();
    }
}
