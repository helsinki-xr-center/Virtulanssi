using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbulanceControllerWASD : MonoBehaviour
{
    public WheelCollider WheelColliderFL, WheelColliderFR, WheelColliderBL, WheelColliderBR;

    public GameObject FL, FR, BL, BR;

    public float topSpeed = 250f;//the top speed
    public float maxTorque = 200f;//the maximum torque to apply to wheels
    public float maxSteerAngle = 45f;
    public float currentSpeed;
    public float maxBrakeTorque = 2200;


    private float forward, turn, brake;

    private Rigidbody rb;//rigid body of car


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() 
    {
        forward = Input.GetAxis("Vertical");
        turn = Input.GetAxis("Horizontal");
        brake = Input.GetAxis("Jump");

        WheelColliderFL.steerAngle = maxSteerAngle * turn;
        WheelColliderFR.steerAngle = maxSteerAngle * turn;

        currentSpeed = 2 * 22 / 7 * WheelColliderBL.radius * WheelColliderBL.rpm * 60 / 1000; //formula for calculating speed in kmph

        if (currentSpeed < topSpeed)
        {
            WheelColliderBL.motorTorque = maxTorque * forward; //runs all four wheels
            WheelColliderBR.motorTorque = maxTorque * forward;
            WheelColliderFL.motorTorque = maxTorque * forward;
            WheelColliderFR.motorTorque = maxTorque * forward;
        }//the top speed will not be accurate but will try to slow the car before top speed

        WheelColliderBL.brakeTorque = maxBrakeTorque * brake;
        WheelColliderBR.brakeTorque = maxBrakeTorque * brake;
        WheelColliderFL.brakeTorque = maxBrakeTorque * brake;
        WheelColliderFR.brakeTorque = maxBrakeTorque * brake;

    }
    void Update()
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