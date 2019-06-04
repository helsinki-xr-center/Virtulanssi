using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;

    // Activate main menu using escape. Also pauses/unpauses the game
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mainMenu.SetActive(!mainMenu.activeSelf);
        }
        if (mainMenu.activeSelf)
        {
            Time.timeScale = 0f;
        }
        if (!mainMenu.activeSelf)
        {
            Time.timeScale = 1f;
        }
    }

}
