using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPUCarDrive : MonoBehaviour
{
    public float targetSpeed;

    public enum DriveState
    {
        IsStopped,
        AccelerateToTargetSpeed,
        KeepTargetSpeed,
        DecelerateToTargetSpeed,
        DecelerateToStop,
        EmergencyStop
    };


    DriveState driveState = DriveState.IsStopped;
    public Vector3 target;
    public Vector3 midpoint;
    public Nodes previousNode;
    public Nodes nextTargetNode;

    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public Vector3 lastPost;
    public float lastTime = 0.1f;
    public Text speedText;
    public float motor = 0f;
    public float steering = 0f;
    public float centerOfMassHeight = 0f;
    private const float checkDistance = 3f;
    bool randomNext = true;


    private void Awake()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        Vector3 com = rb.centerOfMass;
        com.y = centerOfMassHeight;
        rb.centerOfMass = com;
    }

    private void Start()
    {
        InitDirection();
    }

    void Update()
    {
        switch(driveState)
        {
            case DriveState.IsStopped:
                break;
            case DriveState.AccelerateToTargetSpeed:
                break;
            case DriveState.KeepTargetSpeed:
                break;
            case DriveState.DecelerateToTargetSpeed:
                break;
            case DriveState.DecelerateToStop:
                break;
            case DriveState.EmergencyStop:
                break;
        }
    }

    private void InitDirection()
    {
        float dist = 0f;
        Nodes n = previousNode;
        bool midFound = false;
        Nodes mid = null;
        if (randomNext)
        {
            while (dist < checkDistance)
            {
                int i = NextRandomNode(n);
                dist += Vector3.Distance(previousNode.transform.position, previousNode.OutNodes[i].transform.position);
                if (dist >= checkDistance)
                {
                    nextTargetNode = previousNode.OutNodes[i];
                    break;
                }
                if (!midFound)
                {
                    mid = previousNode.OutNodes[i];
                    if (dist >= checkDistance * 0.5f)
                    {
                        midFound = true;
                    }
                }
            }
            target = nextTargetNode.transform.position;
            if (mid == null)
            {
                midpoint = previousNode.transform.position + (target - previousNode.transform.position) * 0.5f;
            }
            else
            {
                midpoint = mid.transform.position;
            }
        }
    }

    private int NextRandomNode(Nodes n)
    {
        int i = n.OutNodes.Length - 1;
        if (i > 0)
        {
            int index = Random.Range(0, i);
            return index;
        }
        else
        {
            return 0;
        }
    }

    private void UpdateTargetDirection()
    {

    }

    private void AccelerateToTargetSpeed()
    {

    }

    private void KeepTargetSpeed()
    {

    }

    private void DecelerateToTargetSpeed()
    {

    }

    private void DecelerateToStop()
    {

    }

    private void DoEmergencyStop()
    {

    }

    private void ApplyLocalPositionsToVisuals(WheelCollider c)
    {
        
        Transform visualWheel = c.transform.GetChild(0);
        Vector3 position;
        Quaternion rotation;
        c.GetWorldPose(out position, out rotation);
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    private void ShowSpeedInUI(float distInMeters)
    {
        if (speedText != null)
        {
            float t = Time.time;
            float inKmS = distInMeters * 3.6f / (t - lastTime);
            lastTime = t;
            speedText.text = "Speed (km / h) : " + inKmS;
        }
    }

    private void FixedUpdate()
    {
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        foreach (AxleInfo a in axleInfos)
        {
            if (a.steering)
            {
                a.leftWheel.steerAngle = steering;
                a.rightWheel.steerAngle = steering;
            }
            if (a.motor)
            {
                a.leftWheel.motorTorque = motor;
                a.rightWheel.motorTorque = motor;
            }
            ApplyLocalPositionsToVisuals(a.leftWheel);
            ApplyLocalPositionsToVisuals(a.rightWheel);
        }
        Vector3 newPos = new Vector3(transform.position.x, 0f, transform.position.z);
        float dist = Vector3.Distance(newPos, lastPost);
        ShowSpeedInUI(dist);
        lastPost = newPos;
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}
