using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheelInput : MonoBehaviour
{

    public float wheelInput;
    public float gasInput;
    public float breakInput;
    private float axesMax = 32767f; // the max value for axes rotation and position (pedals and steering wheel);

    public float accelerationResponse; // How fast the car starts to accelerate as you push the acceleration pedal (-1 to 1)
    public float breakResponse;  // How fast the car starts to decelerate as you push the break pedal (-1 to 1)

    //public int currentGear;
    //public float clutchInput;

    // Start is called before the first frame update
    void Start()
    {

    }

    //Converts input values from the steering wheel and pedals to a more readable -1 to 1 scale
    void FixedUpdate()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)) // Checks that the steering wheel is connected and that the applictaion's main window is active
        {
            LogitechGSDK.DIJOYSTATE2ENGINES rec;
            rec = LogitechGSDK.LogiGetStateUnity(0); // Stores info about the device's positional information for axes, POVs and buttons.

            wheelInput = rec.lX / axesMax; // Normalize x-axis position value (-1 to 1) 

            if (rec.lY > (accelerationResponse * axesMax))
            {
                gasInput = 0;
            }
            else if (rec.lY < (accelerationResponse * axesMax))
            {
                gasInput = rec.lY / - axesMax; // Normalize y-axis position value (-1 to 1) 
            }
            if (rec.lRz > (breakResponse * axesMax))
            {
                breakInput = 0;
            }
            else if (rec.lRz < (breakResponse * axesMax))
            {
                breakInput = rec.lRz / - axesMax; // Normalize z-axis position value (-1 to 1) 
            }
        }
        else
        {
            Debug.Log("No steering wheel connected.");
        }
    }
}
