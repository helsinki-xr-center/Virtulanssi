using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Intersection : MonoBehaviour
{
    [SerializeField]
    private Vector3 centerPoint;
    [SerializeField]
    private float frameWidth;
    [SerializeField]
    private float frameHeight;
    private const float defaultFrameMeasure = 5f;
    public bool framed;
    public bool allNodesSet;
    public GameObject roadNetwork;
    //for selecting /deselcting in and out nodes in set up
    public NodeInfo[] nodesInBox;
    public bool nodesInBoxSet = false;
    [SerializeField]
    private int infoIndex = -1;
    [SerializeField]
    public List<HelperLine> helperLines;
    [SerializeField]
    public List<ExistingLane> existingLanes;
    public int existingLaneIndex = 0;
    public bool existingLanesChecked = false;
    public SplineData[] createdSplines;
    public int splineIndex = -1;
    public bool splinesSet = false;

    public int inIndex = 0;
    public int outIndex = 0;
    // basic getters and setters
    public Vector3 CenterPoint
    {
        get
        {
            return centerPoint;
        }
        set
        {
            centerPoint = value;
        }
    }

    public float FrameWidth
    {
        get
        {
            return frameWidth;
        }
        set
        {
            frameWidth = value;
        }
    }

    public float FrameHeight
    {
        get
        {
            return frameHeight;
        }
        set
        {
            frameHeight = value;
        }
    }

    // nodes-in-box [NodeInfo] related
    public int GetInfoIndex
    {
        get
        {
            return infoIndex;
        }
    }
    
    public void SetInfoIndexToFirst()
    {
        if (nodesInBox == null || nodesInBox.Length == 0)
        {
            infoIndex = -1;
        }
        else
        {
            infoIndex = 0;
        }
    }

    public void SetInOutAll(NodeInOut inOut)
    {
        if (nodesInBox != null)
        {
            for (int i = 0; i < nodesInBox.Length; i++)
            {
                nodesInBox[i].inOut = inOut;
            }
        }
    }

    public void SetInOut(NodeInOut inOut)
    {
        if (nodesInBox == null || nodesInBox.Length != 0)
        {
            nodesInBox[infoIndex].inOut = inOut;
        }
    }

    public void MoveInfoIndex (int val)
    {
        if (nodesInBox == null || nodesInBox.Length == 0)
        {
            return;
        }
        int index = infoIndex + val;
        if (index < 0)
        {
            infoIndex = nodesInBox.Length - 1;
        }
        else if (index > nodesInBox.Length - 1)
        {
            infoIndex = 0;
        }
        else
        {
            infoIndex = index;
        }
    }

    public int GetInfoSize()
    {
        if (nodesInBox == null)
        {
            return 0;
        }
        else
        {
            return nodesInBox.Length;
        }
    }

    public int SelectNextAvailable()
    {
        if (infoIndex == nodesInBox.Length - 1)
        {
            bool found = false;
            for (int i = 0; i < nodesInBox.Length - 1; i++)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    found = true;
                    infoIndex = i;
                    break;
                }
                if (!found)
                {
                    infoIndex = -1;
                }
            }
        }
        else
        {
            bool found = false;
            int val = -1;
            for (int i = infoIndex + 1; i < nodesInBox.Length; i++)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    val = i;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                for (int i = 0; i <= infoIndex; i++)
                {
                    if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                    {
                        val = i;
                        break;
                    }
                }
            }
            infoIndex = val;
        }
        return infoIndex;
    }

    public int SelectPreviousAvailable()
    {
        if (infoIndex == 0)
        {
            bool found = false;
            for (int i = nodesInBox.Length - 1; i >= 0; i--)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    found = true;
                    infoIndex = i;
                    break;
                }
            }
            if (!found)
            {
                infoIndex = -1;
            }
        }
        else
        {
            bool found = false;
            int val = -1;
            for (int i = infoIndex - 1; i >= 0; i--)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    val = i;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                for (int i = nodesInBox.Length - 1; i >= infoIndex; i--)
                {
                    if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                    {
                        val = i;
                        break;
                    }
                }
            }
            infoIndex = val;
        }
        return infoIndex;
    }

    public void SelectAdjacents()
    {
        NodeInOut inOut = nodesInBox[infoIndex].inOut;
        Nodes n = nodesInBox[infoIndex].node;
        if (n.ParallelRight)
        {
            Nodes r1 = n.ParallelRight;
            int index = FindNodeInfoIndex(r1);
            if (index > -1)
            {
                nodesInBox[index].inOut = inOut;
                if (r1.ParallelRight)
                {
                    Nodes r2 = r1.ParallelRight;
                    index = FindNodeInfoIndex(r2);
                    if (index > -1)
                    {
                        nodesInBox[index].inOut = inOut;
                    }
                }
            }
        }
        if (n.ParallelLeft)
        {
            Nodes l1 = n.ParallelLeft;
            int index = FindNodeInfoIndex(l1);
            if (index > -1)
            {
                nodesInBox[index].inOut = inOut;
                if (l1.ParallelLeft)
                {
                    Nodes l2 = l1.ParallelLeft;
                    index = FindNodeInfoIndex(l2);
                    if (index > -1)
                    {
                        nodesInBox[index].inOut = inOut;
                    }
                }
            }
        }
    }

    private int FindNodeInfoIndex(Nodes node)
    {
        int ind = -1;
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            if (nodesInBox[i].node == node)
            {
                ind = i;
                break;
            }
        }
        return ind;
    }

    public Nodes GetSelectedNodeInfo(out NodeInOut inOut)
    {
        if (infoIndex == -1)
        {
            inOut = NodeInOut.NotUsed;
            return null;
        }
        else
        {
            inOut = nodesInBox[infoIndex].inOut;
            return nodesInBox[infoIndex].node;
        }
    }

    public Nodes GetInNodeOfCurrentIndex()
    {
        Nodes n = null;
        int count = 0;
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            if (nodesInBox[i].inOut == NodeInOut.InNode)
            {
                if (count == inIndex)
                {
                    n = nodesInBox[i].node;
                    break;
                }
                else
                {
                    count++;
                }
            }
        }
        return n;
    }

    public Nodes GetOutNodeOfCurrentIndex()
    {
        Nodes n = null;
        int count = 0;
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            if (nodesInBox[i].inOut == NodeInOut.OutNode)
            {
                if (count == outIndex)
                {
                    n = nodesInBox[i].node;
                    break;
                }
                else
                {
                    count++;
                }
            }
        }
        return n;
    }
    // Helperlines
    public void RemoveHelperLines()
    {
        helperLines = new List<HelperLine>();
        inIndex = 0;
        outIndex = 0;
    }
    // Helperlines AND nodes-in-box
    public int GetInOutPositions(out List<Vector3> ins, out List<Vector3> outs)
    {
        ins = new List<Vector3>();
        outs = new List<Vector3>();
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            NodeInfo n = nodesInBox[i];
            if (n.inOut== NodeInOut.InNode)
            {
                ins.Add(n.node.transform.position);
            }
            else if (n.inOut == NodeInOut.OutNode)
            {
                outs.Add(n.node.transform.position);
            }
        }
        if (helperLines == null)
        {
            helperLines = new List<HelperLine>();
        }
        for (int i = 0; i < helperLines.Count; i++)
        {
            HelperLine h = helperLines[i];
            Vector3 p0 = h.startPoint;
            Vector3 dir = h.direction;
            float lenght = h.lenght;
            for (int j = 0; j < h.nodePoints.Count; j++)
            {
                Vector3 pnt = p0 + h.nodePoints[j] * lenght * dir;
                if (h.inOut[j] == NodeInOut.InNode)
                {
                    ins.Add(pnt);
                }
                else
                {
                    outs.Add(pnt);
                }
            }
        }
        return ins.Count + outs.Count;
    }
    
    public int GetHelperLineIndex(Vector3 position)
    {
        int index = 0;
        for (int i = 0; i < helperLines.Count; i++)
        {
            bool found = false;
            HelperLine h = helperLines[i];
            Vector3 p0 = h.startPoint;
            Vector3 dir = h.direction;
            float lenght = h.lenght;
            for (int j = 0; j < h.nodePoints.Count; j++)
            {
                Vector3 pnt = p0 + h.nodePoints[j] * lenght * dir;
                if (pnt == position)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                index = i;
                break;
            }
        }
        return index;
    }
    // Existing lane
    public ExistingLane GetCurrentExistingLane()
    {
        if (existingLanes==null || existingLanes.Count == 0)
        {
            return null;
        }
        return existingLanes[existingLaneIndex];
    }

    public void MoveExistingLaneIndex(int val)
    {
        int v = existingLaneIndex + val;
        if (v < 0)
        {
            existingLaneIndex = existingLanes.Count - 1;
        }
        else if (v > existingLanes.Count - 1)
        {
            existingLaneIndex = 0;
        }
        else
        {
            existingLaneIndex = v;
        }
    }

    public void SetCurrentExistingLane(ExistingLane ex)
    {
        existingLanes[existingLaneIndex] = ex;
    }

    public int GetUnconfirmedExistingLaneCount()
    {
        int v = 0;
        for (int i=0; i < existingLanes.Count; i++)
        {
            if (!existingLanes[i].confirmed)
            {
                v++;
            }
        }
        return v;
    }
    // Spline data
    public void SetSplinePoint(int splineIndex, int pointIndex, Vector3 point)
    {
        SplineData sp = createdSplines[splineIndex];
        if (pointIndex % 3 == 0)
        {
            Vector3 delta = point - sp.points[pointIndex];
            if (pointIndex > 0)
            {
                sp.points[pointIndex - 1] += delta;
            }
            if (pointIndex + 1 < sp.points.Length)
            {
                sp.points[pointIndex + 1] += delta;
            }
        }
        //***********
        sp.points[pointIndex] = point;
        EnforceMode(splineIndex, pointIndex);
    }

    public void MoveSplineIndex(int val)
    {
        int v = splineIndex + val;
        if (v < 0 && createdSplines.Length > 0)
        {
            splineIndex = createdSplines.Length - 1;
        }
        else if (v > createdSplines.Length - 1)
        {
            splineIndex = 0;
        }
        else
        {
            splineIndex = v;
        }
    }

    public void AddSegmentToCurrentSpline()
    {
        SplineData sd = createdSplines[splineIndex];
        Vector3 point = sd.points[sd.points.Length - 1];
        float length;
        if (frameHeight < frameWidth)
        {
            length = frameHeight * 0.1f;
        }
        else
        {
            length = frameWidth * 0.1f;
        }
        Vector3 dir = GetSegmentedDirection((sd.points.Length - 1) % 3, 1f);
        Array.Resize(ref sd.points, sd.points.Length + 3);
        point += length * dir;
        sd.points[sd.points.Length - 3] = point;
        point += length * dir;
        sd.points[sd.points.Length - 2] = point;
        point += length * dir;
        sd.points[sd.points.Length - 1] = point;

        Array.Resize(ref sd.modes, sd.modes.Length + 1);
        sd.modes[sd.modes.Length - 1] = sd.modes[sd.modes.Length - 2];
        EnforceMode(splineIndex, sd.points.Length - 4);
    }

    public void ConnectSplineToOutNode()
    {
        Nodes n = GetOutNodeOfCurrentIndex();

        SplineData sd = createdSplines[splineIndex];
        if (n != null)
        {
            sd.endNode = n;
            sd.points[sd.points.Length - 1] = n.transform.position;
        }
        else
        {
            List<Vector3> ins, outs;
            GetInOutPositions(out ins, out outs);
            sd.points[sd.points.Length - 1] = outs[outIndex];
        }
        sd.endPointSet = true;
    }

    public void SetSegmentArrays()
    {
        for (int i = 0; i < createdSplines.Length; i++)
        {
            SplineData sd = createdSplines[i];
            int segs = (sd.points.Length - 1) / 3;
            sd.segmentNodes = new int[segs];
            for (int j = 0; j < segs; j++)
            {
                sd.segmentNodes[j] = 0;
            }
        }
    }

    public List<Vector3> GetNodePositionInBetweenEndPoints (int splineInd)
    {
        List<Vector3> pnts = new List<Vector3>();
        SplineData sd = createdSplines[splineInd];
        for (int seg = 0; seg < sd.segmentNodes.Length; seg++)
        {
            int nodesOnSeg = sd.segmentNodes[seg];
            for (int pnt = 1; pnt <= nodesOnSeg; pnt++)
            {
                Vector3 pos = GetSegmentedPoint(splineInd, seg, (float)pnt / nodesOnSeg);
                pnts.Add(pos);
            }
        }
        // Remove the last one
        pnts.RemoveAt(pnts.Count - 1);
        return pnts;
    }

    public bool GetSegmentNodePositions(out List<Vector3> current, out List<Vector3> other)
    {
        current = new List<Vector3>();
        other = new List<Vector3>();
        for (int i = 0; i < createdSplines.Length; i++)
        {
            SplineData sd = createdSplines[i];
            Vector3 pos = sd.points[0];
            if (i == splineIndex)
            {
                current.Add(pos);
            }
            else
            {
                other.Add(pos);
            }
            for (int seg = 0; seg < sd.segmentNodes.Length; seg++)
            {
                int nodesOnSeg = sd.segmentNodes[seg];
                for (int pnt = 1; pnt <= nodesOnSeg; pnt++)
                {
                    pos = GetSegmentedPoint(i, seg, (float)pnt / nodesOnSeg);
                    if (i == splineIndex)
                    {
                        current.Add(pos);
                    }
                    else
                    {
                        other.Add(pos);
                    }
                }
            }
        }
        return true;
    }

    public bool AllSplineEndPointsConnected()
    {
        bool connected = true;
        if (createdSplines == null)
        {
            return connected;
        }
        for (int i = 0; i < createdSplines.Length; i++)
        {
            SplineData sd = createdSplines[i];
            if (!sd.endPointSet)
            {
                connected = false;
                break;
            }
        }
        return connected;
    }

    public bool NodesOnAllSegments()
    {
        bool isTrue = true;
        if (createdSplines == null)
        {
            return isTrue;
        }
        for (int i = 0; i < createdSplines.Length; i++)
        {
            SplineData sd = createdSplines[i];
            bool b = true;
            for (int seg = 0; seg < sd.segmentNodes.Length; seg++)
            {
                if (sd.segmentNodes[seg]==0)
                {
                    b = false;
                    break;
                }
            }
            if (b == false)
            {
                isTrue = false;
                break;
            }
        }
        return isTrue;
    }

    private Vector3 GetSegmentedPoint(int splineInd, int segment, float fraction)
    {
        SplineData sd = createdSplines[splineInd];
        int i = segment * 3;
        return Bezier.GetPoint(
            sd.points[i], sd.points[i + 1], sd.points[i + 2], sd.points[i + 3], fraction);
    }

    private Vector3 GetSegmentedDirection(int segment, float fraq)
    {

        return GetSegmentedVelocity(segment, fraq).normalized;
    }

    private Vector3 GetSegmentedVelocity(int segment, float fraq)
    {
        SplineData sd = createdSplines[splineIndex];
        int i = segment * 3;
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(sd.points[i], sd.points[i + 1],
            sd.points[i + 2], sd.points[i + 3], fraq))
            - transform.position;
    }

    public void RemoveCurrentSpline()
    {
        if (createdSplines == null || createdSplines.Length == 0)
        {
            return;
        }
        for (int i = splineIndex; i < createdSplines.Length - 1; i++)
        {
            createdSplines[i] = createdSplines[i + 1];
        }
        Array.Resize(ref createdSplines, createdSplines.Length - 1);
        splineIndex--;
        if (splineIndex < 0 && createdSplines.Length > 0)
        {
            splineIndex = createdSplines.Length - 1;
        }
    }

    private void EnforceMode(int splineIndex, int pointIndex)
    {
        SplineData sp = createdSplines[splineIndex];
        int modeIndex = (pointIndex + 1) / 3;
        BezierControlPointMode mode = sp.modes[modeIndex];
        // We don't enforce if we are at end points or the current mode is set to 'FREE'.
        if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == sp.modes.Length - 1)
        {
            return;
        }
        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (pointIndex <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = sp.points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= sp.points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= sp.points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = sp.points.Length - 2;
            }
        }

        Vector3 middle = sp.points[middleIndex];
        Vector3 enforcedTangent = middle - sp.points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, sp.points[enforcedIndex]);
        }
        sp.points[enforcedIndex] = middle + enforcedTangent;
    }


    public void Reset()
    {
        CenterPoint = transform.position;
        FrameWidth = defaultFrameMeasure;
        FrameHeight = defaultFrameMeasure;
        framed = false;
    }
}

[Serializable]
public class NodeInfo
{
    public Nodes node;
    public NodeInOut inOut;
}

[Serializable]
public class HelperLine
{
    public Vector3 startPoint;
    public Vector3 direction;
    public float lenght;
    public List<float> nodePoints;
    public List<NodeInOut> inOut;
    public TrafficSize traffic = TrafficSize.Average;
    public SpeedLimits speedLimit = SpeedLimits.KMH_30;
    public DriverYield laneYield;
}

[Serializable]
public class ExistingLane
{
    public List<Nodes> nodes;
    public int inNodeIndex;
    public int outNodeIndex;
    public DriverYield laneYield;
    public IntersectionDirection turnDirection;
    public TrafficSize traffic = TrafficSize.Average;
    public SpeedLimits speedLimit = SpeedLimits.KMH_30;
    public bool confirmed;
}

[Serializable]
public class SplineData
{
    public Vector3[] points;
    public BezierControlPointMode[] modes;
    public Nodes startNode;
    public Nodes endNode;
    public bool endPointSet;
    public IntersectionDirection turnDirection;
    public int[] segmentNodes;
}