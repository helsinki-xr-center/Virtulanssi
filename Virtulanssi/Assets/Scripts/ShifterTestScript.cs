using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShifterTestScript : MonoBehaviour
{
    LogitechGSDK.LogiControllerPropertiesData properties;
    public float xAxes, GasInput, BreakInput, ClutchInput;
    public int currentGear;

    public bool HShift = true;

    private void Start()
    {
        print(LogitechGSDK.LogiSteeringInitialize(false));
    }
    // Update is called once per frame
    void Update()
    {

        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            LogitechGSDK.DIJOYSTATE2ENGINES rec;
            rec = LogitechGSDK.LogiGetStateUnity(0);
            HShifter(rec);
            xAxes = rec.lX / 32768f;

            if (rec.lY > 0)
            {
                GasInput = 0;
            }
            else if (rec.lY < 0)
            {
                GasInput = rec.lY / -32768f;
            }

            if (rec.lRz > 0)
            {
                BreakInput = 0;
            }
            else if (rec.lRz < 0)
            {
                BreakInput = rec.lRz / -32768f;
            }

            if (rec.rglSlider[1] > 0)
            {
                ClutchInput = 0;
            }
            else if (rec.rglSlider[1] < 0)
            {
                ClutchInput = rec.rglSlider[1] / -32768f;
            }
        }
        else
        {
            print("No wheel");
        }
    }

    void HShifter(LogitechGSDK.DIJOYSTATE2ENGINES shifter)
    {

        for (int i = 0; i < 128; i++)
        {

            if (shifter.rgbButtons[i] == 128)
            {
                Debug.Log(shifter.rgbButtons[i].ToString());
                Debug.Log(i);
                if (ClutchInput > 0.5f)
                {
                    if (i == 10)
                    {
                        currentGear = 1;
                    }
                    else if (i == 11)
                    {
                        currentGear = 2;
                    }
                }

                //        if (ClutchInput > 0.5f)
                //        {
                //            
                //            
                //            else if (i == 14)
                //            {
                //                currentGear = 3;
                //            }
                //            else if (i == 15)
                //            {
                //                currentGear = 4;
                //            }
                //            else if (i == 16)
                //            {
                //                currentGear = 5;
                //            }
                //            else if (i == 17)
                //            {
                //                currentGear = 6;
                //            }
                //            else if (i == 18)
                //            {
                //                currentGear = -1;
                //            }
                //        }
            }
        }
    }
}
