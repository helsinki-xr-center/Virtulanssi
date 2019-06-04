using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRMenuManager : MonoBehaviour
{
    public GameObject vrMainMenu;

    LogitechGSDK.DIJOYSTATE2ENGINES rec;

    // Activate main menu using escape. Also pauses/unpauses the game
    private void Update()
    {
        rec = LogitechGSDK.LogiGetStateUnity(0);

        if (rec.rgbButtons[6] == 128)
        {
            vrMainMenu.SetActive(!vrMainMenu.activeSelf);
        }
        if (vrMainMenu.activeSelf)
        {
            Time.timeScale = 0f;
        }
        if (!vrMainMenu.activeSelf)
        {
            Time.timeScale = 1f;
        }
    }

}
