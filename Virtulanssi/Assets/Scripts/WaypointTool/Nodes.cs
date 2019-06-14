using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Nodes : MonoBehaviour
{
    [SerializeField]
    private Nodes[] inNodes;
    [SerializeField]
    private Nodes[] outNodes;
    [SerializeField]
    private Nodes parallelLeft;
    [SerializeField]
    private Nodes parallelRight;
    [SerializeField]
    private Nodes laneChangeLeft;
    [SerializeField]
    private Nodes laneChangeRight;
    [SerializeField]
    bool isConnectNode;
    [SerializeField]
    bool isBusLane;
    [SerializeField]
    bool isInIntersection;
    [SerializeField]
    IntersectionDirection turnDirection;
    [SerializeField]
    Lane parentLane;

    public TrafficSize Traffic
    {
        get
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            return parentLane.Traffic;
        }
        set
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            parentLane.Traffic = value;
        }
    }

    public IntersectionDirection TurnDirection
    {
        get
        {
            return turnDirection;
        }
        set
        {
            turnDirection = value;
        }
    }

    public bool ConnectNode
    {
        get
        {
            return isConnectNode;
        }
        set
        {
            isConnectNode = value;
        }
    }

    public bool BusLane
    {
        get
        {
            return isBusLane;
        }
        set
        {
            isBusLane = value;

        }
    }

    public bool IsInIntersection
    {
        get
        {
            return isInIntersection;
        }
        set
        {
            isInIntersection = value;
        }
    }

    public Nodes ParallelLeft
    {
        get
        {
            return parallelLeft;
        }
        set
        {
            parallelLeft = value;
        }
    }

    public Nodes ParallelRight
    {
        get
        {
            return parallelRight;
        }
        set
        {
            parallelRight = value;
        }
    }

    public Nodes LaneChangeLeft
    {
        get
        {
            return laneChangeLeft;
        }
        set
        {
            laneChangeLeft = value;
        }
    }

    public Nodes LaneChangeRight
    {
        get
        {
            return laneChangeRight;
        }
        set
        {
            laneChangeRight = value;
        }
    }

    public DriverYield LaneYield
    {
        get
        {
            if (parentLane==null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            return parentLane.LaneYield;
        }
        set
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            parentLane.LaneYield = value;
        }
    }

    public SpeedLimits SpeedLimit
    {
        get
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            return parentLane.SpeedLimit;
        }
        set
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            parentLane.SpeedLimit = value;
        }
    }

    // This is for editor use. User may have manually removed some nodes that are linked to
    // this node. InNodes and OutNodes arrays must be checked to remove nulls
    public void CheckNullNodes()
    {
        bool inNodesChecked = false;
        bool outNodesChecked = false;
        while(true)
        {
            bool isNulls = false;
            int index = 0;
            if (!inNodesChecked)
            {
                for (int i = 0; i < inNodes.Length; i++)
                {
                    if (inNodes[i] == null)
                    {
                        index = i;
                        isNulls = true;
                    }
                }
            }
            if (isNulls == false)
            {
                inNodesChecked = true;
            }
            else
            {
                // move values in array and resize
                for (int i = index; i < inNodes.Length - 1; i++)
                {
                    if (inNodes[i + 1] == null)
                    {
                        inNodes[i] = null;
                    }
                    else
                    {
                        inNodes[i] = inNodes[i + 1];
                    }
                }
                Array.Resize(ref inNodes, inNodes.Length - 1);
            }
            isNulls = false;
            if (!outNodesChecked)
            {
                for (int i = 0; i < outNodes.Length; i++)
                {
                    if (outNodes[i] == null)
                    {
                        index = i;
                        isNulls = true;
                    }
                }
            }
            if (isNulls == false)
            {
                outNodesChecked = true;
            }
            else
            {
                // move values in array and resize
                for (int i = index; i < outNodes.Length - 1; i++)
                {
                    if (outNodes[i + 1] == null)
                    {
                        outNodes[i] = null;
                    }
                    else
                    {
                        outNodes[i] = outNodes[i + 1];
                    }
                }
                Array.Resize(ref outNodes, outNodes.Length - 1);
            }
            if (inNodesChecked && outNodesChecked)
            {
                break;
            }
        }
    }

    public int InNodesLength
    {
        get
        {
            return inNodes.Length;
        }
    }

    public Nodes[] InNodes
    {
        get
        {
            return inNodes;
        }
    }

    public int OutNodesLength
    {
        get
        {
            return outNodes.Length;
        }
    }

    public Nodes[] OutNodes
    {
        get
        {
            return outNodes;
        }
    }

    public void AddInNode(Nodes n)
    {
        if (!IsInInNodes(n))
        {
            int oldSize = inNodes.Length;
            Array.Resize(ref inNodes, oldSize + 1);
            inNodes[oldSize] = n;
            if (oldSize == 1)
            {
                isConnectNode = true;
            }
        }
    }

    public void AddOutNode(Nodes n)
    {
        if (!IsInOutNodes(n))
        {
            int oldSize = outNodes.Length;
            Array.Resize(ref outNodes, oldSize + 1);
            outNodes[oldSize] = n;
            if (oldSize == 1)
            {
                isConnectNode = true;

            }
        }
    }

    public void RemoveInNode(Nodes n)
    {
        int index = -1;
        for (int i = 0; i < inNodes.Length; i++)
        {
            if (inNodes[i] == n)
            {
                index = i;
                break;
            }
        }
        if (index >= 0 && index < inNodes.Length - 1)
        {
            for (int i = index + 1; i < inNodes.Length; i++)
            {
                inNodes[i - 1] = inNodes[i];
            }
        }
        if (index >= 0)
        {
            Array.Resize(ref inNodes, inNodes.Length - 1);
        }
    }

    public void RemoveOutNode(Nodes n)
    {
        int index = -1;
        for (int i = 0; i < outNodes.Length; i++)
        {
            if (outNodes[i] == n)
            {
                index = i;
                break;
            }
        }
        if (index >= 0 && index < outNodes.Length - 1)
        {
            for (int i = index + 1; i < outNodes.Length; i++)
            {
                outNodes[i - 1] = outNodes[i];
            }
        }
        if (index >= 0)
        {
            Array.Resize(ref outNodes, outNodes.Length - 1);
        }
    }

    private bool IsInOutNodes(Nodes n)
    {
        bool isNext = false;
        for (int i = 0; i < outNodes.Length; i++)
        {
            if (n == outNodes[i])
            {
                isNext = true;
                break;
            }
        }
        return isNext;

    }

    private bool IsInInNodes(Nodes n)
    {
        bool isNext = false;
        for (int i = 0; i < inNodes.Length; i++)
        {
            if (n == inNodes[i])
            {
                isNext = true;
                break;
            }
        }
        return isNext;

    }

    public bool GetDirectionOut(out Vector3 direction)
    {
        if (outNodes.Length != 1)
        {
            direction = Vector3.forward;
            return false;
        }
        else
        {
            direction = (outNodes[0].transform.position - transform.position).normalized;
            return true;
        }
    }

    public bool GetDirectionIn(out Vector3 direction)
    {
        if (inNodes.Length != 1)
        {
            direction = Vector3.forward;
            return false;
        }
        else
        {
            direction = (transform.position - inNodes[0].transform.position).normalized;
            return true;
        }
    }

    public Lane ParentLane
    {
        get
        {
            return parentLane;
        }
        set
        {
            parentLane = value;
        }
    }

    private void Reset()
    {
        parentLane = null;
        inNodes = new Nodes[0];
        outNodes = new Nodes[0];
        parallelLeft = null;
        parallelRight = null;
        laneChangeLeft = null;
        laneChangeRight = null;
        isConnectNode = false;
        isBusLane = false;
        isInIntersection = false;
        turnDirection = IntersectionDirection.Straight;
    }
}
