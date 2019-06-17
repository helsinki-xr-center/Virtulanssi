using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheelRotation : MonoBehaviour
{
    private float normalize = 364.09f; // 32767 divided by 90
    private float steeringWheelInput;

    void Update()
    {
        LogitechGSDK.DIJOYSTATE2ENGINES rec;
        rec = LogitechGSDK.LogiGetStateUnity(0); // Stores info about the device's positional information for axes, POVs and buttons.
        steeringWheelInput = rec.lX / normalize;
        transform.eulerAngles = new Vector3(0, 0, -steeringWheelInput);
    }
}
