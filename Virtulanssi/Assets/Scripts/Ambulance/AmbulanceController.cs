using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbulanceController : MonoBehaviour
{
    public WheelCollider WheelColliderFL, WheelColliderFR, WheelColliderBL, WheelColliderBR;

    public GameObject FL, FR, BL, BR;

    public float steeringWheelInput, gasInput, brakeInput;
    private float axesMax = 32767f; // the max value for axes rotation and position (pedals and steering wheel);

    [Range(0f, 1.0f)]
    public float gasResponse = .8f; // How fast the car starts to accelerate as you push the acceleration pedal 
    [Range(0f, 1.0f)]
    public float brakeResponse = 1f;  // How fast the car starts to decelerate as you push the brake pedal 
    [Range(0f, 1.0f)]
    public float wheelResponse = .5f; // How responsive the wheel is

    public float topSpeed = 200f;
    public float maxTorque = 400f;
    public float maxSteerAngle = 45f;
    public float currentSpeed;
    public float maxBrakeTorque = 2200;

    //private float reverse, forward, turn, brake;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        WheelColliderFL.steerAngle = maxSteerAngle * steeringWheelInput;
        WheelColliderFR.steerAngle = maxSteerAngle * steeringWheelInput;

        currentSpeed = 2 * 22 / 7 * WheelColliderBL.radius * WheelColliderBL.rpm * 60 / 1000; //formula for calculating speed in kmph

        LogitechGSDK.DIJOYSTATE2ENGINES rec;
        rec = LogitechGSDK.LogiGetStateUnity(0); // Stores info about the device's positional information for axes, POVs and buttons.

        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)) // Checks that the steering wheel is connected and that the applictaion's main window is active
        {
            steeringWheelInput = rec.lX / axesMax * wheelResponse; // Normalize x-axis position value (-1 to 1) 

            if (rec.lY >= axesMax)
            {
                gasInput = 0;
            }
            else if (rec.lY < axesMax)
            {
                gasInput = rec.lY / -axesMax * gasResponse; // Normalize y-axis position value (-1 to 1)
            }
            if (rec.lRz >= axesMax) // due to normalization not being quite accurate, axesmax-1 ensures that the breakInput gets zeroed
            {
                brakeInput = 0;
            }
            else if (rec.lRz < axesMax)
            {
                brakeInput = rec.lRz / -axesMax * brakeResponse; // Normalize z-axis position value (-1 to 1) 
            }
        }

        if (currentSpeed < topSpeed)
        {
            if (rec.rgbButtons[8] == 128 || rec.rgbButtons[10] == 128 || rec.rgbButtons[12] == 128)
            {
                WheelColliderBL.motorTorque = maxTorque * gasInput; //runs all four wheels
                WheelColliderBR.motorTorque = maxTorque * gasInput;
                WheelColliderFL.motorTorque = maxTorque * gasInput;
                WheelColliderFR.motorTorque = maxTorque * gasInput;
            }
            else if (rec.rgbButtons[9] == 128 || rec.rgbButtons[11] == 128 || rec.rgbButtons[13] == 128)
            {
                WheelColliderBL.motorTorque = maxTorque * gasInput * -1;//run the wheels on back left and back right
                WheelColliderBR.motorTorque = maxTorque * gasInput * -1;
                WheelColliderFL.motorTorque = maxTorque * gasInput * -1;
                WheelColliderFR.motorTorque = maxTorque * gasInput * -1; 
            }
        }
        WheelColliderBL.brakeTorque = maxBrakeTorque * brakeInput;
        WheelColliderBR.brakeTorque = maxBrakeTorque * brakeInput;
        WheelColliderFL.brakeTorque = maxBrakeTorque * brakeInput;
        WheelColliderFR.brakeTorque = maxBrakeTorque * brakeInput;
    }

    void Update()//update is called once per frame
    {
        Quaternion flq;//rotation of wheel collider
        Vector3 flv;//position of wheel collider
        WheelColliderFL.GetWorldPose(out flv, out flq);//get wheel collider position and rotation
        FL.transform.position = flv;
        FL.transform.rotation = flq;

        Quaternion Blq;//rotation of wheel collider
        Vector3 Blv;//position of wheel collider
        WheelColliderBL.GetWorldPose(out Blv, out Blq);//get wheel collider position and rotation
        BL.transform.position = Blv;
        BL.transform.rotation = Blq;

        Quaternion fRq;//rotation of wheel collider
        Vector3 fRv;//position of wheel collider
        WheelColliderFR.GetWorldPose(out fRv, out fRq);//get wheel collider position and rotation
        FR.transform.position = fRv;
        FR.transform.rotation = fRq;

        Quaternion BRq;//rotation of wheel collider
        Vector3 BRv;//position of wheel collider
        WheelColliderBR.GetWorldPose(out BRv, out BRq);//get wheel collider position and rotation
        BR.transform.position = BRv;
        BR.transform.rotation = BRq;
    }
}