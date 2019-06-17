using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

[CustomEditor(typeof(Intersection))]
public class IntersectionInspector : Editor
{
    private Intersection intersection;
    private GameObject roadNetwork;
    // naming
    private bool nameAutoChecked = true;
    private bool nameIsValid = false;
    private string intersectionName = "";
    private string renameInfo = "";

    // framing
    private Vector3[] framingHorizontal;
    private Vector3[] framingVertical;
    private float step = 1f;

    //positioning line;
    private Vector3 linePos;
    private Vector3[] linePoints;
    private float lineLength;
    private Vector3 lineDir;
    private bool showLine = false;
    private float lineYAngle = 0f;
    private float lineAngle = 5f;
    private int nodesOnLine = 0;
    private float[] nodePlaces;
    private float[] tempPlaces;
    private NodeInOut[] lineNodesInOut;
    private DriverYield lineYield = DriverYield.Normal;
    private TrafficSize lineTraffic = TrafficSize.Average;
    private SpeedLimits lineSpeedLimit = SpeedLimits.KMH_30;

    private List<Vector3> pointsToDraw;
    private List<Color> pointsToDrawColors;
    private List<Vector3> existingLaneLinesToDraw;
    private DriverYield exYield;
    private IntersectionDirection exTurn;
    private TrafficSize exTraffic;
    private SpeedLimits exSpeedLimit;
    private bool exConfirmed;

    private bool addingNodes = false;
    private bool confirming = false;

    public List<Vector3> inPositions;
    public List<Vector3> outPositions;
    public int inOutCount;

    Transform handleTransform;
    Quaternion handleRotation;
    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;
    private Vector2Int selectedIndex;

    private List<Vector3> currentSplineNodes;
    private List<Vector3> otherSplineNodes;
    private bool nodesFetched = false;
    private bool allEndNodesConnected = true;
    private bool allSegmentsHaveNodes = true;


    //*************************** INSPECTOR START

    public override void OnInspectorGUI()
    {
        Undo.RecordObject(intersection, "changed");
        NameMenu();
        if (intersection.splinesSet)
        {
            NodeSetupMenu();
            //base.OnInspectorGUI();
            return;
        }
        if (intersection.allNodesSet)
        {
            if (intersection.existingLanesChecked)
            {
                SplineSetupMenu();
            }
            else
            {
                ConfirmExistingLanesMenu();
            }
        }
        else
        {
            if (!intersection.framed)
            {
                FramingMenu();
            }
            else
            {
                SetupMenu();
            }
        }
        //base.OnInspectorGUI();
    }

    private void NameMenu()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Road network:", EditorStyles.boldLabel);
        if (roadNetwork == null)
        {
            EditorGUILayout.LabelField("Not selected");
        }
        else
        {
            EditorGUILayout.LabelField(roadNetwork.name);
        }
        EditorGUILayout.EndHorizontal();
        if (roadNetwork == null)
        {
            if (intersection.roadNetwork != null)
            {
                roadNetwork = intersection.roadNetwork;
                nameAutoChecked = false;
            }
            else
            {
                RoadNetwork[] networks = GameObject.FindObjectsOfType<RoadNetwork>();
                if (networks == null || networks.Length == 0)
                {
                    GameObject g = new GameObject();
                    g.AddComponent<RoadNetwork>();
                    g.name = "NodeNetwork";
                    roadNetwork = g;
                    intersection.roadNetwork = g;
                }
                else if (networks.Length == 1)
                {
                    roadNetwork = networks[0].gameObject;
                    intersection.roadNetwork = roadNetwork;
                    nameAutoChecked = false;
                }
                else
                {
                    EditorGUILayout.LabelField("Select parent network", EditorStyles.boldLabel);
                    for (int i = 0; i < networks.Length; i++)
                    {
                        bool selected = false;
                        selected = EditorGUILayout.Toggle(networks[i].gameObject.name, selected);
                        if (selected)
                        {
                            roadNetwork = networks[i].gameObject;
                            intersection.roadNetwork = networks[i].gameObject;
                            nameAutoChecked = false;
                        }
                    }
                }
            }
        }

        EditorGUILayout.Separator();

        if (roadNetwork != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Intersection name:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(intersection.gameObject.name);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            ItalicLabel("As a suggestion (for easier managing) intersection names should have a prefix, " +
                "for example. 'int_'.");
            if (!nameAutoChecked)
            {
                nameAutoChecked = true;
                nameIsValid = CheckName(intersection.gameObject.name);
                if (nameIsValid)
                {
                    intersectionName = intersection.gameObject.name;
                }
                else
                {
                    intersectionName = "";
                }
            }
            if (nameIsValid)
            {
                ItalicLabel("Name is valid");
            }
            else
            {
                WarningLabel("Invalid name. Name already exists.");
            }
            intersectionName = GUILayout.TextField(intersectionName);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enter name");
            if (GUILayout.Button("Rename"))
            {
                bool valid = CheckName(intersectionName);
                if (!valid)
                {
                    renameInfo = "New name was not valid.";
                }
                else
                {
                    renameInfo = "Name changed to '" + intersectionName + "'.";
                    intersection.gameObject.name = intersectionName;
                    nameIsValid = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            ItalicLabel(renameInfo);

            EditorGUILayout.Separator();
        }
        DrawEditorLine();
    }

    private void FramingMenu()
    {
        EditorGUILayout.LabelField("Framing", EditorStyles.boldLabel);
        step = EditorGUILayout.FloatField("Step:", step);
        EditorGUILayout.Separator();
        // centerpoint
        EditorGUILayout.LabelField("Adjust centerpoint", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Up"))
        {
            intersection.CenterPoint += new Vector3(0f, 0f, step);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Down"))
        {
            intersection.CenterPoint += new Vector3(0f, 0f, -step);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Left"))
        {
            intersection.CenterPoint += new Vector3(-step, 0f, 0f);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Right"))
        {
            intersection.CenterPoint += new Vector3(step, 0f, 0f);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        // bounding box
        EditorGUILayout.LabelField("Adjust bounding box", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Width+"))
        {
            intersection.FrameWidth += step;
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Width-"))
        {
            float width = intersection.FrameWidth - step;
            if (width > 0f)
            {
                intersection.FrameWidth = width;
            }
            else
            {
                intersection.FrameWidth = 0.1f;
            }
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Height+"))
        {
            intersection.FrameHeight += step;
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Height-"))
        {
            float height = intersection.FrameHeight - step;
            if (height > 0f)
            {
                intersection.FrameHeight = height;
            }
            else
            {
                intersection.FrameHeight = 0f;
            }
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Framing done"))
        {
            intersection.framed = true;
            UpdateNodesInBox();
        }
    }

    private void SetupMenu()
    {
        if (GUILayout.Button("Back to framing"))
        {
            intersection.framed = false;
        }
        DrawEditorLine();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Setup Menu", EditorStyles.boldLabel);

        if (intersection.GetInfoSize() > 0)
        {
            if (!addingNodes)
            {
                if (GUILayout.Button("Open node options"))
                {
                    addingNodes = true;
                    intersection.SetInfoIndexToFirst();
                    SceneView.RepaintAll();
                }
            }
            else
            {
                if (GUILayout.Button("Hide node options"))
                {
                    addingNodes = false;
                }
            }
        }
        if (addingNodes)
        {
            EntryNodesSetupMenu();
        }
        EditorGUILayout.Separator();
        EntryUsingGuideLine();
        EditorGUILayout.Separator();
        DrawEditorLine();
        if (!confirming)
        {
            if (GUILayout.Button("Nodes set, start drawing lanes"))
            {
                confirming = true;
            }
        }
        else
        {
            EditorGUILayout.LabelField("Are you sure?", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
                intersection.allNodesSet = true;
                GenerateExistingLanes();
                if (intersection.existingLanesChecked)
                {
                    intersection.inIndex = 0;
                    intersection.outIndex = 0;
                }
                else
                {
                    intersection.existingLaneIndex = 0;
                    SetCurrentExistingLaneValuesToInspector();
                }
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("No"))
            {
                confirming = false;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void EntryNodesSetupMenu()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Select nodes", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        if (intersection.GetInfoSize() == 0)
        {
            ItalicLabel("There are no nodes in the selected area.");
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous"))
            {
                intersection.MoveInfoIndex(-1);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Next"))
            {
                intersection.MoveInfoIndex(1);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            Nodes n = intersection.GetSelectedNodeInfo(out NodeInOut inOut);
            if (inOut == NodeInOut.NotUsed)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set as in-node"))
                {
                    intersection.SetInOut(NodeInOut.InNode);
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Set as out-node"))
                {
                    intersection.SetInOut(NodeInOut.OutNode);
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (n.ParallelLeft || n.ParallelRight)
                {
                    if (GUILayout.Button("Select adjacents also"))
                    {
                        intersection.SelectAdjacents();
                        SceneView.RepaintAll();
                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Deselect"))
                {
                    intersection.SetInOut(NodeInOut.NotUsed);
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Deselect this and adjacents"))
                {
                    intersection.SetInOut(NodeInOut.NotUsed);
                    intersection.SelectAdjacents();
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("Deselect all"))
            {
                intersection.SetInOutAll(NodeInOut.NotUsed);
                SceneView.RepaintAll();
            }
        }
    }

    private void EntryUsingGuideLine()
    {
        DrawEditorLine();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Create new entry points", EditorStyles.boldLabel);
        if (showLine == false)
        {
            if (GUILayout.Button("Create new"))
            {
                linePos = intersection.CenterPoint;
                lineLength = intersection.FrameHeight * 0.5f;
                Vector3 p0 = linePos + new Vector3(0f, 0f, -lineLength / 2f);
                Vector3 p1 = linePos + new Vector3(0f, 0f, lineLength / 2f);
                linePoints = new Vector3[] { p0, p1 };
                lineDir = (p1 - p0).normalized;
                showLine = true;
                nodesOnLine = 0;
                SceneView.RepaintAll();
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("Discard created points"))
            {
                intersection.RemoveHelperLines();
                UpdatePointsToDraw();
                SceneView.RepaintAll();
            }
        }
        else
        {
            LinePlacementMenu();
            if (GUILayout.Button("Cancel"))
            {
                showLine = false;
                SceneView.RepaintAll();
            }
        }
    }

    private void ConfirmExistingLanesMenu()
    {
        EditorGUILayout.LabelField("Existing lanes", EditorStyles.boldLabel);
        ItalicLabel("Set and confirm values");
        int unconfirmed = intersection.GetUnconfirmedExistingLaneCount();
        if (unconfirmed == 0)
        {
            ItalicLabel("All lanes confirmed.");
        }
        else
        {
            WarningLabel("" + unconfirmed + " lanes left to confirm.");
        }
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            intersection.MoveExistingLaneIndex(-1);
            SetCurrentExistingLaneValuesToInspector();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            intersection.MoveExistingLaneIndex(1);
            SetCurrentExistingLaneValuesToInspector();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        ExistingLane ex = intersection.GetCurrentExistingLane();
        EditorGUILayout.LabelField("Existing lane " + intersection.existingLaneIndex + ":",
            EditorStyles.boldLabel);
        if (ex.confirmed)
        {
            ItalicLabel("Confirmed.");
        }
        else
        {
            WarningLabel("Not confirmed");
        }

        exYield = (DriverYield)EditorGUILayout.EnumPopup("Lane yield", exYield);
        if (exYield != ex.laneYield)
        {
            ex.laneYield = exYield;
            intersection.SetCurrentExistingLane(ex);
        }

        exTurn = (IntersectionDirection)EditorGUILayout.EnumPopup("Turn direction", exTurn);
        if (exTurn != ex.turnDirection)
        {
            ex.turnDirection = exTurn;
            intersection.SetCurrentExistingLane(ex);
        }
        exSpeedLimit = (SpeedLimits)EditorGUILayout.EnumPopup("Speed limit", exSpeedLimit);
        if (exSpeedLimit != ex.speedLimit)
        {
            ex.speedLimit = exSpeedLimit;
            intersection.SetCurrentExistingLane(ex);
        }
        exTraffic = (TrafficSize)EditorGUILayout.EnumPopup("Traffic", exTraffic);
        if (exTraffic != ex.traffic)
        {
            ex.traffic = exTraffic;
            intersection.SetCurrentExistingLane(ex);
        }
        if (!ex.confirmed)
        {
            if (GUILayout.Button("Confirm lane"))
            {
                ex.confirmed = true;
                intersection.SetCurrentExistingLane(ex);
            }
        }
        else
        {
            if (GUILayout.Button("Undo confirmation"))
            {
                ex.confirmed = false;
                intersection.SetCurrentExistingLane(ex);
            }
        }
        if (unconfirmed == 0)
        {
            EditorGUILayout.Separator();
            if (GUILayout.Button("Done"))
            {
                intersection.existingLanesChecked = true;
            }
        }
    }

    private void SplineSetupMenu()
    {
        //in nodes
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("In-node selector", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current in-node: " + intersection.inIndex);
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            int val = intersection.inIndex - 1;
            if (val < 0)
            {
                intersection.inIndex = inPositions.Count - 1;
            }
            else
            {
                intersection.inIndex--;
            }
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            int val = intersection.inIndex + 1;
            if (val > inPositions.Count -1)
            {
                intersection.inIndex = 0;
            }
            else
            {
                intersection.inIndex = val;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        //out nodes
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Out-node selector", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current out-node: " + intersection.outIndex);
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            int val = intersection.outIndex - 1;
            if (val < 0)
            {
                intersection.outIndex = outPositions.Count - 1;
            }
            else
            {
                intersection.outIndex--;
            }
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            int val = intersection.outIndex + 1;
            if (val > outPositions.Count - 1)
            {
                intersection.outIndex = 0;
            }
            else
            {
                intersection.outIndex = val;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        DrawEditorLine();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Add spline", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("In " + intersection.inIndex);
        if (GUILayout.Button("Add spline"))
        {
            CreateBezier();
            SceneView.RepaintAll();
        }
        DrawEditorLine();
        SplineEditMenu();
    }

    private void LinePlacementMenu()
    {
        EditorGUILayout.LabelField("Traffic settings", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        lineTraffic = (TrafficSize)EditorGUILayout.EnumPopup("Traffic", lineTraffic);
        lineSpeedLimit = (SpeedLimits)EditorGUILayout.EnumPopup("Speed limit", lineSpeedLimit);
        lineYield = (DriverYield)EditorGUILayout.EnumPopup("Lane yield", lineYield);
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        step = EditorGUILayout.FloatField("Step", step);
        EditorGUILayout.LabelField("Adjust position", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Up"))
        {
            linePos.z += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Down"))
        {
            linePos.z -= step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Left"))
        {
            linePos.x -= step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Right"))
        {
            linePos.x += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Adjust size", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Line size -"))
        {
            if (lineLength - step > 1f)
            {
                lineLength -= step;
            }
            else
            {
                lineLength = 1f;
            }
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Line size +"))
        {
            lineLength += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Adjust angle", EditorStyles.boldLabel);
        float angle = lineAngle;
        angle = EditorGUILayout.FloatField("Angle", angle);
        {
            if (angle != lineAngle)
            {
                lineAngle = angle % 360f;
            }
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-"))
        {
            lineYAngle = (360f + lineYAngle - lineAngle) % 360;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("+"))
        {
            lineYAngle = (360f + lineYAngle + lineAngle) % 360;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        int nodes = nodesOnLine;
        nodes = EditorGUILayout.IntField("Nodes (" + nodesOnLine + ")", nodesOnLine);
        if (nodes != nodesOnLine)
        {
            if (nodes > -1 && nodes < 7)
            {
                nodesOnLine = nodes;
            }
        }
        if (nodesOnLine > 0)
        {
            bool changed = false;
            if (nodePlaces == null || nodePlaces.Length != 6)
            {
                changed = true;
                nodePlaces = new float[6];
            }
            if (lineNodesInOut == null || lineNodesInOut.Length != 6)
            {
                changed = true;
                lineNodesInOut = new NodeInOut[6];
                for (int i = 0; i < 6; i++)
                {
                    lineNodesInOut[i] = NodeInOut.InNode;
                }
            }
            if (tempPlaces == null || tempPlaces.Length != 6)
            {
                changed = true;
                tempPlaces = new float[6];
            }
            if (changed)
            {
                SetupLineNodes();
            }
        }
        if (nodesOnLine > 0)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Node placement on line (0-1)", EditorStyles.boldLabel);
        }
        for (int i = 0; i < nodesOnLine; i++)
        {
            tempPlaces[i] = EditorGUILayout.FloatField("" + (i+1) + " (" + nodePlaces[i] + ")", tempPlaces[i]);
            if (GUILayout.Button("Set"))
            {
                if (tempPlaces[i] != nodePlaces[i])
                {
                    bool isOk = true;
                    if (i == 0)
                    {
                        if (tempPlaces[i] < 0f)
                        {
                            isOk = false;
                        }
                    }
                    else
                    {
                        if (tempPlaces[i] <= nodePlaces[i - 1])
                        {
                            isOk = false;
                        }
                    }
                    if (i == nodesOnLine - 1)
                    {
                        if (tempPlaces[i] > 1f)
                        {
                            isOk = false;
                        }
                    }
                    else
                    {
                        if (tempPlaces[i] >= nodePlaces[i + 1])
                        {
                            isOk = false;
                        }
                    }
                    if (isOk)
                    {
                        nodePlaces[i] = tempPlaces[i];
                        SceneView.RepaintAll();
                    }
                    else
                    {
                        tempPlaces[i] = nodePlaces[i];
                    }
                }
            }

            bool isOut = false;
            if (lineNodesInOut[i] == NodeInOut.OutNode)
            {
                isOut = true;
            }
            bool check = isOut;
            isOut = EditorGUILayout.ToggleLeft("is out node?", isOut);
            if (isOut != check)
            {
                if (isOut)
                {
                    lineNodesInOut[i] = NodeInOut.OutNode;
                }
                else
                {
                    lineNodesInOut[i] = NodeInOut.InNode;
                }
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.Separator();
        if (nodesOnLine > 0)
        {
            if (GUILayout.Button("Done"))
            {
                SaveHelperLine();
                showLine = false;
                ResetLineValues();
                UpdatePointsToDraw();
                SceneView.RepaintAll();
            }
        }
    }

    private void SplineEditMenu()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Edit spline", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            intersection.MoveSplineIndex(-1);
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            intersection.MoveSplineIndex(1);
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        if (intersection.splineIndex == -1)
        {
            EditorGUILayout.LabelField("No created splines");
        }
        else
        {
            ItalicLabel("Spline " + intersection.splineIndex);
            intersection.createdSplines[intersection.splineIndex].turnDirection =
                (IntersectionDirection)EditorGUILayout.EnumPopup("Turn direction",
                intersection.createdSplines[intersection.splineIndex].turnDirection);
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("Add segment"))
        {
            intersection.AddSegmentToCurrentSpline();
            SceneView.RepaintAll();
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("Connect to selected outnode (" + intersection.outIndex + ")"))
        {
            intersection.ConnectSplineToOutNode();
            SceneView.RepaintAll();
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("Delete this spline"))
        {
            intersection.RemoveCurrentSpline();
            SceneView.RepaintAll();
        }
        EditorGUILayout.Separator();
        DrawEditorLine();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Finish splines", EditorStyles.boldLabel);
        ItalicLabel("When all splines are set, press 'Done'");
        if (GUILayout.Button("Done"))
        {
            allEndNodesConnected = intersection.AllSplineEndPointsConnected();
            if (allEndNodesConnected)
            {
                intersection.SetSegmentArrays();
                intersection.splinesSet = true;
                SceneView.RepaintAll();
            }
            
        }
        if (!allEndNodesConnected)
        {
            WarningLabel("End points of all splines must be connected to an out-node before" +
                "continuing");
        }
    }

    private void NodeSetupMenu()
    {
        SplineData sd = intersection.createdSplines[intersection.splineIndex];
        int index = intersection.splineIndex;
        EditorGUILayout.LabelField("Set nodes on splines", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            intersection.MoveSplineIndex(-1);
            nodesFetched = intersection.GetSegmentNodePositions(out currentSplineNodes, out otherSplineNodes);
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            intersection.MoveSplineIndex(1);
            nodesFetched = intersection.GetSegmentNodePositions(out currentSplineNodes, out otherSplineNodes);
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        ItalicLabel("Spline " + index);
        EditorGUILayout.Separator();
        for (int i = 0; i < sd.segmentNodes.Length; i++)
        {
            int pnts = sd.segmentNodes[i];
            pnts = EditorGUILayout.IntField("Nodes", pnts);
            if (pnts != sd.segmentNodes[i])
            {
                if (pnts > -1 && pnts < 30)
                {
                    sd.segmentNodes[i] = pnts;
                    nodesFetched = intersection.GetSegmentNodePositions(
                        out currentSplineNodes, out otherSplineNodes);
                }
            }
        }
        EditorGUILayout.Separator();
        DrawEditorLine();
        if (GUILayout.Button("Create Nodes"))
        {
            allSegmentsHaveNodes = intersection.NodesOnAllSegments();
            nameAutoChecked = false;
            if (allSegmentsHaveNodes && nameIsValid)
            {
                CreateIntersection();
            }
        }
        if (!allSegmentsHaveNodes)
        {
            WarningLabel("Some spline segments don't have nodes set.");
        }
        if (!nameIsValid)
        {
            WarningLabel("Please check the name");
        }
        DrawEditorLine();
        if (GUILayout.Button("Back"))
        {
            intersection.splinesSet = false;
            nodesFetched = false;
            SceneView.RepaintAll();
        }
    }

    private void CreateBezier()
    {
        
        if (intersection.createdSplines == null)
        {
            intersection.createdSplines = new SplineData[0];
        }
        SplineData sd = new SplineData();
        Vector3 pos = inPositions[intersection.inIndex];
        Nodes n = intersection.GetInNodeOfCurrentIndex();
        Vector3 dir;
        if (n!= null)
        {
            sd.startNode = n;
            dir = GetBezierStartDirection(pos, n);
        }
        else
        {
            dir = GetBezierStartDirection(pos);
        }
        float length = GetBezierStartLength();
        Vector3 p0 = pos;
        Vector3 p1 = pos + length / 3f * dir;
        Vector3 p2 = pos + length * 2f / 3f * dir;
        Vector3 p3 = pos + length * dir;
        sd.points = new Vector3[] { p0, p1, p2, p3 };
        sd.modes = new BezierControlPointMode[] { BezierControlPointMode.Aligned,
            BezierControlPointMode.Aligned};
        sd.endPointSet = false;
        sd.turnDirection = IntersectionDirection.Straight;
        Array.Resize(ref intersection.createdSplines, intersection.createdSplines.Length + 1);
        intersection.createdSplines[intersection.createdSplines.Length - 1] = sd;
        intersection.splineIndex = intersection.createdSplines.Length - 1;

    }

    private float GetBezierStartLength()
    {
        if (intersection.FrameHeight < intersection.FrameWidth)
        {
            return 0.5f * intersection.FrameHeight;
        }
        else
        {
            return 0.5f * intersection.FrameWidth;
        }
    }

    private Vector3 GetBezierStartDirection(Vector3 pos, Nodes n = null)
    {
        if (n != null)
        {
            if (n.OutNodes != null && n.OutNodes.Length > 0)
            {
                return (n.OutNodes[0].gameObject.transform.position
                    - n.gameObject.transform.position).normalized;
            }
            else if (n.InNodes != null && n.InNodes.Length > 0)
            {
                return (n.gameObject.transform.position
                    - n.InNodes[0].gameObject.transform.position).normalized;
            }
        }
        Vector3 d = intersection.CenterPoint - pos;
        if (Mathf.Abs(d.x) > Mathf.Abs(d.z))
        {
            return new Vector3(d.x, 0f, 0f).normalized;
        }
        else
        {
            return new Vector3(0f, 0f, d.z).normalized;
        }
    }

    private void SaveHelperLine()
    {
        HelperLine h = new HelperLine();
        h.startPoint = linePoints[0];
        h.direction = lineDir;
        h.lenght = lineLength;
        List<float> pnts = new List<float>();
        List<NodeInOut> ios = new List<NodeInOut>();
        for (int i = 0; i < nodesOnLine; i++)
        {
            pnts.Add(nodePlaces[i]);
            ios.Add(lineNodesInOut[i]);
        }
        h.nodePoints = pnts;
        h.inOut = ios;
        h.laneYield = lineYield;
        h.traffic = lineTraffic;
        h.speedLimit = lineSpeedLimit;
        intersection.helperLines.Add(h);
    }

    private void ResetLineValues()
    {
        nodePlaces = null;
        tempPlaces = null;
        lineNodesInOut = null;
        lineYAngle = 0f;
        lineAngle = 5f;
    }

    private void GenerateExistingLanes()
    {
        List<ExistingLane> existingLanes = new List<ExistingLane>();
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            Nodes currentNode = intersection.nodesInBox[i].node;
            List<Nodes> laneNodes = new List<Nodes>();
            bool isLane = false;
            int inNodeIndex = -1;
            int outNodeIndex = -1;
            // start node must be in-node
            if (intersection.nodesInBox[i].inOut == NodeInOut.InNode)
            {
                //Get in node index
                inNodeIndex = GetInNodeIndex(currentNode);
                while (true)
                {
                    // Add current node to list
                    laneNodes.Add(currentNode);
                    // exit if node is not in the box
                    if (!IsInBoxNodes(currentNode))
                    {
                        break;
                    }
                    // check if the node is out-node
                    if (IsOutNode(currentNode))
                    {
                        // Get out node index
                        outNodeIndex = GetOutNodeIndex(currentNode);
                        isLane = true;
                        break;
                    }
                    // exit if there is not a linked next node
                    if (currentNode.OutNodes == null || currentNode.OutNodes.Length==0)
                    {
                        break;
                    }
                    else
                    {
                        // iterate next node
                        currentNode = currentNode.OutNodes[0];
                    }
                }
            }
            // if we have found a string of nodes with in- and out-nodes in the box
            if (isLane)
            {
                // a new ExistingLane
                ExistingLane ex = new ExistingLane();
                ex.nodes = laneNodes;
                ex.confirmed = false;
                ex.laneYield = DriverYield.Normal;
                ex.inNodeIndex = inNodeIndex;
                ex.outNodeIndex = outNodeIndex;
                ex.turnDirection = IntersectionDirection.Straight;
                existingLanes.Add(ex);
            }
        
        }
        intersection.existingLanes = existingLanes;
        // Get positions for drawing existing lanes
        GetExistingLaneLinesToDraw();
        if (existingLanes.Count==0)
        {
            intersection.existingLanesChecked = true;
        }
        else
        {
            intersection.existingLanesChecked = false;
            intersection.existingLaneIndex = 0;
        }
    }

    private void GetExistingLaneLinesToDraw()
    {
        existingLaneLinesToDraw = new List<Vector3>();
        if (intersection.existingLanes == null)
        {
            return;
        }
        for (int i = 0; i < intersection.existingLanes.Count; i++)
        {
            ExistingLane ex = intersection.existingLanes[i];
            for (int j = 0; j < ex.nodes.Count - 1; j++)
            {
                existingLaneLinesToDraw.Add(ex.nodes[j].gameObject.transform.position);
                existingLaneLinesToDraw.Add(ex.nodes[j + 1].gameObject.transform.position);
            }
        }
    }

    private bool IsInBoxNodes(Nodes n)
    {
        bool isTrue = false;
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            if (intersection.nodesInBox[i].node == n)
            {
                isTrue = true;
                break;
            }
        }
        return isTrue;
    }

    private bool IsOutNode(Nodes n)
    {
        bool isTrue = false;
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            if (intersection.nodesInBox[i].node == n)
            {
                if (intersection.nodesInBox[i].inOut == NodeInOut.OutNode)
                {
                    isTrue = true;
                }
                break;
            }
        }
        return isTrue;
    }

    private int GetInNodeIndex(Nodes n)
    {
        int ind = -1;
        bool found = false;
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            NodeInfo ni = intersection.nodesInBox[i];
            if (ni.inOut == NodeInOut.InNode)
            {
                ind++;
            }
            if (ni.node == n)
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            return ind;
        }
        else
        {
            return -1;
        }
    }

    private int GetOutNodeIndex(Nodes n)
    {
        int ind = -1;
        bool found = false;
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            NodeInfo ni = intersection.nodesInBox[i];
            if (ni.inOut == NodeInOut.OutNode)
            {
                ind++;
            }
            if (ni.node == n)
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            return ind;
        }
        else
        {
            return -1;
        }
    }

    private void SetCurrentExistingLaneValuesToInspector()
    {
        ExistingLane ex = intersection.GetCurrentExistingLane();
        if (ex != null)
        {
            intersection.inIndex = ex.inNodeIndex;
            intersection.outIndex = ex.outNodeIndex;
            exYield = ex.laneYield;
            exTurn = ex.turnDirection;
            exConfirmed = ex.confirmed;
            exSpeedLimit = ex.speedLimit;
            exTraffic = ex.traffic;
        }
    }

    private void UpdatePointsToDraw()
    {
        pointsToDraw = new List<Vector3>();
        if (intersection.helperLines == null)
        { return; }
        pointsToDrawColors = new List<Color>();
        for (int i = 0; i < intersection.helperLines.Count; i++)
        {
            HelperLine h = intersection.helperLines[i];
            Vector3 dir = h.direction;
            float lenght = h.lenght;
            Vector3 p0 = h.startPoint;
            for (int j = 0; j < h.nodePoints.Count; j++)
            {
                Vector3 pnt = p0 + h.nodePoints[j] * lenght * dir;
                Color c = Color.blue;
                if (h.inOut[j] == NodeInOut.OutNode)
                {
                    c = Color.red;
                }
                pointsToDraw.Add(pnt);
                pointsToDrawColors.Add(c);
            }
        }
    }

    private void SetupLineNodes()
    {
        switch (nodesOnLine)
        {
            case 1:
                nodePlaces[0] = 0.5f;
                nodePlaces[1] = 1.0f;
                nodePlaces[2] = 1.0f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 2:
                nodePlaces[0] = 0.25f;
                nodePlaces[1] = 0.75f;
                nodePlaces[2] = 1.0f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 3:
                nodePlaces[0] = 0.25f;
                nodePlaces[1] = 0.5f;
                nodePlaces[2] = 0.75f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 4:
                nodePlaces[0] = 0.2f;
                nodePlaces[1] = 0.4f;
                nodePlaces[2] = 0.6f;
                nodePlaces[3] = 0.8f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 5:
                nodePlaces[0] = 0.2f;
                nodePlaces[1] = 0.35f;
                nodePlaces[2] = 0.5f;
                nodePlaces[3] = 0.65f;
                nodePlaces[4] = 0.8f;
                nodePlaces[5] = 1.0f;
                break;
            case 6:
                nodePlaces[0] = 0.125f;
                nodePlaces[1] = 0.275f;
                nodePlaces[2] = 0.425f;
                nodePlaces[3] = 0.575f;
                nodePlaces[4] = 0.725f;
                nodePlaces[5] = 0.875f;
                break;
        }
        tempPlaces[0] = nodePlaces[0];
        tempPlaces[1] = nodePlaces[1];
        tempPlaces[2] = nodePlaces[2];
        tempPlaces[3] = nodePlaces[3];
        tempPlaces[4] = nodePlaces[4];
        tempPlaces[5] = nodePlaces[5];
    }

    private void CreateIntersection()
    {
        string parentName = intersectionName;
        // Create parent gameobject
        GameObject parent = new GameObject(parentName);
        parent.transform.position = intersection.CenterPoint;
        parent.AddComponent<Road>();
        // Existing lanes
        int laneObjectIndex = CreateLanesFromExistingLanes(parent);
        // Lanes from spline data
        CreateLanesFromSplineData(laneObjectIndex, parent);


        // finally parent intersection gameobject to roadnetwork
        parent.transform.parent = intersection.roadNetwork.transform;
        // Tagging
    }
    // modifies lanes and adds Lane-gameobjects (with list of nodes but no child objects) to parent
    private int CreateLanesFromExistingLanes(GameObject parent)
    {
        int index = 0;
        if (intersection.existingLanes == null)
        {
            return index;
        }
        for (int i = 0; i < intersection.existingLanes.Count; i++)
        {
            ExistingLane ex = intersection.existingLanes[i];
            // Create parent object (Lane) and assign values
            string laneName = parent.gameObject.name + "_" + i + "_" + "ex";
            GameObject g = new GameObject(laneName);
            g.transform.position = ex.nodes[0].transform.position;
            g.AddComponent(typeof(Lane));
            Lane lane = g.GetComponent<Lane>();
            lane.Traffic = ex.traffic;
            lane.LaneYield = ex.laneYield;
            lane.SpeedLimit = ex.speedLimit;
            // Iterate nodes
            List<Nodes> nodes = new List<Nodes>();
            for (int j = 0; j < ex.nodes.Count; j++)
            {
                Nodes n = ex.nodes[j];
                n.ParallelLeft = null;
                n.ParallelRight = null;
                n.LaneChangeLeft = null;
                n.LaneChangeRight = null;
                n.IsInIntersection = true;
                n.TurnDirection = ex.turnDirection;
                nodes.Add(n);
            }
            // Add nodes to lane's node array
            lane.nodesOnLane = new Nodes[nodes.Count];
            for (int j = 0; j < nodes.Count; j++)
            {
                lane.nodesOnLane[j] = nodes[j];
            }
            // parent lane
            lane.transform.parent = parent.transform;
            ObjectTagger.SetLaneIcons(TagColorScheme.ByLaneNumber, i % 6, ref lane.nodesOnLane);
            index++;
        }
        return index;
    }

    private void CreateLanesFromSplineData(int laneObjectIndex, GameObject parent)
    {
        if (intersection.createdSplines == null)
        {
            return;
        }
        List<Nodes> newInNodes = new List<Nodes>();
        List<Nodes> newOutNodes = new List<Nodes>();
        string parentName = parent.gameObject.name;
        for (int splineInd = 0; splineInd < intersection.createdSplines.Length; splineInd++)
        {
            string laneName = parentName + "_" + laneObjectIndex;
            SplineData sd = intersection.createdSplines[splineInd];

            // Create lane object
            GameObject laneObject = new GameObject(laneName);
            laneObject.transform.position = sd.points[0];
            laneObject.AddComponent(typeof(Lane));
            Lane lane = laneObject.GetComponent<Lane>();

            Nodes startNode = null;
            Nodes endNode = null;

            //these are needed for parenting
            bool startNodeGenerated = false;
            bool endNodeGenerated = false;
            int startHelperIndex = 0;
            int endHelperIndex = 0;

            // assign start node, generate a new one if it doesn't exist
            if (sd.startNode != null)
            {
                startNode = sd.startNode;
                startNode.ParallelLeft = null;
                startNode.ParallelRight = null;
                startNode.LaneChangeLeft = null;
                startNode.LaneChangeRight = null;
                startNode.IsInIntersection = true;
                startNode.TurnDirection = sd.turnDirection;
            }
            else
            {
                Vector3 pos = sd.points[0];
                bool found = false;
                for ( int i = 0; i < newInNodes.Count; i++)
                {
                    if (newInNodes[i].transform.position == pos)
                    {
                        startNode = newInNodes[i];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    string nodeName = laneName + "_" + 0;
                    startNodeGenerated = true;
                    startNode = GenerateNode(pos, sd.turnDirection, nodeName);
                    newInNodes.Add(startNode);
                    startHelperIndex = intersection.GetHelperLineIndex(pos);
                }
            }
            // assign end node, generate a new one if it doesn't exist
            if (sd.endNode != null)
            {
                endNode = sd.endNode;
                endNode.ParallelLeft = null;
                endNode.ParallelRight = null;
                endNode.LaneChangeLeft = null;
                endNode.LaneChangeRight = null;
                endNode.IsInIntersection = true;
                endNode.TurnDirection = sd.turnDirection;
            }
            else
            {
                Vector3 pos = sd.points[sd.points.Length - 1];
                bool found = false;
                for (int i = 0; i < newOutNodes.Count; i++)
                {
                    if (newOutNodes[i].transform.position == pos)
                    {
                        endNode = newOutNodes[i];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    string nodeName = laneName + "_" + (sd.points.Length - 1);
                    endNodeGenerated = true;
                    endNode = GenerateNode(pos, sd.turnDirection, nodeName);
                    newOutNodes.Add(endNode);
                    endHelperIndex = intersection.GetHelperLineIndex(pos);
                }
            }
            // positions for the rest of the nodes
            List<Vector3> inBetweenPositions = intersection.GetNodePositionInBetweenEndPoints(splineInd);
            // Create nodes in between start and end nodes
            List<Nodes> bNodes = new List<Nodes>();
            for (int i = 0; i < inBetweenPositions.Count; i++)
            {
                string nodeName = laneName + "_" + (i + 1);
                Nodes n = GenerateNode(inBetweenPositions[i], sd.turnDirection, nodeName);
                bNodes.Add(n);
            }
            // Assign values to lane
            if (startNodeGenerated)
            {
                lane.Traffic = intersection.helperLines[startHelperIndex].traffic;
                lane.LaneYield = intersection.helperLines[startHelperIndex].laneYield;
            }
            else
            {
                lane.Traffic = startNode.Traffic;
                lane.LaneYield = startNode.LaneYield;
            }
            if (endNodeGenerated)
            {
                lane.SpeedLimit = intersection.helperLines[endHelperIndex].speedLimit;
            }
            else
            {
                lane.SpeedLimit = endNode.SpeedLimit;
            }
            // Assign nodes to lane
            lane.nodesOnLane = new Nodes[inBetweenPositions.Count + 2];
            lane.nodesOnLane[0] = startNode;
            for (int i = 0; i < bNodes.Count; i++)
            {
                lane.nodesOnLane[i + 1] = bNodes[i];
            }
            lane.nodesOnLane[lane.nodesOnLane.Length - 1] = endNode;
            for (int i = 0; i < lane.nodesOnLane.Length; i++)
            {
                Nodes n = lane.nodesOnLane[i];
                // for pre-existing start nodes no parenting
                if (i == 0)
                {
                    if (startNodeGenerated)
                    {
                        n.ParentLane = lane;
                        n.transform.parent = lane.transform;
                    }
                }
                // for pre existing end nodes no parenting
                else  if (i == lane.nodesOnLane.Length - 1)
                {
                    if (endNodeGenerated)
                    {
                        n.ParentLane = lane;
                        n.transform.parent = lane.transform;
                    }
                }
                else
                {
                    n.ParentLane = lane;
                    n.transform.parent = lane.transform;
                }
                // connect in and out nodes
                if (i > 0)
                {
                    n.AddInNode(lane.nodesOnLane[i - 1]);
                }
                if (i < lane.nodesOnLane.Length - 1)
                {
                    n.AddOutNode(lane.nodesOnLane[i + 1]);
                }
            }
            laneObject.transform.parent = parent.transform;
            ObjectTagger.SetLaneIcons(TagColorScheme.ByLaneNumber, laneObjectIndex % 6, ref lane.nodesOnLane);
            laneObjectIndex++;
        }
    }

    private Nodes GenerateNode (Vector3 position, IntersectionDirection turnDirection, string goName)
    {
        GameObject g = new GameObject(goName);
        g.AddComponent(typeof(Nodes));
        g.transform.position = position;
        Nodes n = g.GetComponent<Nodes>();
        n.IsInIntersection = true;
        n.TurnDirection = turnDirection;
        return n;
    }

    private void EntryHelp()
    {
        EditorGUILayout.LabelField("Entry direction means here a road connecting to this" +
            " intersection. It may consist of multiple lanes going both directions.", EditorStyles.wordWrappedLabel);
    }

    private bool CheckName(string name)
    {
        if (roadNetwork == null)
        {
            return false;
        }
        Transform t = roadNetwork.transform.Find(name);
        if (t == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void DrawEditorLine()
    {
        int thickness = 2;
        int padding = 10;
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, Color.black);
    }

    private void ItalicLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.fontStyle = FontStyle.Italic;
        gs.wordWrap = true;
        EditorGUILayout.LabelField(message, gs);
    }

    private void WarningLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.normal.textColor = Color.red;
        gs.wordWrap = true;
        EditorGUILayout.LabelField(message, gs);
    }

    //************************ SCENE VIEW START


    private void OnSceneGUI()
    {
        handleTransform = intersection.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;
        Handles.color = Color.white;
        Handles.DrawLines(framingHorizontal);
        Handles.DrawLines(framingVertical);
        HighlightNodes();
        if (showLine)
        {
            ShowMarkerLine();
            if (nodesOnLine > 0)
            {
                ShowNodePlacesOnLine();
            }
        }
        if (pointsToDraw != null || pointsToDraw.Count > 0)
        {
            ShowSavedHelperPoints();
        }
        if (intersection.allNodesSet)
        {
            if (inPositions == null || outPositions == null)
            {
                inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
                //inIndex = 0;
                //outIndex = 0;
            }
            ShowNodeNumbers();
            ShowExistingLanes();
            ShowBeziers();
            if (intersection.splinesSet)
            {
                ShowSplineNodes();
            }
        }
    }

    private void DrawSceneDisc(GameObject targetObject, Color c, bool larger)
    {
        Handles.color = c;
        float m = 0.01f;
        if (larger)
        {
            m = 0.015f;
        }
        Handles.DrawSolidDisc(targetObject.transform.position, new Vector3(0f, 1f, 0f),
            m * Vector3.Distance(targetObject.transform.position,
            SceneView.lastActiveSceneView.camera.transform.position));
    }

    private void DrawSceneDisc(Vector3 pos, Color c, bool larger)
    {
        Handles.color = c;
        float m = 0.01f;
        if (larger)
        {
            m = 0.015f;
        }
        Handles.DrawSolidDisc(pos, new Vector3(0f, 1f, 0f), m * Vector3.Distance(
            pos, SceneView.lastActiveSceneView.camera.transform.position));
    }

    private void ShowNodeNumbers()
    {
        GUIStyle g = new GUIStyle();
        g.normal.textColor = Color.white;
        for (int i = 0; i < inPositions.Count; i++)
        {
            Handles.Label(inPositions[i], "In " + i, g);
        }

        for (int i = 0; i < outPositions.Count; i++)
        {
            Handles.Label(outPositions[i], "Out " + i, g);
        }
    }

    private void ShowExistingLanes()
    {
        if (existingLaneLinesToDraw != null)
        {
            Handles.color = Color.green;
            for (int i = 0; i < existingLaneLinesToDraw.Count; i += 2)
            {
                Handles.DrawLine(existingLaneLinesToDraw[i], existingLaneLinesToDraw[i + 1]);
            }
        }
    }

    private void ShowBeziers()
    {
        if (intersection.splineIndex == -1)
        {
            return;
        }
        for (int i = 0; i < intersection.createdSplines.Length; i++)
        {
            SplineData sd = intersection.createdSplines[i];
            Vector3 p0 = sd.points[0];
            for (int j = 1; j < sd.points.Length; j += 3)
            {
                Vector3 p1 = sd.points[j];
                Vector3 p2 = sd.points[j + 1];
                Vector3 p3 = sd.points[j + 2];
                Color c = Color.gray;
                if (i == intersection.splineIndex)
                {
                    c = Color.magenta;
                    DrawControlPoint(i, j);
                    DrawControlPoint(i, j + 1);
                    if (!(j + 3 == sd.points.Length && sd.endPointSet))
                    {
                        DrawControlPoint(i, j + 2);
                    }
                    if (!intersection.splinesSet)
                    {
                        Handles.DrawLine(p0, p1);
                        Handles.DrawLine(p2, p3);
                    }
                }
                Handles.color = Color.gray;
                
                Handles.DrawBezier(p0, p3, p1, p2, c, null, 2f);
                p0 = p3;
            }
        }
    }


    private void ShowSplineNodes()
    {
        if (nodesFetched)
        {
            if (currentSplineNodes != null)
            {
                for (int i = 0; i < currentSplineNodes.Count; i++)
                {
                    DrawSceneDisc(currentSplineNodes[i], Color.yellow, true);
                }
            }
            if (otherSplineNodes != null)
            {
                for (int i = 0; i < otherSplineNodes.Count; i++)
                {
                    DrawSceneDisc(otherSplineNodes[i], Color.gray, false);
                }
            }
        }
    }

    private void DrawControlPoint(int splineIndex, int pointIndex)
    {
        if (intersection.splinesSet)
        {
            return;
        }
        SplineData sd = intersection.createdSplines[splineIndex];
        Vector3 point = sd.points[pointIndex];
        float size = HandleUtility.GetHandleSize(point);

        Handles.color = Color.cyan;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedIndex = new Vector2Int (splineIndex, pointIndex);
            Repaint(); // refresh inspector
        }
        if (selectedIndex.x == splineIndex && selectedIndex.y == pointIndex)
        {
            Event e = Event.current;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            EditorGUI.BeginChangeCheck();
            
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(intersection, "MovePoint");
                EditorUtility.SetDirty(intersection);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                intersection.SetSplinePoint(splineIndex, pointIndex, point);
            }
        }
    }

    private void ShowMarkerLine()
    {
        Handles.color = Color.cyan;
        Handles.DrawLine(linePoints[0], linePoints[1]);
    }

    private void ShowNodePlacesOnLine()
    {
        for (int i = 0; i < nodesOnLine; i++)
        {
            Vector3 pos = linePoints[0] + nodePlaces[i] * lineLength * lineDir;
            if (lineNodesInOut[i] == NodeInOut.InNode)
            {
                DrawSceneDisc(pos, Color.blue, false);
            }
            else
            {
                DrawSceneDisc(pos, Color.red, false);
            }
        }
    }

    private void ShowSavedHelperPoints()
    {
        for (int i = 0; i < pointsToDraw.Count; i++)
        {
            DrawSceneDisc(pointsToDraw[i], pointsToDrawColors[i], false);
        }
    }

    private void HighlightNodes()
    {
        if (!intersection.allNodesSet)
        {
            Nodes selected = null;
            if (intersection.GetInfoIndex >= 0)
            {
                selected = intersection.GetSelectedNodeInfo(out NodeInOut inOut);
            }
            if (selected != null)
            {
                DrawSceneDisc(selected.gameObject, Color.yellow, true);
            }
        }
        else
        {
            Vector3 pos = inPositions[intersection.inIndex];
            DrawSceneDisc(pos, Color.yellow, true);

            pos = outPositions[intersection.outIndex];
            DrawSceneDisc(pos, Color.magenta, true);
        }
        int index = intersection.GetInfoIndex;
        if (intersection.nodesInBox != null)
        {
            for (int i = 0; i < intersection.nodesInBox.Length; i++)
            {
                NodeInfo ni = intersection.nodesInBox[i];
                if (ni.inOut == NodeInOut.InNode)
                {
                    DrawSceneDisc(ni.node.gameObject, Color.blue, false);
                }
                else if (ni.inOut == NodeInOut.OutNode)
                {
                    DrawSceneDisc(ni.node.gameObject, Color.red, false);
                }
            }
        }
    }

    private void RotateLine (bool clockwise)
    {
        float angle = lineAngle;
        if (!clockwise)
        {
            angle = -angle;
        }
        Vector3 dir0 = linePoints[0] - linePos;
        Vector3 dir1 = linePoints[1] - linePos;
        dir0 = Quaternion.Euler(new Vector3(0f, angle, 0f))*dir0;
        dir1 = Quaternion.Euler(new Vector3(0f, angle, 0f)) * dir1;
        linePoints[0] = dir0 + linePos;
        linePoints[1] = dir1 + linePos;
    }

    private void UpdateLinePosition()
    {
        Vector3 p0 = linePos + new Vector3(0f, 0f, -lineLength / 2f);
        Vector3 p1 = linePos + new Vector3(0f, 0f, lineLength / 2f);
        //rotation
        Vector3 dir0 = p0 - linePos;
        Vector3 dir1 = p1 - linePos;
        dir0 = Quaternion.Euler(new Vector3(0f, lineYAngle, 0f)) * dir0;
        dir1 = Quaternion.Euler(new Vector3(0f, lineYAngle, 0f)) * dir1;
        p0 = dir0 + linePos;
        p1 = dir1 + linePos;

        linePoints = new Vector3[] { p0, p1 };
        bool needToAdjust = CheckLinePointsInBounds();
        lineDir = (linePoints[1] - linePoints[0]).normalized;
        if (needToAdjust)
        {
            lineLength = Vector3.Distance(linePoints[0], linePoints[1]);
            linePos = linePoints[0] + lineDir * lineLength * 0.5f;
        }
    }

    private bool CheckLinePointsInBounds()
    {
        float minX = intersection.CenterPoint.x - intersection.FrameWidth / 2f;
        float maxX = intersection.CenterPoint.x + intersection.FrameWidth / 2f;
        float minZ = intersection.CenterPoint.z - intersection.FrameHeight / 2f;
        float maxZ = intersection.CenterPoint.z + intersection.FrameHeight / 2f;

        bool needToAdjust = false;

        if (linePoints[0].x < minX)
        {
            needToAdjust = true;
            linePoints[0].x = minX;
        }
        else if (linePoints[0].x > maxX)
        {
            needToAdjust = true;
            linePoints[0].x = maxX;
        }
        if (linePoints[1].x < minX)
        {
            needToAdjust = true;
            linePoints[1].x = minX;
        }
        else if (linePoints[1].x > maxX)
        {
            needToAdjust = true;
            linePoints[1].x = maxX;
        }
        if (linePoints[0].z < minZ)
        {
            needToAdjust = true;
            linePoints[0].z = minZ;
        }
        else if (linePoints[0].z > maxZ)
        {
            needToAdjust = true;
            linePoints[0].z = maxZ;
        }
        if (linePoints[1].z < minZ)
        {
            needToAdjust = true;
            linePoints[1].z = minZ;
        }
        else if (linePoints[1].z > maxZ)
        {
            needToAdjust = true;
            linePoints[1].z = maxZ;
        }

        return needToAdjust;
    }

    private void UpdateFramingBox()
    {
        Vector3 corner1 = intersection.CenterPoint;
        corner1 += new Vector3(-intersection.FrameWidth * 0.5f, 0f, -intersection.FrameHeight * 0.5f);
        Vector3 corner2 = intersection.CenterPoint;
        corner2 += new Vector3(intersection.FrameWidth * 0.5f, 0f, -intersection.FrameHeight * 0.5f);
        Vector3 corner3 = intersection.CenterPoint;
        corner3 += new Vector3(intersection.FrameWidth * 0.5f, 0f, intersection.FrameHeight * 0.5f);
        Vector3 corner4 = intersection.CenterPoint;
        corner4 += new Vector3(-intersection.FrameWidth * 0.5f, 0f, intersection.FrameHeight * 0.5f);
        framingHorizontal = new Vector3[] { corner1, corner2, corner3, corner4 };
        framingVertical = new Vector3[] { corner4, corner1, corner3, corner2 };
    }

    private void UpdateNodesInBox()
    {
        if (!intersection.nodesInBoxSet)
        {
            float minX = intersection.CenterPoint.x - intersection.FrameWidth / 2f;
            float maxX = intersection.CenterPoint.x + intersection.FrameWidth / 2f;
            float minZ = intersection.CenterPoint.z - intersection.FrameHeight / 2f;
            float maxZ = intersection.CenterPoint.z + intersection.FrameHeight / 2f;

            List<Nodes> nodes = new List<Nodes>();
            Nodes[] allNodes = GameObject.FindObjectsOfType<Nodes>();
            for (int i = 0; i < allNodes.Length; i++)
            {
                float nodeX = allNodes[i].gameObject.transform.position.x;
                float nodeZ = allNodes[i].gameObject.transform.position.z;
                if (nodeX > minX && nodeX < maxX && nodeZ > minZ && nodeZ < maxZ)
                {
                    nodes.Add(allNodes[i]);
                }
            }
            NodeInfo[] nInfo = new NodeInfo[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                NodeInfo ni = new NodeInfo();
                ni.node = nodes[i];
                ni.inOut = NodeInOut.NotUsed;
                nInfo[i] = ni;
            }
            intersection.nodesInBox = nInfo;
        }
    }
    //************************ ENABLE / DISABLE

    private void DisableTools()
    {
        Tools.current = Tool.View;
        Tools.hidden = true;
    }
    
    private void OnEnable()
    {
        intersection = target as Intersection;
        UpdateFramingBox();

        if (intersection.framed)
        {
            Tools.current = Tool.View;
            Tools.hidden = true;
            if (intersection.nodesInBox == null)
            {
                UpdateNodesInBox();
            }
        }
        UpdatePointsToDraw();
        GetExistingLaneLinesToDraw();
        if (intersection.allNodesSet)
        {
            inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
        }
        if (intersection.existingLanesChecked == false)
        {
            intersection.existingLaneIndex = 0;
            SetCurrentExistingLaneValuesToInspector();
        }
        if (intersection.splinesSet)
        {
            nodesFetched = intersection.GetSegmentNodePositions(
                out currentSplineNodes, out otherSplineNodes);
        }

        SetCameraAngle();
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    private void SetCameraAngle()
    {
        var sceneView = SceneView.lastActiveSceneView;
        sceneView.AlignViewToObject(intersection.transform);
        sceneView.LookAtDirect(intersection.transform.position, Quaternion.Euler(90, 0, 0), 30f);
        sceneView.orthographic = true;
    }
}
