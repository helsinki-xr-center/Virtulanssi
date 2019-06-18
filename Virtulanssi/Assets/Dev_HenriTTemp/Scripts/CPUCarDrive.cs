using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPUCarDrive : MonoBehaviour
{

    public enum SteeringStatus
    {
        UnderSteering,
        OnTrack,
        OverSteering
    }

    public enum BrakingStatus
    {
        NoBraking,
        NoGass,
        LightBraking,
        SteadyBraking,
        HardBraking
    };

    public SteeringStatus steeringStatus = SteeringStatus.OnTrack;
    public BrakingStatus brakingStatus = BrakingStatus.NoBraking;

    public Vector3 target;
    public Nodes previousNode;
    public Nodes nextTargetNode;

    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;

    public Vector3 previousPosition;
    public float previousTime = -0.000001f;
    public float speed = 0f;

    public Text speedText;

    public float motor = 0f;
    public float steering = 0f;
    public float centerOfMassHeight = 0f;

    private const float checkDistance = 3f;
    private const float breakForce = -2.5f;

    bool randomNext = true;

    public float previousAngleToTarget = 0f;
    public float previousSteeringAngle = 0f;
    public float previousBrakeForce = 0f;
    public float angleTolerance = 2f;
    public int previousLeftRightStatus;
    public bool autoDrive = true;



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
        previousTime = Time.time;
        previousPosition = transform.position;
    }


    private void InitDirection()
    {
        float dist = 0f;
        Nodes n = previousNode;
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
            }
            target = nextTargetNode.transform.position;
            previousAngleToTarget = AngleToTarget;
            previousLeftRightStatus = LeftRightCheck;
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


    private void AutoDrive()
    {
 
        //Update node
        bool targetChanged = false;
        if (Vector3.Dot(transform.forward, target - transform.position) < 0)
        {
            targetChanged = true;
        }
        if (targetChanged)
        {
            Nodes n = nextTargetNode;
            previousNode = n;
            int counter = 0;
            while(true)
            {
                int i = NextRandomNode(n);
                n = nextTargetNode.OutNodes[i];
                if (Vector3.Distance(transform.position, n.transform.position) >= 3f)//Vector3.Dot(transform.forward, n.transform.position - transform.position) > 0f)
                {
                    nextTargetNode = n;
                    target = new Vector3(nextTargetNode.transform.position.x, 0f, nextTargetNode.transform.position.z);
                    previousAngleToTarget = AngleToTarget;
                    break;
                }
                counter++;
                if (counter > 50)
                {
                    Debug.Log("failed");
                    break;
                }
            }
        }

        //Steering status
        float angleToTarget = AngleToTarget;
        int leftRight = LeftRightCheck;
        float angleDifference = Mathf.Abs(previousAngleToTarget - angleToTarget);
        // Not steered over
        if (leftRight == previousLeftRightStatus)
        {
            if (leftRight == 0)
            {
                steeringStatus = SteeringStatus.OnTrack;
            }
            else
            {
                if (angleDifference > 5f)
                {
                    steeringStatus = SteeringStatus.UnderSteering;
                }
                else
                {
                    steeringStatus = SteeringStatus.OnTrack;
                }
            }
        }
        else
        {
            if (angleDifference > 5f)
            {
                steeringStatus = SteeringStatus.OverSteering;
            }
            else
            {
                steeringStatus = SteeringStatus.OnTrack;
            }
        }
        previousLeftRightStatus = leftRight; ;

        // steering correction
        bool tooMuchSpeed = false;
        if (steeringStatus == SteeringStatus.OnTrack)
        {
            steering = 0f;
            previousSteeringAngle = 0f;
        }
        else if (steeringStatus == SteeringStatus.UnderSteering)
        {
            if (angleToTarget > angleTolerance)
            {
                if (Mathf.Abs(previousSteeringAngle) == maxSteeringAngle)
                {
                    tooMuchSpeed = true;
                    //Debug.Log("Too much speed");
                }
                else
                {
                    if (leftRight < 0)
                    {
                        previousSteeringAngle = Mathf.Clamp(previousSteeringAngle - 1f, -maxSteeringAngle, maxSteeringAngle);
                    }
                    else
                    {
                        previousSteeringAngle = Mathf.Clamp(previousSteeringAngle + 1f, -maxSteeringAngle, maxSteeringAngle);
                    }
                }
            }
        } // oversteering
        else
        {
            if (angleDifference > 5f)
            {
                if (leftRight < 0)
                {
                    previousSteeringAngle = Mathf.Clamp(previousSteeringAngle + 1f, -maxSteeringAngle, maxSteeringAngle);
                }
                else
                {
                    previousSteeringAngle = Mathf.Clamp(previousSteeringAngle - 1f, -maxSteeringAngle, maxSteeringAngle);
                }
            }
        }

        // Speed status
        float targetSpeed = KmsToMs.Convert(previousNode.SpeedLimit);
        Vector3 newPos = new Vector3(transform.position.x, 0f, transform.position.z);
        float distanceTarveled = Vector3.Distance(newPos, previousPosition);
        float t = Time.time;
        float diff = t - previousTime;
        speed = distanceTarveled / (t - previousTime);
        if (speedText != null)
        {
            speedText.text = "Speed (km / h): " + speed * 3.6f;
        }
        previousTime = t;
        Debug.Log("speed: " + speed);
        // update braking status
        if (speed < targetSpeed && !tooMuchSpeed)
        {
            switch (brakingStatus)
            {
                case BrakingStatus.NoBraking:
                    motor = Mathf.Clamp(motor + 10f, 0f, maxMotorTorque);
                    break;
                case BrakingStatus.NoGass:
                    brakingStatus = BrakingStatus.NoBraking;
                    break;
                case BrakingStatus.LightBraking:
                    brakingStatus = BrakingStatus.NoGass;
                    break;
                case BrakingStatus.SteadyBraking:
                    brakingStatus = BrakingStatus.LightBraking;
                    break;
                case BrakingStatus.HardBraking:
                    brakingStatus = BrakingStatus.SteadyBraking;
                    break;
            }
        }
        else if (speed > targetSpeed || tooMuchSpeed)
        {
            switch (brakingStatus)
            {
                case BrakingStatus.NoBraking:
                    if (motor == 0)
                    {
                        brakingStatus = BrakingStatus.NoGass;
                    }
                    else
                    {
                        motor = Mathf.Clamp(motor - 10f, 0f, maxMotorTorque);
                    }
                    break;
                case BrakingStatus.NoGass:
                    motor = 0f;
                    brakingStatus = BrakingStatus.LightBraking;
                    break;
                case BrakingStatus.LightBraking:
                    motor = 0f;
                    brakingStatus = BrakingStatus.SteadyBraking;
                    break;
                case BrakingStatus.SteadyBraking:
                    motor = 0f;
                    brakingStatus = BrakingStatus.HardBraking;
                    previousBrakeForce = 2400f;
                    break;
                case BrakingStatus.HardBraking:
                    motor = 0f;
                    previousBrakeForce += 10f;
                    break;
            }
            if (speed < 36f)
            {
                motor = 100f;
                brakingStatus = BrakingStatus.NoBraking;
            }
        }

        switch(brakingStatus)
        {
            case BrakingStatus.NoBraking:
                previousBrakeForce = 0f;
                break;
            case BrakingStatus.NoGass:
                previousBrakeForce = 0f;
                break;
            case BrakingStatus.LightBraking:
                previousBrakeForce = 300f;
                break;
            case BrakingStatus.SteadyBraking:
                previousBrakeForce = 600f;
                break;
            case BrakingStatus.HardBraking:
                break;
        }
        foreach (AxleInfo a in axleInfos)
        {
            a.leftWheel.brakeTorque = previousBrakeForce;
            a.rightWheel.brakeTorque = previousBrakeForce;
            if (a.steering)
            {
                a.leftWheel.steerAngle = previousSteeringAngle;
                a.rightWheel.steerAngle = previousSteeringAngle;
            }
            if (a.motor)
            {
                a.leftWheel.motorTorque = motor;
                a.rightWheel.motorTorque = motor;
            }
        }
        previousPosition = newPos;

    }

    private void ManualDrive()
    {
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        // light braking
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (AxleInfo a in axleInfos)
            {
                a.leftWheel.brakeTorque = 300f;
                a.rightWheel.brakeTorque = 300f;
            }
        }
        // steady braking
        else if (Input.GetKey(KeyCode.Z))
        {
            foreach (AxleInfo a in axleInfos)
            {
                a.leftWheel.brakeTorque = 600f;
                a.rightWheel.brakeTorque = 600f;
            }
        }
        // heavy braking
        else if (Input.GetKey(KeyCode.X))
        {
            foreach (AxleInfo a in axleInfos)
            {
                a.leftWheel.brakeTorque = 2400f;
                a.rightWheel.brakeTorque = 2400f;
            }
        }
        else
        {
            foreach (AxleInfo a in axleInfos)
            {
                a.leftWheel.brakeTorque = 0f;
                a.rightWheel.brakeTorque = 0f;
            }
        }

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
        float dist = Vector3.Distance(newPos, previousPosition);
        ShowSpeedInUI(dist);
        previousPosition = newPos;
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
            float inKmS = distInMeters * 3.6f / (t - previousTime);
            previousTime = t;
            speedText.text = "Speed (km / h) : " + inKmS;
        }
    }

    private float AngleToTarget
    {
        get
        {
            if (target == null)
            {
                return 0f;
            }
            return Vector3.Angle(transform.forward, (target - transform.position).normalized);
        }
    }

    private int LeftRightCheck
    {
        get
        {
            if (target == null)
            {
                return 0;
            }
            Vector3 cross = Vector3.Cross(transform.forward, (target - transform.position));
            float dir = Vector3.Dot(cross, transform.up);
            if (dir > 0f)
            {
                return 1;
            }
            else if (dir < 0f)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

    }

    private void FixedUpdate()
    {
        if (autoDrive)
        {
            AutoDrive();
        }
        else
        {
            ManualDrive();
        }
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
