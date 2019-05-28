﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbulanceController : MonoBehaviour
{
    public WheelCollider WheelColliderFL;//the wheel colliders
    public WheelCollider WheelColliderFR;
    public WheelCollider WheelColliderBL;
    public WheelCollider WheelColliderBR;

    public GameObject FL;//the wheel gameobjects
    public GameObject FR;
    public GameObject BL;
    public GameObject BR;

    public float wheelInput;
    public float gasInput;
    public float brakeInput;
    private float axesMax = 32767f; // the max value for axes rotation and position (pedals and steering wheel);

    [Range(0f, 1.0f)]
    public float accelerationResponse = .8f; // How fast the car starts to accelerate as you push the acceleration pedal 
    [Range(0f, 1.0f)]
    public float brakeResponse = 1f;  // How fast the car starts to decelerate as you push the brake pedal 
    [Range(0f, 1.0f)]
    public float wheelResponse = .5f; // How responsive the wheel is

    public float topSpeed = 250f;//the top speed
    public float maxTorque = 400f;//the maximum torque to apply to wheels
    public float maxSteerAngle = 45f;
    public float currentSpeed;
    public float maxBrakeTorque = 2200;


    private float Forward;//forward axis
    private float Turn;//turn axis
    private float Brake;//brake axis

    private Rigidbody rb;//rigid body of car


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    void FixedUpdate() 
    {
        Forward = gasInput;
        Turn = wheelInput;
        Brake = brakeInput;

        WheelColliderFL.steerAngle = maxSteerAngle * Turn;
        WheelColliderFR.steerAngle = maxSteerAngle * Turn;

        currentSpeed = 2 * 22 / 7 * WheelColliderBL.radius * WheelColliderBL.rpm * 60 / 1000; //formula for calculating speed in kmph

        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)) // Checks that the steering wheel is connected and that the applictaion's main window is active
        {
            LogitechGSDK.DIJOYSTATE2ENGINES rec;
            rec = LogitechGSDK.LogiGetStateUnity(0); // Stores info about the device's positional information for axes, POVs and buttons.

            wheelInput = rec.lX / axesMax * wheelResponse; // Normalize x-axis position value (-1 to 1) 

            if (rec.lY > (accelerationResponse * axesMax))
            {
                gasInput = 0;
            }
            else if (rec.lY < (accelerationResponse * axesMax))
            {
                gasInput = rec.lY / -axesMax; // Normalize y-axis position value (-1 to 1) 
            }
            if (rec.lRz > (brakeResponse * axesMax))
            {
                brakeInput = 0;
            }
            else if (rec.lRz < (brakeResponse * axesMax))
            {
                brakeInput = rec.lRz / -axesMax; // Normalize z-axis position value (-1 to 1) 
            }
        }

        if (currentSpeed < topSpeed)
        {
            WheelColliderBL.motorTorque = maxTorque * Forward;//run the wheels on back left and back right
            WheelColliderBR.motorTorque = maxTorque * Forward;
            Debug.Log("Currentspeed < topSpeed");
        }

        /*if (currentSpeed >= topSpeed)
        {
            WheelColliderBL.motorTorque = maxTorque * Forward;//run the wheels on back left and back right
            WheelColliderBR.motorTorque = maxTorque * Forward;
            Debug.Log("Currentspeed < maxSpeed");
        }*///the top speed will not be accurate but will try to slow the car before top speed

        WheelColliderBL.brakeTorque = maxBrakeTorque * Brake;
        WheelColliderBR.brakeTorque = maxBrakeTorque * Brake;
        WheelColliderFL.brakeTorque = maxBrakeTorque * Brake;
        WheelColliderFR.brakeTorque = maxBrakeTorque * Brake;

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