using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Lane : MonoBehaviour
{
    [SerializeField]
    private TrafficSize trafficSize;
    [SerializeField]
    private DriverYield laneYield;
    [SerializeField]
    private SpeedLimits speedLimit;
    public Nodes[] nodesOnLane;
    public bool pointToPointLine;
    public bool drawAllLanes;

    public TrafficSize Traffic
    {
        get
        {
            return trafficSize;
        }
        set
        {
            trafficSize = value;
        }
    }

    public DriverYield LaneYield
    {
        get
        {
            return laneYield;
        }
        set
        {
            laneYield = value;
        }
    }

    public SpeedLimits SpeedLimit
    {
        get
        {
            return speedLimit;
        }
        set
        {
            speedLimit = value;
        }
    }


    private void Reset()
    {
        Traffic = TrafficSize.Average;
        LaneYield = DriverYield.Normal;
        SpeedLimit = SpeedLimits.KMH_40;
        nodesOnLane = null;
        pointToPointLine = true;
        drawAllLanes = true;

    }
}
