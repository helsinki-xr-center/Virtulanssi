using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJoystickInput : MonoBehaviour
{

    // Use this for initialization
    void Update()
    {
        for (int joystick = 1; joystick < 17; joystick++)
        {
            for (int button = 0; button < 20; button++)
            {
                if (Input.GetKey("joystick " + joystick + " button " + button))
                {

                    Debug.Log("joystick = " + joystick + "  button = " + button);
                }
            }
        }

    }


}