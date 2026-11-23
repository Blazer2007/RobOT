using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           pausePanel.SetActive(pausePanel.activeSelf ? false : true);
           Time.timeScale = pausePanel.activeSelf ? 0f : 1f;
        }

        if(pausePanel.activeSelf)
        {
           Cursor.lockState = CursorLockMode.None;
           Cursor.visible = true;
        }
        else
        {
           Cursor.lockState = CursorLockMode.Locked;
           Cursor.visible = false;
        }
    }

    public void Continue()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitGame() { 
        Application.Quit();
    }


}
