using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class ParallelBezierSplines : MonoBehaviour
{
    [SerializeField]
    private bool initialized;
    [SerializeField]
    private bool lanesSet;
    [SerializeField]
    private bool nodesSet;

    [SerializeField]
    private Vector3[] points;
    [SerializeField]
    private Vector3[] leftLanePoints1;
    [SerializeField]
    private Vector3[] leftLanePoints2;
    [SerializeField]
    private Vector3[] leftLanePoints3;
    [SerializeField]
    private Vector3[] rightLanePoints1;
    [SerializeField]
    private Vector3[] rightLanePoints2;
    [SerializeField]
    private Vector3[] rightLanePoints3;


    [SerializeField]
    private float[] segmentLengths;
    [SerializeField]
    private int[] nodesOnSegment;
    [SerializeField]
    private int nodeCount;
    
    [SerializeField]
    private float[] leftSegmentLengths1;
    [SerializeField]
    private float[] leftSegmentLengths2;
    [SerializeField]
    private float[] leftSegmentLengths3;
    [SerializeField]
    private float[] rightSegmentLengths1;
    [SerializeField]
    private float[] rightSegmentLengths2;
    [SerializeField]
    private float[] rightSegmentLengths3;

    [SerializeField]
    private float splineLength;
    
    [SerializeField]
    private float[] leftSplineLengths;
    [SerializeField]
    private float[] rightSplineLengths;
    
    
    [SerializeField]
    private float[] leftSpacings1;
    [SerializeField]
    private float[] leftSpacings2;
    [SerializeField]
    private float[] leftSpacings3;
    [SerializeField]
    private float[] rightSpacings1;
    [SerializeField]
    private float[] rightSpacings2;
    [SerializeField]
    private float[] rightSpacings3;

    [SerializeField]
    private BezierControlPointMode[] modes;
    
    [SerializeField]
    private BezierControlPointMode[] leftModes1;
    [SerializeField]
    private BezierControlPointMode[] leftModes2;
    [SerializeField]
    private BezierControlPointMode[] leftModes3;

    [SerializeField]
    private BezierControlPointMode[] rightModes1;
    [SerializeField]
    private BezierControlPointMode[] rightModes2;
    [SerializeField]
    private BezierControlPointMode[] rightModes3;

    [SerializeField]
    private TrafficSize traffic;
    [SerializeField]
    private SpeedLimits speedLimit;
    [SerializeField]
    public List<GameObject> wayPointsLeft1;
    [SerializeField]
    public List<GameObject> wayPointsLeft2;
    [SerializeField]
    public List<GameObject> wayPointsLeft3;
    [SerializeField]
    public List<GameObject> wayPointsRight1;
    [SerializeField]
    public List<GameObject> wayPointsRight2;
    [SerializeField]
    public List<GameObject> wayPointsRight3;
    public GameObject roadNetwork;
    public GameObject roadParent;
    public GameObject leftParent1;
    public GameObject leftParent2;
    public GameObject leftParent3;
    public GameObject rightParent1;
    public GameObject rightParent2;
    public GameObject rightParent3;
    public Nodes[] startNodes;
    public Nodes[] endNodes;

    [SerializeField]
    private int leftLaneCount;
    [SerializeField]
    private int rightLaneCount;
    [SerializeField]
    private bool busLaneLeft;
    [SerializeField]
    private int busLeftStart;
    [SerializeField]
    private int busLeftEnd;
    [SerializeField]
    private int busRightStart;
    [SerializeField]
    private int busRightEnd;
    [SerializeField]
    private bool busLaneRight;
    // To save space, possible lane changes are arranged in ana array, the order is:
    // 0 = right side, lane 1 change to lane 2
    // 1 = right side, lane 2 change to lane 1
    // 2 = right side, lane 2 change to lane 3
    // 3 = right side, lane 3 change to lane 2
    // 4 = left side, lane 1 change to lane 2
    // 5 = left side, lane 2 change to lane 1
    // 6 = left side, lane 2 change to lane 3
    // 7 = left side, lane 3 change to lane 2
    public bool[] permittedLaneChanges;
    public int[] laneChangeStartIndex;
    public int[] laneChangeEndIndex;


    public int LeftLaneCount
    {
        get { return leftLaneCount; }
        set
        {
            int v = Mathf.Clamp(value, 0, 3);
            if (leftLaneCount != v)
            {
                leftLaneCount = v;
            }
        }
    }

    public int RightLaneCount
    {
        get { return rightLaneCount; }
        set
        {
            int v = Mathf.Clamp(value, 0, 3);
            if (rightLaneCount != v)
            {
                rightLaneCount = v;

            }
        }
    }

    public bool BusLaneLeft
    {
        get
        {
            return busLaneLeft;
        }
        set
        {
            busLaneLeft = value;
        }
    }

    public bool BusLaneRight
    {
        get
        {
            return busLaneRight;
        }
        set
        {
            busLaneRight = value;
        }
    }

    public int BusLeftStart
    {
        get
        {
            return busLeftStart;
        }
        set
        {
            busLeftStart = value;
        }
    }

    public int BusLeftEnd
    {
        get
        {
            return busLeftEnd;
        }
        set
        {
            busLeftEnd = value;
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

    public int BusRightStart
    {
        get
        {
            return busRightStart;
        }
        set
        {
            busRightStart = value;
        }
    }

    public int BusRightEnd
    {
        get
        {
            return busRightEnd;
        }
        set
        {
            busRightEnd = value;
        }
    }

    public bool Initialized
    {
        get
        {
            return initialized;
        }
        set
        {
            initialized = value;
        }
    }

    public bool LanesSet
    {
        get
        {
            return lanesSet;
        }
        set
        {
            lanesSet = value;
        }
    }

    public bool NodesSet
    {
        get
        {
            return nodesSet;
        }
        set
        {
            nodesSet = value;
        }
    }

    public int NodeCount
    {
        get
        {
            return nodeCount;
        }
        set
        {
            nodeCount = value;
        }
    }

    public TrafficSize Traffic
    {
        get
        {
            return traffic;
        }
        set
        {
            traffic = value;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        LeftLaneCount = leftLaneCount;
        RightLaneCount = rightLaneCount;
        //LanePositioning = lanePositioning;
    }

#endif

    public Vector3 GetStartPoint()
    {
        return points[0];
    }

    public int GetNodesOnSegment(int segment)
    {
        if (segment > nodesOnSegment.Length - 1)
        {
            return -1;
        }
        return nodesOnSegment[segment];
    }

    public void SetNodesOnSegment(int segment, int amount)
    {
        if (segment > nodesOnSegment.Length - 1)
        {
            return;
        }
        nodesOnSegment[segment] = amount;
    }

    public int SegmentCount
    {
        get
        {
            return segmentLengths.Length;
        }
    }

    public int ControlPointCount
    {
        get
        {
            return points.Length;
        }
    }

    public float SplineLength
    {
        get
        {
            return splineLength;
        }
    }

    //not used yet
    public float GetLaneLength(int lane, bool rightLane)
    {
        float length = 0f;
        if (rightLane)
        {
            if (lane < rightLaneCount)
            {
                length = rightSplineLengths[lane];
            }
        }
        else
        {
            if (lane < leftLaneCount)
            {
                length = leftSplineLengths[lane];
            }
        }
        return length;
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public Vector3 GetLanePoint(int lane, int index, bool rightLane)
    {
        Vector3 v = Vector3.zero;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    v = rightLanePoints1[index];
                    break;
                case 1:
                    v = rightLanePoints2[index];
                    break;
                case 2:
                    v = rightLanePoints3[index];
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    v = leftLanePoints1[index];
                    break;
                case 1:
                    v = leftLanePoints2[index];
                    break;
                case 2:
                    v = leftLanePoints3[index];
                    break;
            }
        }
        return v;
    }
 
    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
    }

    public float GetLaneSpacing(int lane, int node, bool rightLane)
    {
        float space = 0f;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    space = rightSpacings1[node / 3];
                    break;
                case 1:
                    space = rightSpacings2[node / 3];
                    break;
                case 2:
                    space = rightSpacings3[node / 3];
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    space = leftSpacings1[node / 3];
                    break;
                case 1:
                    space = leftSpacings2[node / 3];
                    break;
                case 2:
                    space = leftSpacings3[node / 3];
                    break;
            }
        }
        return space;
    }

    public void SetLaneSpacing(int lane, int node, float spacing, bool rightLane)
    {
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    rightSpacings1[node / 3] = spacing;
                    break;
                case 1:
                    rightSpacings2[node / 3] = spacing;
                    break;
                case 2:
                    rightSpacings3[node / 3] = spacing;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    leftSpacings1[node / 3] = spacing;
                    break;
                case 1:
                    leftSpacings2[node / 3] = spacing;
                    break;
                case 2:
                    leftSpacings3[node / 3] = spacing;
                    break;
            }
        }
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (index > 0)
            {
                points[index - 1] += delta;
            }
            if (index + 1 < points.Length)
            {
                points[index + 1] += delta;
            }
        }
        //***********
        points[index] = point;
        EnforceMode(index);
    }

    public void SetLanePoint(int lane, int index, Vector3 point, bool rightLane)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        Vector3[] pointArray = rightLanePoints1;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    pointArray = rightLanePoints1;
                    break;
                case 1:
                    pointArray = rightLanePoints2;
                    break;
                case 2:
                    pointArray = rightLanePoints3;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    pointArray = leftLanePoints1;
                    break;
                case 1:
                    pointArray = leftLanePoints2;
                    break;
                case 2:
                    pointArray = leftLanePoints3;
                    break;
            }
        }

        if (index % 3 == 0)
        {
            Vector3 delta = point - pointArray[index];
            if (index > 0)
            {
                pointArray[index - 1] += delta;
            }
            if (index + 1 < ControlPointCount)
            {
                pointArray[index + 1] += delta;
            }
        }
        //***********
        pointArray[index] = point;
        EnforceLaneMode(lane, index, rightLane);
    }

    public void AdjustLane(int lane, int index, Vector3 moved, bool rightLane)
    {
        Vector3[] pointArray = rightLanePoints1;
        int ind;

        if (rightLane)
        {
            ind = index;
            switch (lane)
            {
                case 0:
                    pointArray = rightLanePoints1;
                    break;
                case 1:
                    pointArray = rightLanePoints2;
                    break;
                case 2:
                    pointArray = rightLanePoints3;
                    break;
            }
        }
        else
        {
            ind = ControlPointCount - 1 - index;
            switch (lane)
            {
                case 0:
                    pointArray = leftLanePoints1;
                    break;
                case 1:
                    pointArray = leftLanePoints2;
                    break;
                case 2:
                    pointArray = leftLanePoints3;
                    break;
            }
        }
        if (index % 3 == 0)
        {
            pointArray[ind] = GetControlPoint(index) + moved;
            if (ind > 0)
            {
                if (rightLane)
                {
                    pointArray[index - 1] = GetControlPoint(index - 1) + moved;
                }
                else
                {
                    pointArray[ind - 1] = GetControlPoint(index + 1) + moved;
                }
            }
            if (ind < pointArray.Length - 1)
            {
                if (rightLane)
                {
                    pointArray[index + 1] = GetControlPoint(index + 1) + moved;
                }
                else
                {
                    pointArray[ind + 1] = GetControlPoint(index - 1) + moved;
                }
            }
        }
        else
        {
            pointArray[ind] += moved;
            EnforceLaneMode(lane, ind, rightLane);
        }
    }
    
    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = modes[modeIndex];
        EnforceLane(index, mode, ref points);
        return;
    }
    
    private void EnforceLaneMode(int lane, int index, bool rightLane)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = GetLaneMode(lane, index, rightLane);
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    EnforceLane(index, mode, ref rightLanePoints1);
                    break;
                case 1:
                    EnforceLane(index, mode, ref rightLanePoints2);
                    break;
                case 2:
                    EnforceLane(index, mode, ref rightLanePoints3);
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    EnforceLane(index, mode, ref leftLanePoints1);
                    break;
                case 1:
                    EnforceLane(index, mode, ref leftLanePoints2);
                    break;
                case 2:
                    EnforceLane(index, mode, ref leftLanePoints3);
                    break;
            }
        }
    }

    private void EnforceLane(int index, BezierControlPointMode mode, ref Vector3[] pointArray)
    {
        int modeIndex = (index + 1) / 3;
        // We don't enforce if we are at end points or the current mode is set to 'FREE'.
        if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == modes.Length - 1)
        {
            return;
        }
        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = ControlPointCount - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= ControlPointCount)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= ControlPointCount)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = ControlPointCount - 2;
            }
        }

        Vector3 middle = pointArray[middleIndex];
        Vector3 enforcedTangent = middle - pointArray[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, pointArray[enforcedIndex]);
        }
        pointArray[enforcedIndex] = middle + enforcedTangent;
    }

    public Vector3 GetDirectionWhenTraveled(float distanceTraveled)
    {
        float dist = distanceTraveled;
        int segment = 0;
        int segmentCount = segmentLengths.Length;
        if (segmentCount > 1)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                if (dist - segmentLengths[i] < 0)
                {
                    segment = i;
                    break;
                }
                else
                {
                    dist -= segmentLengths[i];
                }
            }
        }
        float fraction = dist / segmentLengths[segment];
        return GetSegmentedDirection(segment, fraction);
    }

    private Vector3 GetSegmentedDirection(int segment, float fraction)
    {

        return GetSegmentedVelocity(segment, fraction).normalized;
    }

    private Vector3 GetSegmentedVelocity(int segment, float fraction)
    {
        int i = segment * 3;
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], fraction))
            - transform.position;
    }

    public Vector3 GetSegmentedPoint(int segment, float fraction)
    {
        if (segment == segmentLengths.Length)
        {
            segment -= 1;
        }
        int i = segment * 3;
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], fraction));
    }

    public Vector3 GetSegmentedPointLane(int lane, int segment, float fraction, bool rightLane)
    {
        if (segment == segmentLengths.Length)
        {
            segment -= 1;
        }
        int i = segment * 3;
        Vector3 v = Vector3.zero;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints1[i], rightLanePoints1[i + 1],
                        rightLanePoints1[i + 2], rightLanePoints1[i + 3], fraction));
                    break;
                case 1:
                    v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints2[i], rightLanePoints2[i + 1],
                        rightLanePoints2[i + 2], rightLanePoints2[i + 3], fraction));
                    break;
                case 2:
                    v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints3[i], rightLanePoints3[i + 1],
                        rightLanePoints3[i + 2], rightLanePoints3[i + 3], fraction));
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints1[i], leftLanePoints1[i + 1],
                        leftLanePoints1[i + 2], leftLanePoints1[i + 3], fraction));
                    break;
                case 1:
                    v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints2[i], leftLanePoints2[i + 1],
                        leftLanePoints2[i + 2], leftLanePoints2[i + 3], fraction));
                    break;
                case 2:
                    v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints3[i], leftLanePoints3[i + 1],
                        leftLanePoints3[i + 2], leftLanePoints3[i + 3], fraction));
                    break;
            }
        }
        return v;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public Vector3 GetDirectionLane(int lane, float t, bool rightLane)
    {
        return GetVelocityLane(lane, t, rightLane).normalized;
    }

    public Vector3 GetVelocity(float t)
    {
        int i = GetI(ref t);
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t))
            - transform.position;
    }

    public Vector3 GetVelocityLane(int lane, float t, bool rightLane)
    {
        Vector3 v = Vector3.zero;
        int i = GetI(ref t);
        if (rightLane)
        {
            if (lane >= RightLaneCount)
            {
                return v;
            }
            switch (lane)
            {
                case 0:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints1[i],
                        rightLanePoints1[i + 1], rightLanePoints1[i + 2], rightLanePoints1[i + 3], t))
                        - transform.position;
                    break;
                case 1:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints2[i],
                        rightLanePoints2[i + 1], rightLanePoints2[i + 2], rightLanePoints2[i + 3], t))
                        - transform.position;
                    break;
                case 2:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints3[i],
                        rightLanePoints3[i + 1], rightLanePoints3[i + 2], rightLanePoints3[i + 3], t))
                        - transform.position;
                    break;
            }
        }
        else
        {
            if (lane >= LeftLaneCount)
            {
                return v;
            }
            switch (lane)
            {
                case 0:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints1[i],
                        leftLanePoints1[i + 1], leftLanePoints1[i + 2], leftLanePoints1[i + 3], t))
                        - transform.position;
                    break;
                case 1:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints2[i],
                        leftLanePoints2[i + 1], leftLanePoints2[i + 2], leftLanePoints2[i + 3], t))
                        - transform.position;
                    break;
                case 2:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints3[i],
                        leftLanePoints3[i + 1], leftLanePoints3[i + 2], leftLanePoints3[i + 3], t))
                        - transform.position;
                    break;
            }
        }
        return v;
    }

    private int GetI(ref float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return i;
    }

    public void RecalculateLength(int index)
    {
        int segment = index / 3;
        if (segment == segmentLengths.Length)
        {
            segment -= 1;
        }
        float dist = 0f;
        Vector3 prev = GetSegmentedPoint(segment, 0f);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPoint(segment, (float)i / 1000f);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        segmentLengths[segment] = dist;
        if (segment == 0)
        {
            UpdateSplineLength();
            return;
        }
        else
        {
            if (segment == 0)
            {
                segment = segmentLengths.Length - 1;
            }
            else
            {
                segment -= 1;
            }
            dist = 0f;
            prev = GetSegmentedPoint(segment, 0f);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPoint(segment, i / 1000f);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            segmentLengths[segment] = dist;
            UpdateSplineLength();
        }
    }

    public void RecalculateLaneLength(int lane, int index, bool rightLane)
    {
        int segment = index / 3;
        if (segment == segmentLengths.Length)
        {
            segment -= 1;
        }
        float dist = 0f;
        float[] lengthArray = rightSegmentLengths1;
        if (rightLane)
        {
            switch(lane)
            {
                case 0:
                    lengthArray = rightSegmentLengths1;
                    break;
                case 1:
                    lengthArray = rightSegmentLengths2;
                    break;
                case 2:
                    lengthArray = rightSegmentLengths3;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    lengthArray = leftSegmentLengths1;
                    break;
                case 1:
                    lengthArray = leftSegmentLengths2;
                    break;
                case 2:
                    lengthArray = leftSegmentLengths3;
                    break;
            }
        }
        Vector3 prev = GetSegmentedPointLane(lane, segment, 0f, rightLane);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPointLane(lane, segment, i / 1000f, rightLane);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        lengthArray[segment] = dist;

        if (segment == 0)
        {
            UpdateLaneLength(lane, rightLane);
            return;
        }
        else
        {
            if (segment == 0)
            {
                segment = segmentLengths.Length - 1;
            }
            else
            {
                segment -= 1;
            }
            dist = 0f;
            prev = GetSegmentedPointLane(lane, segment, 0f, rightLane);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPointLane(lane, segment, i / 1000f, rightLane);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            lengthArray[segment] = dist;
            UpdateLaneLength(lane, rightLane);
        }
    }

    private void UpdateSplineLength()
    {
        float length = 0;
        for (int i = 0; i < segmentLengths.Length; i++)
        {
            length += segmentLengths[i];
        }
        splineLength = length;
    }

    private void UpdateLaneLength(int lane, bool rightLane)
    {
        float length = 0f;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    for (int i = 0; i < rightSegmentLengths1.Length; i++)
                    {
                        length += rightSegmentLengths1[i];
                    }
                    rightSplineLengths[lane] = length;
                    break;
                case 1:
                    for (int i = 0; i < rightSegmentLengths2.Length; i++)
                    {
                        length += rightSegmentLengths2[i];
                    }
                    rightSplineLengths[lane] = length;
                    break;
                case 2:
                    for (int i = 0; i < rightSegmentLengths3.Length; i++)
                    {
                        length += rightSegmentLengths3[i];
                    }
                    rightSplineLengths[lane] = length;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    for (int i = 0; i < leftSegmentLengths1.Length; i++)
                    {
                        length += leftSegmentLengths1[i];
                    }
                    leftSplineLengths[lane] = length;
                    break;
                case 1:
                    for (int i = 0; i < leftSegmentLengths2.Length; i++)
                    {
                        length += leftSegmentLengths2[i];
                    }
                    leftSplineLengths[lane] = length;
                    break;
                case 2:
                    for (int i = 0; i < leftSegmentLengths3.Length; i++)
                    {
                        length += leftSegmentLengths3[i];
                    }
                    leftSplineLengths[lane] = length;
                    break;
            }
        }
    }

    public void AddCurve()
    {
        // First, update the main spline
        Vector3 point = points[points.Length - 1];
        //use the length of the previous segment as a measure for the new one
        float length = segmentLengths[segmentLengths.Length - 1] / 3f;
        //continue to the direction of the previous segment
        Vector3 dir = GetSegmentedDirection(segmentLengths.Length - 1, 1f);
        // Array requires System-namespace. points is passed as a REFERENCE (not a copy)
        // 1. Add new points
        Array.Resize(ref points, points.Length + 3);
        point += length * dir;
        points[points.Length - 3] = point;
        point += length * dir;
        points[points.Length - 2] = point;
        point += length * dir;
        points[points.Length - 1] = point;
        // 2. Resize segmentLengths
        Array.Resize(ref segmentLengths, segmentLengths.Length + 1);
        float lastSegmentLegth = Vector3.Distance(points[points.Length - 4], points[points.Length - 1]);
        segmentLengths[segmentLengths.Length - 1] = lastSegmentLegth;
        // 3. Resize nodesOnSegment
        Array.Resize(ref nodesOnSegment, nodesOnSegment.Length + 1);
        // 4. Resize modes
        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        // 5. Add new right lane points
        //int size = points.Length;
        Vector3 rightDir = new Vector3(dir.z, dir.y, -dir.x);
        //***********
        float space = 0f;
        Vector3 spacing;
        //rightLanePoints1
        Array.Resize(ref rightLanePoints1, rightLanePoints1.Length + 3);
        space += rightSpacings1[rightSpacings1.Length - 1];
        spacing = space * rightDir;
        rightLanePoints1[rightLanePoints1.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints1[rightLanePoints1.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints1[rightLanePoints1.Length - 1] = points[points.Length - 1] + spacing;
        //rightLanePoints2
        Array.Resize(ref rightLanePoints2, rightLanePoints2.Length + 3);
        space += rightSpacings2[rightSpacings2.Length - 1];
        spacing = space * rightDir;
        rightLanePoints2[rightLanePoints2.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints2[rightLanePoints2.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints2[rightLanePoints2.Length - 1] = points[points.Length - 1] + spacing;
        //rightLanePoints3
        Array.Resize(ref rightLanePoints3, rightLanePoints3.Length + 3);
        space += rightSpacings3[rightSpacings3.Length - 1];
        spacing = space * rightDir;
        rightLanePoints3[rightLanePoints3.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints3[rightLanePoints3.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints3[rightLanePoints3.Length - 1] = points[points.Length - 1] + spacing;

        // 6. Add new left lane points
        space = 0f;
        //leftLanePoints1
        Array.Resize(ref leftLanePoints1, leftLanePoints1.Length + 3);
        for (int i = leftLanePoints1.Length - 4; i >= 0; i--)
        {
            leftLanePoints1[i + 3] = leftLanePoints1[i];
        }
        space += leftSpacings1[0];
        spacing = rightDir * space;
        leftLanePoints1[0] = points[points.Length - 1] - spacing;
        leftLanePoints1[1] = points[points.Length - 2] - spacing;
        leftLanePoints1[2] = points[points.Length - 3] - spacing;
        //leftLanePoints2
        Array.Resize(ref leftLanePoints2, leftLanePoints2.Length + 3);
        for (int i = leftLanePoints2.Length - 4; i >= 0; i--)
        {
            leftLanePoints2[i + 3] = leftLanePoints2[i];
        }
        space += leftSpacings2[0];
        spacing = rightDir * space;
        leftLanePoints2[0] = points[points.Length - 1] - spacing;
        leftLanePoints2[1] = points[points.Length - 2] - spacing;
        leftLanePoints2[2] = points[points.Length - 3] - spacing;
        //leftLanePoints3
        Array.Resize(ref leftLanePoints3, leftLanePoints3.Length + 3);
        for (int i = leftLanePoints3.Length - 4; i >= 0; i--)
        {
            leftLanePoints3[i + 3] = leftLanePoints3[i];
        }
        space += leftSpacings3[0];
        spacing = rightDir * space;
        leftLanePoints3[0] = points[points.Length - 1] - spacing;
        leftLanePoints3[1] = points[points.Length - 2] - spacing;
        leftLanePoints3[2] = points[points.Length - 3] - spacing;

        // Add new spacings, copy previous value
        // 7. Update right spacings
        //rightSpacings1
        Array.Resize(ref rightSpacings1, rightSpacings1.Length + 1);
        rightSpacings1[rightSpacings1.Length - 1] = rightSpacings1[rightSpacings1.Length - 2];
        //rightSpacings2
        Array.Resize(ref rightSpacings2, rightSpacings2.Length + 1);
        rightSpacings2[rightSpacings2.Length - 1] = rightSpacings2[rightSpacings2.Length - 2];
        //rightSpacings3
        Array.Resize(ref rightSpacings3, rightSpacings3.Length + 1);
        rightSpacings3[rightSpacings3.Length - 1] = rightSpacings3[rightSpacings3.Length - 2];

        // 8. Update left spacings
        //leftSpacings1
        Array.Resize(ref leftSpacings1, leftSpacings1.Length + 1);
        for (int i = leftSpacings1.Length - 2; i >= 0; i--)
        {
            leftSpacings1[i + 1] = leftSpacings1[i];
        }
        leftSpacings1[0] = leftSpacings1[1];
        //leftSpacings2
        Array.Resize(ref leftSpacings2, leftSpacings2.Length + 1);
        for (int i = leftSpacings2.Length - 2; i >= 0; i--)
        {
            leftSpacings2[i + 1] = leftSpacings2[i];
        }
        leftSpacings2[0] = leftSpacings2[1];
        //leftSpacings3
        Array.Resize(ref leftSpacings3, leftSpacings3.Length + 1);
        for (int i = leftSpacings3.Length - 2; i >= 0; i--)
        {
            leftSpacings3[i + 1] = leftSpacings3[i];
        }
        leftSpacings3[0] = leftSpacings3[1];

        // 9. Resize right segments
        //rightSegmentLengths1
        Array.Resize(ref rightSegmentLengths1, rightSegmentLengths1.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints1[rightLanePoints1.Length - 4],
            rightLanePoints1[rightLanePoints1.Length - 1]);
        rightSegmentLengths1[rightSegmentLengths1.Length - 1] = lastSegmentLegth;
        //rightSegmentLengths2
        Array.Resize(ref rightSegmentLengths2, rightSegmentLengths2.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints2[rightLanePoints2.Length - 4],
            rightLanePoints2[rightLanePoints2.Length - 1]);
        rightSegmentLengths2[rightSegmentLengths2.Length - 1] = lastSegmentLegth;
        //rightSegmentLengths3
        Array.Resize(ref rightSegmentLengths3, rightSegmentLengths3.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints3[rightLanePoints3.Length - 4],
            rightLanePoints3[rightLanePoints3.Length - 1]);
        rightSegmentLengths3[rightSegmentLengths3.Length - 1] = lastSegmentLegth;

        // 10. Resize left segments
        //leftSegmentLengths1
        Array.Resize(ref leftSegmentLengths1, leftSegmentLengths1.Length + 1);
        for (int i = leftSegmentLengths1.Length -1; i > 0; i--)
        {
            leftSegmentLengths1[i] = leftSegmentLengths1[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints1[0], leftLanePoints1[3]);
        leftSegmentLengths1[0] = lastSegmentLegth;
        //leftSegmentlengths2
        Array.Resize(ref leftSegmentLengths2, leftSegmentLengths2.Length + 1);
        for (int i = leftSegmentLengths2.Length - 1; i > 0; i--)
        {
            leftSegmentLengths2[i] = leftSegmentLengths2[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints2[0], leftLanePoints2[3]);
        leftSegmentLengths2[0] = lastSegmentLegth;
        //leftSegmentLengths3
        Array.Resize(ref leftSegmentLengths3, leftSegmentLengths3.Length + 1);
        for (int i = leftSegmentLengths3.Length - 1; i > 0; i--)
        {
            leftSegmentLengths3[i] = leftSegmentLengths3[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints3[0], leftLanePoints3[3]);
        leftSegmentLengths3[0] = lastSegmentLegth;

        // 11. Update right modes
        //rightModes1
        Array.Resize(ref rightModes1, rightModes1.Length + 1);
        rightModes1[rightModes1.Length - 1] = rightModes1[rightModes1.Length - 2];
        //rightModes2
        Array.Resize(ref rightModes2, rightModes2.Length + 1);
        rightModes2[rightModes2.Length - 1] = rightModes2[rightModes2.Length - 2];
        //rightModes3
        Array.Resize(ref rightModes3, rightModes3.Length + 1);
        rightModes3[rightModes3.Length - 1] = rightModes3[rightModes3.Length - 2];

        // 12. Update left modes
        //leftModes1
        Array.Resize(ref leftModes1, leftModes1.Length + 1);
        for (int i = leftModes1.Length - 1; i > 0; i--)
        {
            leftModes1[i] = leftModes1[i - 1];
        }
        leftModes1[0] = leftModes1[1];
        //leftModes2
        Array.Resize(ref leftModes2, leftModes2.Length + 1);
        for (int i = leftModes2.Length - 1; i > 0; i--)
        {
            leftModes2[i] = leftModes2[i - 1];
        }
        leftModes2[0] = leftModes2[1];
        //leftModes1
        Array.Resize(ref leftModes3, leftModes3.Length + 1);
        for (int i = leftModes3.Length - 1; i > 0; i--)
        {
            leftModes3[i] = leftModes3[i - 1];
        }
        leftModes3[0] = leftModes3[1];
    }

    public BezierControlPointMode GetLaneMode(int lane, int index, bool rightLane)
    {
        BezierControlPointMode mode = BezierControlPointMode.Aligned;
        int modeIndex = (index + 1) / 3;
        if (rightLane)
        {
            switch(index)
            {
                case 0:
                    mode = rightModes1[modeIndex];
                    break;
                case 1:
                    mode = rightModes2[modeIndex];
                    break;
                case 2:
                    mode = rightModes3[modeIndex];
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 0:
                    mode = leftModes1[modeIndex];
                    break;
                case 1:
                    mode = leftModes2[modeIndex];
                    break;
                case 2:
                    mode = leftModes3[modeIndex];
                    break;
            }
        }
        return mode;
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        EnforceMode(index);
    }

    public void SetLaneMode(int lane, int index, BezierControlPointMode mode, bool rightLane)
    {
        int modeIndex = (index + 1) / 3;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    rightModes1[modeIndex] = mode;
                    break;
                case 1:
                    rightModes2[modeIndex] = mode;
                    break;
                case 2:
                    rightModes3[modeIndex] = mode;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    leftModes1[modeIndex] = mode;
                    break;
                case 1:
                    leftModes2[modeIndex] = mode;
                    break;
                case 2:
                    leftModes3[modeIndex] = mode;
                    break;
            }
        }
        EnforceLaneMode(lane, index, rightLane);
    }

    public void Reset()
    {
        initialized = false;
        lanesSet = false;
        nodesSet = false;
        busLaneLeft = false;
        busLeftStart = 0;
        busLeftEnd = 0;
        busLaneRight = false;
        busRightStart = 0;
        busRightEnd = 0;
        traffic = TrafficSize.Average;
        speedLimit = SpeedLimits.KMH_40;
        permittedLaneChanges = new bool[] { false, false, false, false, false, false, false, false };

        roadNetwork = null;
        roadParent = null;
        leftParent1 = null;
        leftParent2 = null;
        leftParent3 = null;
        rightParent1 = null;
        rightParent2 = null;
        rightParent3 = null;

        startNodes = new Nodes[] { null, null, null, null, null, null };
        endNodes = new Nodes[] { null, null, null, null, null, null };

        points = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f)
        };
        modes = new BezierControlPointMode[]
        {
            BezierControlPointMode.Aligned,
            BezierControlPointMode.Aligned
        };

        splineLength = Vector3.Distance(points[0], points[3]);
        segmentLengths = new float[] { splineLength }; //jos ei alusteta suoralla pitää muuttaa
        nodesOnSegment = new int[] { 0 };

        NodeCount = 0;
        LeftLaneCount = 0;
        RightLaneCount = 0;

        wayPointsLeft1 = new List<GameObject>();
        wayPointsLeft2 = new List<GameObject>();
        wayPointsLeft3 = new List<GameObject>();
        wayPointsRight1 = new List<GameObject>();
        wayPointsRight2 = new List<GameObject>();
        wayPointsRight3 = new List<GameObject>();

        leftLanePoints1 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftLanePoints2 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftLanePoints3 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints1 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints2 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints3 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftSegmentLengths1 = new float[] { 0 };
        leftSegmentLengths2 = new float[] { 0 };
        leftSegmentLengths3 = new float[] { 0 };
        rightSegmentLengths1 = new float[] { 0 };
        rightSegmentLengths2 = new float[] { 0 };
        rightSegmentLengths3 = new float[] { 0 };

        leftSplineLengths = new float[] {0f ,0f ,0f };
        rightSplineLengths = new float[] {0f ,0f ,0f };

        leftSpacings1 = new float[] { 0f, 0f };
        leftSpacings2 = new float[] { 0f, 0f };
        leftSpacings3 = new float[] { 0f, 0f };
        rightSpacings1 = new float[] { 0f, 0f };
        rightSpacings2 = new float[] { 0f, 0f };
        rightSpacings3 = new float[] { 0f, 0f };

        laneChangeStartIndex = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        laneChangeEndIndex = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };


        leftModes1 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        leftModes2 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        leftModes3 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        rightModes1 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        rightModes2 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
        rightModes3 = new BezierControlPointMode[]
        { BezierControlPointMode.Aligned, BezierControlPointMode.Aligned };
    }
}