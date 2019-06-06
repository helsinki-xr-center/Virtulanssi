using System.Collections;
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

    public float steeringWheelInput;
    public float gasInput;
    public float brakeInput;
    private float axesMax = 32767f; // the max value for axes rotation and position (pedals and steering wheel);

    [Range(0f, 1.0f)]
    public float accelerationResponse = .8f; // How fast the car starts to accelerate as you push the acceleration pedal 
    [Range(0f, 1.0f)]
    public float brakeResponse = 1f;  // How fast the car starts to decelerate as you push the brake pedal 
    [Range(0f, 1.0f)]
    public float wheelResponse = .5f; // How responsive the wheel is

    public float topSpeed = 200f;//the top speed
    public float maxTorque = 400f;//the maximum torque to apply to wheels
    public float maxSteerAngle = 45f;
    public float currentSpeed;
    public float maxBrakeTorque = 2200;

    private float reverse;
    private float forward;//forward axis
    private float turn;//turn axis
    private float brake;//brake axis

    private Rigidbody rb;//rigid body of car

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        reverse = gasInput * -1;
        forward = gasInput;
        turn = steeringWheelInput;
        brake = brakeInput;

        WheelColliderFL.steerAngle = maxSteerAngle * turn;
        WheelColliderFR.steerAngle = maxSteerAngle * turn;

        currentSpeed = 2 * 22 / 7 * WheelColliderBL.radius * WheelColliderBL.rpm * 60 / 1000; //formula for calculating speed in kmph

        LogitechGSDK.DIJOYSTATE2ENGINES rec;
        rec = LogitechGSDK.LogiGetStateUnity(0); // Stores info about the device's positional information for axes, POVs and buttons.

        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)) // Checks that the steering wheel is connected and that the applictaion's main window is active
        {
            steeringWheelInput = rec.lX / axesMax * wheelResponse; // Normalize x-axis position value (-1 to 1) 

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
            if (rec.rgbButtons[8] == 128 || rec.rgbButtons[10] == 128 || rec.rgbButtons[12] == 128)
            {
                WheelColliderBL.motorTorque = maxTorque * forward; //runs all four wheels
                WheelColliderBR.motorTorque = maxTorque * forward;
                WheelColliderFL.motorTorque = maxTorque * forward;
                WheelColliderFR.motorTorque = maxTorque * forward;
            }
            if (rec.rgbButtons[9] == 128 || rec.rgbButtons[11] == 128 || rec.rgbButtons[13] == 128)
            {
                WheelColliderBL.motorTorque = maxTorque * forward * -1;//run the wheels on back left and back right
                WheelColliderBR.motorTorque = maxTorque * forward * -1;
            }
        }
        WheelColliderBL.brakeTorque = maxBrakeTorque * brake;
        WheelColliderBR.brakeTorque = maxBrakeTorque * brake;
        WheelColliderFL.brakeTorque = maxBrakeTorque * brake;
        WheelColliderFR.brakeTorque = maxBrakeTorque * brake;
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