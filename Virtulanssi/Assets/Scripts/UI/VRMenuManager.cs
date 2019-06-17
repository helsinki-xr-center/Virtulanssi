using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMenuManager : MonoBehaviour
{
    public GameObject vrMainMenu;

    LogitechGSDK.DIJOYSTATE2ENGINES rec;

    // Activate main menu using steering wheel button 6. Also pauses/unpauses the game
    private void Update()
    {
        rec = LogitechGSDK.LogiGetStateUnity(0);

        //if (rec.rgbButtons[6] == 128)
        if (Input.GetKeyDown("joystick " + 1 + " button " + 6))
        {
            vrMainMenu.SetActive(!vrMainMenu.activeSelf);
        }
        if (vrMainMenu.activeSelf)
        {
            Time.timeScale = 0f;
        }
        else if (!vrMainMenu.activeSelf)
        {
            Time.timeScale = 1f;
        }
    }

}
