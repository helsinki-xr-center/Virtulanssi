using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(ParallelBezierSplines))]
public class ParallelBezierSplinesInspector : Editor
{
    // for displaying bezier direction at set intervals
    private const int stepsPerCurve = 10;
    // length modifier of direction visualisation lines
    private const float directionScale = 0.5f;
    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;
    private const float laneChangeDistance = 3.5f;
    [Range(0, 200)]
    public int wayPoints = 0;
    private int selectedIndex = -1;
    private float angle = 0f;
    private bool arraysInitialized = false;

    private bool nameIsValid = false;
    private bool nameAutoChecked = true;
    private string roadName = "";
    private string renameInfo = "";

    private ParallelBezierSplines parallel;
    private GameObject roadNetwork;
    private Transform handleTransform;
    private Quaternion handleRotation;
    private bool perSpline;
    private bool advancedOptionsOn = false;
    private bool[] selected;
    private Vector3[] rightBusPoints;
    private Vector3[] leftBusPoints;

    //for basic settings
    private bool laneCountSet = false;
    bool linkedToNode = false;
    // if this is false, current selected general direction is used
    bool directionSet = false;
    private bool previewNodes = true;
    bool verifying = false;
    [SerializeField]
    List<Vector3> laneChangePoints;
    bool laneChangesSet = false;

    private int leftLaneCount;
    private int rightLaneCount;
    [SerializeField]
    private float spacingOverride = 2.5f;
    [SerializeField]
    private float adjustment = 0f;
    private float[] lSpacing;
    private float[] rSpacing;
    [SerializeField]
    private Vector3[] guidePoints;
    [SerializeField]
    private float defaultLength = 10f;
    [SerializeField]
    Directions generalDirection;
    [SerializeField]
    Vector3 directionVector;

    //All nodes in scene are gathered here for drawing purposes
    Nodes[] allNodes;
    private bool showNodeLabels = false;
    

    public override void OnInspectorGUI()
    {
        parallel = target as ParallelBezierSplines;
        Undo.RecordObject(parallel, "changed");

        NameMenu();

        if (arraysInitialized == false)
        {
            InitializeArrays();
        }
        if (!parallel.Initialized)
        {
            BasicSettingsMenu();
        }
        else
        {
            if (!parallel.LanesSet)
            {
                EditMenu();
            }
            else
            {
                WaypointEditMenu();
            }
        }
        //base.OnInspectorGUI();
    }

    private void InitializeArrays()
    {
        if (lSpacing == null || lSpacing.Length != 3)
        {
            lSpacing = new float[] { 0, 0, 0 };
        }
        if (rSpacing == null || rSpacing.Length != 3)
        {
            rSpacing = new float[] { 0, 0, 0 };
        }
        if (guidePoints == null || guidePoints.Length != 4)
        {
            Vector3 pos = parallel.transform.position;
            guidePoints = new Vector3[] { pos, pos, pos, pos };
        }
        if (selected == null || selected.Length != 6)
        {
            //right lanes 1, 2, 3, left lanes 1, 2, 3
            selected = new bool[] { false, false, false, false, false, false };
        }
        generalDirection = Directions.North;
        directionVector = GeneralDirection.DirectionVector(generalDirection);

        arraysInitialized = true;
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
            if (parallel.roadNetwork != null)
            {
                roadNetwork = parallel.roadNetwork;
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
                    parallel.roadNetwork = g;
                }
                else if (networks.Length == 1)
                {
                    roadNetwork = networks[0].gameObject;
                    parallel.roadNetwork = roadNetwork;
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
                            parallel.roadNetwork = networks[i].gameObject;
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
            EditorGUILayout.LabelField("Road name:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(parallel.gameObject.name);
            EditorGUILayout.EndHorizontal();
            if (!nameAutoChecked)
            {
                nameAutoChecked = true;
                nameIsValid = CheckName(parallel.gameObject.name);
                if (nameIsValid)
                {
                    roadName = parallel.gameObject.name;
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
            roadName = GUILayout.TextField(roadName);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enter name");
            if (GUILayout.Button("Rename"))
            {
                bool valid = CheckName(roadName);
                if (!valid)
                {
                    renameInfo = "New name was not valid.";
                }
                else
                {
                    renameInfo = "Name changed to '" + roadName + "'.";
                    parallel.gameObject.name = roadName;
                    nameIsValid = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            ItalicLabel(renameInfo);

            EditorGUILayout.Separator();
        }
        EditorGUILayout.LabelField("Traffic", EditorStyles.boldLabel);
        parallel.Traffic = (TrafficSize)EditorGUILayout.EnumPopup("Traffic size", parallel.Traffic);
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Overall speed limit", EditorStyles.boldLabel);
        parallel.SpeedLimit = (SpeedLimits)EditorGUILayout.EnumPopup("Limit", parallel.SpeedLimit);
        EditorGUILayout.Separator();
        bool showLabels = showNodeLabels;
        showLabels = EditorGUILayout.ToggleLeft("Show node labels", showLabels);
        if (showLabels != showNodeLabels)
        {
            showNodeLabels = showLabels;
            SceneView.RepaintAll();
        }
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void WarningLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.normal.textColor = Color.red;
        EditorGUILayout.LabelField(message, gs);
    }

    private void ItalicLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.fontStyle = FontStyle.Italic;
        EditorGUILayout.LabelField(message, gs);
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

    private void BasicSettingsMenu()
    {
        if (verifying)
        {
            EditorGUILayout.LabelField("Are you sure?", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                verifying = false;
                InitializeParallel();
            }
            if (GUILayout.Button("No"))
            {
                verifying = false;
            }
            EditorGUILayout.EndHorizontal();
            return;
        }
        parallel = target as ParallelBezierSplines;
        ShowSetupInstructions();
        if (!laneCountSet)
        {
            ShowLaneCountSelection();
            return;
        }
        if (GUILayout.Button("Reset and set lane counts again (1.)"))
        {
            ResetValues();
            SceneView.RepaintAll();
        }
        ShowLaneSetupSelection();
        
        if (leftLaneCount > 0 || rightLaneCount > 0)
        {
            ShowSpacingSelection();
            if (GUILayout.Button("Setup done"))
            {
                verifying = true;
            }
        }

    }

    private void EditMenu()
    {
        ShowEditInstructions();
        if (GUILayout.Button("Add segment"))
        {
            parallel.AddCurve();
        }
        ShowSelectedNodeInfo();
        ShowAngleChangeOption();
        ShowAdvancedOptions();
        if (GUILayout.Button("Continue"))
        {
            parallel.LanesSet = true;
            SceneView.RepaintAll();
        }
    }

    private void WaypointEditMenu()
    {
        ShowNodeSetupInstructions();
        EditorGUILayout.LabelField("Node set-up", EditorStyles.boldLabel);
        if (GUILayout.Button("Back"))
        {
            if (parallel.NodesSet)
            {
                parallel.NodesSet = false;
                rightBusPoints = null;
                leftBusPoints = null;
                laneChangesSet = false;
                for (int i = 7; i >= 0; i--)
                {
                    parallel.permittedLaneChanges[i] = false;
                }
                SceneView.RepaintAll();
            }
            else
            {
                parallel.LanesSet = false;
                laneChangesSet = false;
                SceneView.RepaintAll();
            }
        }

        EditorGUILayout.Separator();

        if (parallel.NodesSet)
        {
            BusLanesMenu();
            LaneChangeMenu();
            EndNodeLinkMenu();
            CreateNodesMenu();
            return;
        }

        EditorGUILayout.LabelField("Node placement", EditorStyles.boldLabel);

        bool prev = previewNodes;
        prev = EditorGUILayout.Toggle("Preview nodes", prev);
        if (prev != previewNodes)
        {
            previewNodes = prev;
            SceneView.RepaintAll();
        }

        for (int i = 0; i < parallel.SegmentCount; i++)
        {
            int pts = parallel.GetNodesOnSegment(i);
            pts = EditorGUILayout.IntField("Seg. " + (i + 1) + " nodes", pts);
            if (pts >= 0 && pts != parallel.GetNodesOnSegment(i))
            {
                parallel.SetNodesOnSegment(i, pts);
            }
        }

        DrawEditorLine();
        if (GUILayout.Button("Next"))
        {
            //check that node count is set for each segment and count nodes
            bool segSet = true;
            int nodes = 1;
            for (int i = 0; i < parallel.SegmentCount; i++)
            {
                int n = parallel.GetNodesOnSegment(i);
                if (n == 0)
                {
                    segSet = false;
                    break;
                }
                else
                {
                    nodes += n;
                }
            }
            if (segSet == true)
            {
                parallel.NodeCount = nodes;
                if (!parallel.BusLaneRight)
                {
                    parallel.BusRightStart = 0;
                    parallel.BusRightEnd = nodes - 1;
                }
                else
                {
                    if (parallel.BusRightEnd > nodes - 1)
                    {
                        parallel.BusRightEnd = nodes - 1;
                    }
                    if (parallel.BusRightStart >= parallel.BusRightEnd)
                    {
                        parallel.BusRightStart = parallel.BusRightEnd - 1;
                    }
                }
                if (!parallel.BusLaneLeft)
                {
                    parallel.BusLeftStart = 0;
                    parallel.BusLeftEnd = nodes - 1;
                }
                else
                {
                    if (parallel.BusLeftEnd > nodes - 1)
                    {
                        parallel.BusLeftEnd = nodes - 1;
                    }
                    if (parallel.BusLeftStart >= parallel.BusLeftEnd)
                    {
                        parallel.BusLeftStart = parallel.BusLeftEnd - 1;
                    }
                }
                parallel.NodesSet = true;
            }
            SceneView.RepaintAll();
        }
    }

    private void BusLanesMenu()
    {
        EditorGUILayout.LabelField("Bus lanes", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Is the righmost lane on the RIGHT side (BLUE) lanes a bus lane?", EditorStyles.wordWrappedLabel);
        bool busOn = parallel.BusLaneRight;
        parallel.BusLaneRight = EditorGUILayout.Toggle(parallel.BusLaneRight);
        if (busOn != parallel.BusLaneRight)
        {
            rightBusPoints = null;
            SceneView.RepaintAll();
        }
        if (parallel.BusLaneRight)
        {
            int val = EditorGUILayout.IntField("From " + parallel.BusRightStart, parallel.BusRightStart);
            if (val != parallel.BusRightStart)
            {
                if (val >= 0 && val < parallel.BusRightEnd)
                {
                    rightBusPoints = null;
                    parallel.BusRightStart = val;
                    SceneView.RepaintAll();
                }
            }
            val = EditorGUILayout.IntField("To " + parallel.BusRightEnd, parallel.BusRightEnd);
            if (val != parallel.BusRightEnd)
            {
                if (val > parallel.BusRightStart && val < parallel.NodeCount)
                {
                    rightBusPoints = null;
                    parallel.BusRightEnd = val;
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.LabelField("(0-" + (parallel.NodeCount-1) + ")");
            EditorGUILayout.Separator();
        }

        EditorGUILayout.LabelField("Is the righmost lane on the LEFT side (RED) lanes a bus lane?", EditorStyles.wordWrappedLabel);
        busOn = parallel.BusLaneLeft;
        parallel.BusLaneLeft = EditorGUILayout.Toggle(parallel.BusLaneLeft);
        if (busOn != parallel.BusLaneLeft)
        {
            leftBusPoints = null;
            SceneView.RepaintAll();
        }
        if (parallel.BusLaneLeft)
        {
            int val = EditorGUILayout.IntField("From " + parallel.BusLeftStart, parallel.BusLeftStart);
            if (val != parallel.BusLeftStart)
            {
                if (val >= 0 && val < parallel.BusLeftEnd)
                {
                    leftBusPoints = null;
                    parallel.BusLeftStart = val;
                    SceneView.RepaintAll();
                }
            }
            val = EditorGUILayout.IntField("To " + parallel.BusLeftEnd, parallel.BusLeftEnd);
            if (val != parallel.BusLeftEnd)
            {
                if (val > parallel.BusLeftStart && val < parallel.NodeCount)
                {
                    leftBusPoints = null;
                    parallel.BusLeftEnd = val;
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.LabelField("(0-" + (parallel.NodeCount - 1) + ")");
        }
        DrawEditorLine();
    }

    private void LaneChangeMenu()
    {
        EditorGUILayout.LabelField("Lane change options", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        if (parallel.RightLaneCount > 1)
        {
            EditorGUILayout.LabelField("Right lane 1", EditorStyles.boldLabel);
            //from right lane 1 to lane 2
            LaneChangeMenuOptions(0);
            EditorGUILayout.LabelField("Right lane 2", EditorStyles.boldLabel);
            // from right lane 2 to lane 1
            LaneChangeMenuOptions(1);
            if (parallel.RightLaneCount > 2)
            {
                // from right lane 2 to lane 3
                LaneChangeMenuOptions(2);
                EditorGUILayout.LabelField("Right lane 3", EditorStyles.boldLabel);
                // from right lane 3 to lane 2
                LaneChangeMenuOptions(3);
            }
            EditorGUILayout.Separator();
        }
        //left lanes
        if (parallel.LeftLaneCount > 1)
        {
            EditorGUILayout.LabelField("Left lane 1", EditorStyles.boldLabel);
            //from left lane 1 to lane 2
            LaneChangeMenuOptions(4);
            EditorGUILayout.LabelField("Left lane 2", EditorStyles.boldLabel);
            //from left lane 2 to lane 1
            LaneChangeMenuOptions(5);
            if (parallel.RightLaneCount > 2)
            {
                //from left lane 2 to lane 3
                LaneChangeMenuOptions(6);
                EditorGUILayout.LabelField("Left lane 3", EditorStyles.boldLabel);
                //from left lane 3 to lane 2
                LaneChangeMenuOptions(7);
            }
        }
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void LaneChangeMenuOptions(int index)
    {
        string toggleText = "";
        switch(index)
        {
            case 0:
                toggleText = "Change to lane 2 (right) permitted?";
                break;
            case 1:
                toggleText = "Change to lane 1 (left) permitted?";
                break;
            case 2:
                toggleText = "Change to lane 3 (right) permitted?";
                break;
            case 3:
                toggleText = "Change to lane 2 (left) permitted?";
                break;
            case 4:
                toggleText = "Change to lane 2 (right) permitted?";
                break;
            case 5:
                toggleText = "Change to lane 1 (left) permitted?";
                break;
            case 6:
                toggleText = "Change to lane 3 (right) permitted?";
                break;
            case 7:
                toggleText = "Change to lane 2 (left) permitted?";
                break;
        }
        int val;
        bool v = parallel.permittedLaneChanges[index];
        v = EditorGUILayout.ToggleLeft(toggleText, v);
        if (v != parallel.permittedLaneChanges[index])
        {
            parallel.permittedLaneChanges[index] = v;
            //resets values when changed true
            if (v == true)
            {
                parallel.laneChangeStartIndex[index] = 0;
                parallel.laneChangeEndIndex[index] = parallel.NodeCount - 1;
            }
            CalculateLaneChanges();
        }
        if (v == true)
        {
            val = parallel.laneChangeStartIndex[index];
            val = EditorGUILayout.IntField("From " + parallel.laneChangeStartIndex[index], val);
            if (val != parallel.laneChangeStartIndex[index])
            {
                if (val >= 0 && val < parallel.NodeCount && val < parallel.laneChangeEndIndex[index])
                {
                    parallel.laneChangeStartIndex[index] = val;
                    CalculateLaneChanges();
                }
            }
            val = parallel.laneChangeEndIndex[index];
            val = EditorGUILayout.IntField("From " + parallel.laneChangeEndIndex[index], val);
            if (val != parallel.laneChangeEndIndex[index])
            {
                if (val >= 0 && val < parallel.NodeCount && val > parallel.laneChangeStartIndex[index])
                {
                    parallel.laneChangeEndIndex[index] = val;
                    CalculateLaneChanges();
                }
            }
            EditorGUILayout.LabelField("(" + parallel.laneChangeStartIndex[index] + "-"
                + parallel.laneChangeEndIndex[index] + ")");
        }
    }

    private List<Vector3> CalculateNodepoints(bool isRightLane, int laneIndex)
    {
        List<Vector3> pnts = new List<Vector3>();
        Vector3 pos;
        if (isRightLane)
        {
            pos = parallel.GetSegmentedPointLane(laneIndex, 0, 0f, true);
            pnts.Add(pos);
            for (int seg = 0; seg < parallel.SegmentCount; seg++)
            {
                int nodesOnSeg = parallel.GetNodesOnSegment(seg);
                for (int pnt = 1; pnt <= nodesOnSeg; pnt++)
                {
                    pos = parallel.GetSegmentedPointLane(laneIndex, seg, (float)pnt / nodesOnSeg, true);
                    pnts.Add(pos);
                }
            }
            return pnts;
        }
        else
        {
            for (int seg = parallel.SegmentCount - 1; seg >= 0; seg--)
            {
                int lseg = parallel.SegmentCount - seg - 1;
                int nodesOnSeg = parallel.GetNodesOnSegment(seg);
                for (int pnt = 0; pnt < nodesOnSeg; pnt++)
                {
                    pos = parallel.GetSegmentedPointLane(laneIndex, lseg, (float)pnt / nodesOnSeg, false);
                    pnts.Add(pos);
                }
            }
            pos = parallel.GetSegmentedPointLane(laneIndex, parallel.SegmentCount - 1, 1f, false);
            pnts.Add(pos);
            return pnts;
        }
    }

    private void CalculateLaneChanges()
    {
        List<Vector3> rLane1 = CalculateNodepoints(true, 0);
        List<Vector3> rLane2 = CalculateNodepoints(true, 1);
        List<Vector3> rLane3 = CalculateNodepoints(true, 2);
        List<Vector3> lLane1 = CalculateNodepoints(false, 0);
        List<Vector3> lLane2 = CalculateNodepoints(false, 1);
        List<Vector3> lLane3 = CalculateNodepoints(false, 2);

        laneChangePoints = new List<Vector3>();

        //r lane 1 to lane 2
        int start, end;
        if (parallel.permittedLaneChanges[0] && parallel.RightLaneCount > 1)
        {
            start = parallel.laneChangeStartIndex[0];
            end = parallel.laneChangeEndIndex[0];
            for (int i = start; i <= end; i++)
            {
                Vector3 p0 = rLane1[i];
                int ind = i + 1;
                Vector3 p1 = Vector3.zero;
                bool found = false;
                while (true)
                {
                    if (ind >= rLane2.Count)
                    {
                        break;
                    }
                    Vector3 p = rLane2[ind];
                    if (Vector3.Distance(p, p0) >= laneChangeDistance)
                    {
                        found = true;
                        p1 = p;
                        break;
                    }
                    else
                    {
                        ind++;
                    }
                }
                if (found)
                {
                    laneChangePoints.Add(p0);
                    laneChangePoints.Add(p1);
                }
            }
        }
        //r lane 2 to lane 1
        if (parallel.permittedLaneChanges[1] && parallel.RightLaneCount > 1)
        {
            start = parallel.laneChangeStartIndex[1];
            end = parallel.laneChangeEndIndex[1];
            for (int i = start; i <= end; i++)
            {
                Vector3 p0 = rLane2[i];
                int ind = i + 1;
                Vector3 p1 = Vector3.zero;
                bool found = false;
                while (true)
                {
                    if (ind >= rLane1.Count)
                    {
                        break;
                    }
                    Vector3 p = rLane1[ind];
                    if (Vector3.Distance(p, p0) >= laneChangeDistance)
                    {
                        found = true;
                        p1 = p;
                        break;
                    }
                    else
                    {
                        ind++;
                    }
                }
                if (found)
                {
                    laneChangePoints.Add(p0);
                    laneChangePoints.Add(p1);
                }
            }
        }
        //r lane 2 to lane 3
        if (parallel.permittedLaneChanges[2] && parallel.RightLaneCount > 2)
        {
            start = parallel.laneChangeStartIndex[2];
            end = parallel.laneChangeEndIndex[2];
            for (int i = start; i <= end; i++)
            {
                Vector3 p0 = rLane2[i];
                int ind = i + 1;
                Vector3 p1 = Vector3.zero;
                bool found = false;
                while (true)
                {
                    if (ind >= rLane3.Count)
                    {
                        break;
                    }
                    Vector3 p = rLane3[ind];
                    if (Vector3.Distance(p, p0) >= laneChangeDistance)
                    {
                        found = true;
                        p1 = p;
                        break;
                    }
                    else
                    {
                        ind++;
                    }
                }
                if (found)
                {
                    laneChangePoints.Add(p0);
                    laneChangePoints.Add(p1);
                }
            }
        }
        //r lane 3 to lane 2
        if (parallel.permittedLaneChanges[3] && parallel.RightLaneCount > 2)
        {
            start = parallel.laneChangeStartIndex[3];
            end = parallel.laneChangeEndIndex[3];
            for (int i = start; i <= end; i++)
            {
                Vector3 p0 = rLane3[i];
                int ind = i + 1;
                Vector3 p1 = Vector3.zero;
                bool found = false;
                while (true)
                {
                    if (ind >= rLane2.Count)
                    {
                        break;
                    }
                    Vector3 p = rLane2[ind];
                    if (Vector3.Distance(p, p0) >= laneChangeDistance)
                    {
                        found = true;
                        p1 = p;
                        break;
                    }
                    else
                    {
                        ind++;
                    }
                }
                if (found)
                {
                    laneChangePoints.Add(p0);
                    laneChangePoints.Add(p1);
                }
            }
        }
        //left lane 1 to lane 2
        if (parallel.permittedLaneChanges[4] && parallel.LeftLaneCount > 1)
        {
            start = parallel.laneChangeStartIndex[4];
            end = parallel.laneChangeEndIndex[4];
            for (int i = start; i <= end; i++)
            {
                Vector3 p0 = lLane1[i];
                int ind = i + 1;
                Vector3 p1 = Vector3.zero;
                bool found = false;
                while (true)
                {
                    if (ind >= lLane2.Count)
                    {
                        break;
                    }
                    Vector3 p = lLane2[ind];
                    if (Vector3.Distance(p, p0) >= laneChangeDistance)
                    {
                        found = true;
                        p1 = p;
                        break;
                    }
                    else
                    {
                        ind++;
                    }
                }
                if (found)
                {
                    laneChangePoints.Add(p0);
                    laneChangePoints.Add(p1);
                }
            }
        }
        //left lane 2 to lane 1
        if (parallel.permittedLaneChanges[5] && parallel.LeftLaneCount > 1)
        {
            start = parallel.laneChangeStartIndex[5];
            end = parallel.laneChangeEndIndex[5];
            for (int i = start; i <= end; i++)
            {
                Vector3 p0 = lLane2[i];
                int ind = i + 1;
                Vector3 p1 = Vector3.zero;
                bool found = false;
                while (true)
                {
                    if (ind >= lLane1.Count)
                    {
                        break;
                    }
                    Vector3 p = lLane1[ind];
                    if (Vector3.Distance(p, p0) >= laneChangeDistance)
                    {
                        found = true;
                        p1 = p;
                        break;
                    }
                    else
                    {
                        ind++;
                    }
                }
                if (found)
                {
                    laneChangePoints.Add(p0);
                    laneChangePoints.Add(p1);
                }
            }
        }
        //left lane 2 to lane 3
        if (parallel.permittedLaneChanges[6] && parallel.LeftLaneCount > 2)
        {
            start = parallel.laneChangeStartIndex[6];
            end = parallel.laneChangeEndIndex[6];
            for (int i = start; i <= end; i++)
            {
                Vector3 p0 = lLane2[i];
                int ind = i + 1;
                Vector3 p1 = Vector3.zero;
                bool found = false;
                while (true)
                {
                    if (ind >= lLane3.Count)
                    {
                        break;
                    }
                    Vector3 p = lLane3[ind];
                    if (Vector3.Distance(p, p0) >= laneChangeDistance)
                    {
                        found = true;
                        p1 = p;
                        break;
                    }
                    else
                    {
                        ind++;
                    }
                }
                if (found)
                {
                    laneChangePoints.Add(p0);
                    laneChangePoints.Add(p1);
                }
            }
        }
        //left lane 3 to lane 2
        if (parallel.permittedLaneChanges[7] && parallel.LeftLaneCount > 2)
        {
            start = parallel.laneChangeStartIndex[7];
            end = parallel.laneChangeEndIndex[7];
            for (int i = start; i <= end; i++)
            {
                Vector3 p0 = lLane3[i];
                int ind = i + 1;
                Vector3 p1 = Vector3.zero;
                bool found = false;
                while (true)
                {
                    if (ind >= lLane2.Count)
                    {
                        break;
                    }
                    Vector3 p = lLane2[ind];
                    if (Vector3.Distance(p, p0) >= laneChangeDistance)
                    {
                        found = true;
                        p1 = p;
                        break;
                    }
                    else
                    {
                        ind++;
                    }
                }
                if (found)
                {
                    laneChangePoints.Add(p0);
                    laneChangePoints.Add(p1);
                }
            }
        }
        laneChangesSet = true;
        SceneView.RepaintAll();
    }


    private void EndNodeLinkMenu()
    {
        EditorGUILayout.LabelField("End node linking", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Start direction", EditorStyles.boldLabel);
        Nodes n;
        int last = parallel.ControlPointCount - 1;
        for (int i = 0; i < parallel.RightLaneCount; i++)
        {
            n = parallel.startNodes[i];
            n = EditorGUILayout.ObjectField("Right Lane " + (i + 1) + " start", parallel.startNodes[i], typeof(Nodes), true) as Nodes;
            if (n != parallel.startNodes[i])
            {
                parallel.startNodes[i] = n;
                if (n != null)
                {
                    Vector3 pos = n.transform.position - parallel.transform.position;
                    parallel.SetLanePoint(i, 0, pos, true);
                    parallel.RecalculateLaneLength(i, 0, true);
                    //reset right bus lanes
                    rightBusPoints = null;
                    //reset lane changes
                    CalculateLaneChanges();
                    SceneView.RepaintAll();
                }
            }
        }
        EditorGUILayout.Separator();
        for (int i = 0; i < parallel.LeftLaneCount; i++)
        {
            n = parallel.endNodes[i + 3];
            n = EditorGUILayout.ObjectField("Left lane " + (i + 1) + " end", parallel.endNodes[i + 3], typeof(Nodes), true) as Nodes;
            if (n != parallel.endNodes[i + 3])
            {
                parallel.endNodes[i + 3] = n;
                if (n != null)
                {
                    Vector3 pos = n.transform.position - parallel.transform.position;
                    parallel.SetLanePoint(i, last, pos, false);
                    parallel.RecalculateLaneLength(i, last, false);
                    //reset left bus lanes
                    leftBusPoints = null;
                    CalculateLaneChanges();
                    SceneView.RepaintAll();
                }
            }
        }
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("End direction", EditorStyles.boldLabel);
        for (int i = 0; i < parallel.RightLaneCount; i++)
        {
            n = parallel.endNodes[i];
            n = EditorGUILayout.ObjectField("Right Lane " + (i + 1) + " end", parallel.endNodes[i], typeof(Nodes), true) as Nodes;
            if (n != parallel.endNodes[i])
            {
                parallel.endNodes[i] = n;
                if (n != null)
                {
                    Vector3 pos = n.transform.position - parallel.transform.position;
                    parallel.SetLanePoint(i, last, pos, true);
                    parallel.RecalculateLaneLength(i, last, true);
                    //reset left bus lanes
                    rightBusPoints = null;
                    CalculateLaneChanges();
                    SceneView.RepaintAll();
                }
            }
        }
        EditorGUILayout.Separator();
        for (int i = 0; i < parallel.LeftLaneCount; i++)
        {
            n = parallel.startNodes[i + 3];
            n = EditorGUILayout.ObjectField("Left lane " + (i + 1) + " start", parallel.startNodes[i + 3], typeof(Nodes), true) as Nodes;
            if (n != parallel.startNodes[i + 3])
            {
                parallel.startNodes[i + 3] = n;
                if (n != null)
                {
                    Vector3 pos = n.transform.position - parallel.transform.position;
                    parallel.SetLanePoint(i, 0, pos, false);
                    parallel.RecalculateLaneLength(i, 0, false);
                    //reset left bus lanes
                    leftBusPoints = null;
                    CalculateLaneChanges();
                    SceneView.RepaintAll();
                }
            }
        }
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void CreateNodesMenu()
    {
        if (GUILayout.Button("Create nodes"))
        {
            GenerateNodes();
        }
    }

    private void ShowAdvancedOptions()
    {
        if (!advancedOptionsOn)
        {
            if (GUILayout.Button("Show advanced options"))
            {
                advancedOptionsOn = true;
                SceneView.RepaintAll();
            }
            return;
        }
        if (GUILayout.Button("Hide advanced options"))
        {
            advancedOptionsOn = false;
            SceneView.RepaintAll();
        }
        EditorGUILayout.LabelField("Advanced options", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Edit selected lanes:", EditorStyles.boldLabel);
        bool s = selected[0];
        selected[0] = EditorGUILayout.Toggle("R. lane 1", selected[0]);
        if (s != selected[0])
        {
            SceneView.RepaintAll();
        }
        s = selected[1];
        selected[1] = EditorGUILayout.Toggle("R. lane 2", selected[1]);
        if (s != selected[1])
        {
            SceneView.RepaintAll();
        }
        s = selected[2];
        selected[2] = EditorGUILayout.Toggle("R. lane 3", selected[2]);
        if (s != selected[2])
        {
            SceneView.RepaintAll();
        }
        s = selected[3];
        selected[3] = EditorGUILayout.Toggle("L. lane 1", selected[3]);
        if (s != selected[3])
        {
            SceneView.RepaintAll();
        }
        s = selected[4];
        selected[4] = EditorGUILayout.Toggle("L. lane 2", selected[4]);
        if (s != selected[4])
        {
            SceneView.RepaintAll();
        }
        s = selected[5];
        selected[5] = EditorGUILayout.Toggle("L. lane 3", selected[5]);
        if (s != selected[5])
        {
            SceneView.RepaintAll();
        }
    }

    private void ShowSelectedNodeInfo()
    {
        if (selectedIndex > parallel.ControlPointCount - 1)
        {
            selectedIndex = parallel.ControlPointCount - 1;
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            if (selectedIndex < 1)
            {
                selectedIndex = parallel.ControlPointCount - 1;
            }
            else
            {
                selectedIndex--;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.LabelField("Bezier point " + selectedIndex);
        if (GUILayout.Button("Next"))
        {
            if (selectedIndex == parallel.ControlPointCount - 1)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex++;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        int pointNumber = -1;
        int leftPoint = -1;
        string nodeString, leftString;
        leftString = "";
        if (selectedIndex == -1)
        {
            nodeString = "not selected";
            leftString = "";
        }
        else
        {
            if (selectedIndex % 3 == 0)
            {
                pointNumber = selectedIndex / 3;
                nodeString = "point " + pointNumber;
                leftPoint = (parallel.ControlPointCount - 1 - selectedIndex) / 3;
                leftString = " (point " + leftPoint + ")";
            }
            else if (selectedIndex == 1)
            {
                pointNumber = 0;
                nodeString = "curve control (point 0)";
                leftPoint = (parallel.ControlPointCount - 1) / 3;
                leftString = "curve control (point " + leftPoint + ")";
            }
            else if (selectedIndex == parallel.ControlPointCount - 2)
            {
                pointNumber = (selectedIndex + 1) / 3;
                nodeString = "curve control (point " + pointNumber + ")";
                leftPoint = 0;
                leftString = "curve control (point 0)";
            }
            else if ((selectedIndex + 1) % 3 == 0)
            {
                pointNumber = (selectedIndex + 1) / 3;
                nodeString = "curve control 1 (point " + pointNumber + ")";
                leftPoint = (parallel.ControlPointCount - 1) / 3 - pointNumber;
                leftString = "curve control 2 (point " + leftPoint + ")";
            }
            else
            {
                pointNumber = (selectedIndex - 1) / 3;
                nodeString = "curve control 2 (point " + pointNumber + ")";
                leftPoint = (parallel.ControlPointCount - 1) / 3 - pointNumber;
                leftString = "curve control 1 (point " + leftPoint + ")";
            }
        }
        EditorGUILayout.LabelField("Selected node: " + nodeString);
        EditorGUILayout.LabelField("Left side: " + leftString);
        if (pointNumber != -1)
        {
            EditorGUILayout.LabelField("Point " + pointNumber, EditorStyles.boldLabel);
            if (pointNumber != 0)
            {
                EditorGUILayout.LabelField("Spacing options", EditorStyles.boldLabel);
                //right lane spacings
                for (int i = 0; i < parallel.RightLaneCount; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    rSpacing[i] = EditorGUILayout.FloatField("R. lane " + (i+1) 
                        + " current: " + parallel.GetLaneSpacing(i, selectedIndex, true), rSpacing[i]);
                    if (GUILayout.Button("Change"))
                    {
                        if (rSpacing[i] != parallel.GetLaneSpacing(i, selectedIndex, true))
                        {
                            float changeAmount = rSpacing[i]
                                - parallel.GetLaneSpacing(i, selectedIndex, true);
                            parallel.SetLaneSpacing(i, selectedIndex, rSpacing[i], true);
                            int index = pointNumber * 3;
                            Vector3 right = GeneralDirection.DirectionRight(parallel.GetDirection((float)index / parallel.CurveCount));
                            Vector3 moved = changeAmount * right;
                            for (int j = i; j < 3; j++)
                            {
                                Vector3 newPoint = moved + parallel.GetLanePoint(j, index, true);
                                parallel.SetLanePoint(j, index, newPoint, true);
                                parallel.RecalculateLaneLength(j, index, true);
                            }

                            SceneView.RepaintAll();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                //left lane spacings
                for (int i = 0; i < parallel.LeftLaneCount; i++)
                {
                    int index = leftPoint * 3;
                    EditorGUILayout.BeginHorizontal();
                    lSpacing[i] = EditorGUILayout.FloatField("L. lane " + (i + 1)
                        + " current: " + parallel.GetLaneSpacing(i, index, false), lSpacing[i]);
                    if (GUILayout.Button("Change"))
                    {
                        if (lSpacing[i] != parallel.GetLaneSpacing(i, index, false))
                        {
                            float changeAmount = lSpacing[i] - parallel.GetLaneSpacing(i, index, false);
                            parallel.SetLaneSpacing(i, index, lSpacing[i], false);
                            Vector3 right = GeneralDirection.DirectionRight(parallel.GetDirection((float)pointNumber * 3 / parallel.CurveCount));
                            Vector3 moved = - changeAmount * right;
                            for (int j = i; j < 3; j++)
                            {
                                Vector3 newPoint = moved + parallel.GetLanePoint(j, index, false);
                                parallel.SetLanePoint(j, index, newPoint, false);
                                parallel.RecalculateLaneLength(j, index, false);
                            }

                            SceneView.RepaintAll();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }

    private void ShowAngleChangeOption()
    {
        if (selectedIndex > 1)
        {
            EditorGUILayout.LabelField("Change angle (Point " + (selectedIndex + 1)/3 + ")", EditorStyles.boldLabel);
            angle = EditorGUILayout.FloatField("Angle", angle);
            if (GUILayout.Button("Add angle"))
            {
                if (angle != 0f)
                {
                    Vector3 deg = new Vector3(0f, angle, 0f);
                    int pivotIndex = ((selectedIndex + 1) / 3) * 3;
                    Vector3 pivot = parallel.GetControlPoint(pivotIndex);
                    //move guide spline's control points
                    Vector3 dir = parallel.GetControlPoint(pivotIndex - 1) - pivot;
                    dir = Quaternion.Euler(deg) * dir;
                    parallel.SetControlPoint(pivotIndex - 1, dir + pivot);

                    for (int i = 0; i < 3; i++)
                    {
                        //right lanes
                        dir = parallel.GetLanePoint(i, pivotIndex, true) - pivot;
                        dir = Quaternion.Euler(deg) * dir;
                        parallel.SetLanePoint(i, pivotIndex, dir + pivot, true);
                        dir = parallel.GetLanePoint(i, pivotIndex - 1, true) - pivot;
                        dir = Quaternion.Euler(deg) * dir;
                        parallel.SetLanePoint(i, pivotIndex - 1, dir + pivot, true);
                        //left lanes
                        int leftIndex = parallel.ControlPointCount - 1 - pivotIndex;
                        dir = parallel.GetLanePoint(i, leftIndex, false) - pivot;
                        dir = Quaternion.Euler(deg) * dir;
                        parallel.SetLanePoint(i, leftIndex, dir + pivot, false);
                        dir = parallel.GetLanePoint(i, leftIndex + 1, false) - pivot;
                        dir = Quaternion.Euler(deg) * dir;
                        parallel.SetLanePoint(i, leftIndex + 1, dir + pivot, false);

                    }
                }
                SceneView.RepaintAll();
            }
        }
    }

    private void InitializeParallel()
    {
        //parallel.Reset();
        Vector3 right = new Vector3(directionVector.z, directionVector.y, -directionVector.x);
        Vector3 gp0, gp1, gp2, gp3;
        gp0 = Vector3.zero;
        gp1 = guidePoints[1] - guidePoints[0];
        gp2 = guidePoints[2] - guidePoints[0];
        gp3 = guidePoints[3] - guidePoints[0];
        // Note: node points (0, 3) must be set BEFORE the control points (1, 2)
        //parallel.transform.position = gp0;
        parallel.transform.position += right * adjustment;
        parallel.SetControlPoint(0, gp0);
        parallel.SetControlPoint(3, gp3);
        parallel.SetControlPoint(1, gp1);
        parallel.SetControlPoint(2, gp2);
        parallel.RecalculateLength(0);

        parallel.RightLaneCount = rightLaneCount;
        parallel.LeftLaneCount = leftLaneCount;
        float space = 0f;
        for (int i = 0; i < rightLaneCount; i++)
        {
            space += rSpacing[i];
            parallel.SetLanePoint(i, 0, gp0 + right * space, true);
            parallel.SetLanePoint(i, 3, gp3 + right * space, true);
            parallel.SetLanePoint(i, 1, gp1 + right * space, true);
            parallel.SetLanePoint(i, 2, gp2 + right * space, true);
            parallel.SetLaneSpacing(i, 0, rSpacing[i], true);
            parallel.SetLaneSpacing(i, 3, rSpacing[i], true);
            parallel.RecalculateLaneLength(i, 0, true);
        }
        space = 0f;
        for (int i = 0; i < leftLaneCount; i++)
        {
            space += lSpacing[i];
            //opposite direction
            parallel.SetLanePoint(i, 3, gp0 - right * space, false);
            parallel.SetLanePoint(i, 0, gp3 - right * space, false);
            parallel.SetLanePoint(i, 2, gp1 - right * space, false);
            parallel.SetLanePoint(i, 1, gp2 - right * space, false);
            parallel.SetLaneSpacing(i, 0, lSpacing[i], false);
            parallel.SetLaneSpacing(i, 3, lSpacing[i], false);
            parallel.RecalculateLaneLength(i, 0, false);
        }
        adjustment = 0f;
        parallel.Initialized = true;
        SceneView.RepaintAll();
    }

    private void ShowSetupInstructions()
    {
        EditorGUILayout.LabelField("Basic settings:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Instructions:");
        EditorGUILayout.LabelField("1. Set lane counts (0 - 3) for both directions.");
        EditorGUILayout.LabelField("2. Set initial spacing between the lanes. The guiding spline is placed " +
            "in the middle between right side and left side lanes. Spacings are numbered from the centre " +
            "to the sides.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("3. You can anchor this road section to an existing nodepoint using " +
            "the object link field of the respective lane you want to link. (You can link the rest of the lanes " +
            "later on.) The direction will be derived from the linked nodepoint.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("4. If none of the lanes are linked to an existing node, you can set the " +
            "initial direction of the lanes. Yellow line marks the guiding spline and blue lines and red lines " +
            "right side lanes and left side lanes respectively. You can also set the length of the (first) " +
            "spline(s), adjust their uniform positioning (left - right) and set an uniform spacing value for " +
            "each lane.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("5. When you are happy with the settings, press 'Setup done'-button and these " +
            "initial settings are used for creating a set of parallel splines.", EditorStyles.wordWrappedLabel);
        DrawEditorLine();
    }

    private void ShowEditInstructions()
    {
        EditorGUILayout.LabelField("Edit mode instructions:", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void ShowNodeSetupInstructions()
    {
        EditorGUILayout.LabelField("Node set-up instructions:", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        DrawEditorLine();
    }

    private void ShowLaneCountSelection()
    {
        EditorGUILayout.LabelField("1. Lanes:", EditorStyles.boldLabel);
        rightLaneCount = EditorGUILayout.IntSlider(new GUIContent("Right lanes count", "0 - 3")
            , rightLaneCount, 0, 3);
        leftLaneCount = EditorGUILayout.IntSlider(new GUIContent("Left lanes count", "opposite direction, 0 - 3")
            , leftLaneCount, 0, 3);
        if (rightLaneCount != 0 || leftLaneCount != 0)
        {
            if (GUILayout.Button("Done"))
            {
                laneCountSet = true;
            }
        }
        DrawEditorLine();
    }

    private void ShowLaneSetupSelection()
    {
        EditorGUILayout.LabelField("2. - 3. Lane spacing and linking",
                EditorStyles.boldLabel);
        ShowRightLaneSelection();
        ShowLeftLaneSelection();
    }

    private void ShowRightLaneSelection()
    {
        if (rightLaneCount > 0)
        {
            EditorGUILayout.LabelField("Right side lanes: " + rightLaneCount + " lanes",
                EditorStyles.boldLabel);
        }
        for (int i = 0; i < rightLaneCount; i++)
        {
            EditorGUILayout.LabelField("Lane " + (i + 1) + " (right)");
            float sp = EditorGUILayout.Slider("Spacing " + (i + 1), rSpacing[i], 0f, 10f);
            if (sp != rSpacing[i])
            {
                rSpacing[i] = sp;
                SceneView.RepaintAll();
            }
            if (linkedToNode == false)
            {
                Nodes n = null;
                n = EditorGUILayout.ObjectField("Object link", parallel.startNodes[i], typeof(Nodes), true) as Nodes;
                if (n != null)
                {
                    parallel.startNodes[i] = n;
                    linkedToNode = true;
                    if (n.OutNodes.Length > 0)
                    {
                        directionVector = (n.OutNodes[0].transform.position - n.transform.position).normalized;
                        directionSet = true;
                    }
                    else
                    {
                        directionSet = false;
                    }
                }
            }
            else
            {
                if (parallel.startNodes[i] != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Linked: " + parallel.startNodes[i].gameObject.name, EditorStyles.whiteLabel);
                    if (GUILayout.Button("Remove"))
                    {
                        parallel.startNodes[i] = null;
                        linkedToNode = false;
                        directionVector = GeneralDirection.DirectionVector(generalDirection);
                        SceneView.RepaintAll();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        DrawEditorLine();
    }

    private void ShowLeftLaneSelection()
    {
        if (leftLaneCount > 0)
        {
            EditorGUILayout.LabelField("Left side lanes: " + leftLaneCount + " lanes",
                EditorStyles.boldLabel);
        }
        for (int i = 0; i < leftLaneCount; i++)
        {
            EditorGUILayout.LabelField("Lane " + (i + 1) + " (left)");
            float sp = EditorGUILayout.Slider("Spacing " + (i + 1), lSpacing[i], 0f, 10f);
            if (sp != lSpacing[i])
            {
                lSpacing[i] = sp;
                SceneView.RepaintAll();
            }
            if (linkedToNode == false)
            {
                Nodes n = null;
                n = EditorGUILayout.ObjectField("Object link", parallel.endNodes[i + 3], typeof(Nodes), true) as Nodes;
                if (n)
                {
                    linkedToNode = true;
                    parallel.endNodes[i + 3] = n;
                    if (n.OutNodes.Length > 0)
                    {
                        directionVector = (n.transform.position - n.OutNodes[0].transform.position).normalized;
                        SceneView.RepaintAll();
                        directionSet = true;
                    }
                    else
                    {
                        directionSet = false;
                    }
                }
            }
            else
            {
                if (parallel.endNodes[i + 3] != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Linked: " + parallel.endNodes[i + 3].gameObject.name, EditorStyles.whiteLabel);
                    if (GUILayout.Button("Remove"))
                    {
                        parallel.endNodes[i + 3] = null;
                        linkedToNode = false;
                        directionVector = GeneralDirection.DirectionVector(generalDirection);
                        SceneView.RepaintAll();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        DrawEditorLine();
    }

    private void ShowSpacingSelection()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("4. Uniform spacing", EditorStyles.boldLabel);

        if (!linkedToNode)
        {
            Directions d = Directions.East;
            d = (Directions) EditorGUILayout.EnumPopup("General direction", generalDirection);
            if (d != generalDirection)
            {
                generalDirection = d;
                directionVector = GeneralDirection.DirectionVector(generalDirection);
                SceneView.RepaintAll();
            }
        }
        float length = EditorGUILayout.Slider(new GUIContent("Length", "Length of (the first) spline"),
        defaultLength, 1f, 500f);
        if (length != defaultLength)
        {
            defaultLength = length;
            SceneView.RepaintAll();
        }
        if (!linkedToNode)
        {
            float adj = EditorGUILayout.Slider(new GUIContent("Adustment", "Adust left-right"),
            adjustment, -5f, 5f);
            if (adj != adjustment)
            {
                adjustment = adj;
                SceneView.RepaintAll();
            }
        }

        spacingOverride = EditorGUILayout.Slider(new GUIContent("Uniform spacing", "Override all spacing values"),
        spacingOverride, 0f, 10f);
        if (GUILayout.Button("Override spacing values"))
        {
            for (int i = 0; i < 3; i++)
            {
                lSpacing[i] = spacingOverride;
                rSpacing[i] = spacingOverride;
                SceneView.RepaintAll();
            }
        }
        DrawEditorLine();
    }

    private void ResetValues()
    {
        for (int i=0; i < 3; i++)
        {
            lSpacing[i] = 0f;
            rSpacing[i] = 0f;
            parallel.startNodes[i] = null;
            parallel.startNodes[i + 3] = null;
            parallel.endNodes[i] = null;
            parallel.endNodes[i + 3] = null;
            guidePoints[i] = parallel.transform.position;
        }
        guidePoints[3] = parallel.transform.position;
        adjustment = 0f;
        linkedToNode = false;
        laneCountSet = false;
        leftLaneCount = 0;
        rightLaneCount = 0;
        spacingOverride = 1f;
        directionVector = GeneralDirection.DirectionVector(generalDirection);
        directionSet = false;
    }

    private void OnSceneGUI()
    {
        if (parallel != null)
        {
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(parallel.transform.localPosition, new Vector3(0f, 1f, 0f),
                0.01f*Vector3.Distance(parallel.transform.position,
                SceneView.lastActiveSceneView.camera.transform.position));
        }
        else
        {
            parallel = target as ParallelBezierSplines;
        }
        handleTransform = parallel.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;
        if (!parallel.Initialized)
        {
            DrawBasicSettingsObjects();
        }
        else
        {
            if (showNodeLabels)
            {
                DrawNodeLabels();
            }
            DrawParallelBezier();
            DrawSegmentLines();
            if (parallel.LanesSet && previewNodes)
            {
                DrawNodeVisualization();
            }
            if (parallel.NodesSet)
            {
                DrawBusLanes();
            }
            if (laneChangesSet)
            {
                DrawLaneChanges();
            }
        }
    }

    private void DrawParallelBezier()
    {
        Vector3 p0, p1, p2, p3;
        p0 = ShowPoint(0);
        for (int i = 1; i < parallel.ControlPointCount; i +=3)
        {
            p1 = ShowPoint(i);
            p2 = ShowPoint(i + 1);
            p3 = ShowPoint(i + 2);

            Handles.color = Color.red;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.yellow, null, 4f);
            p0 = p3;
        }
        for (int lanes = 0; lanes < parallel.RightLaneCount; lanes++)
        {
            p0 = ShowPointRight(lanes, 0);
            for (int i = 1; i < parallel.ControlPointCount; i += 3)
            {
                p1 = ShowPointRight(lanes, i);
                p2 = ShowPointRight(lanes, i + 1);
                p3 = ShowPointRight(lanes, i + 2);
                Handles.color = Color.magenta;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);
                Handles.DrawBezier(p0, p3, p1, p2, Color.blue, null, 2f);
                p0 = p3;
            }
        }

        for (int lanes = 0; lanes < parallel.LeftLaneCount; lanes++)
        {
            p0 = ShowPointLeft(lanes, 0);
            for (int i = 1; i < parallel.ControlPointCount; i += 3)
            {
                p1 = ShowPointLeft(lanes, i);
                p2 = ShowPointLeft(lanes, i + 1);
                p3 = ShowPointLeft(lanes, i + 2);

                Handles.color = Color.magenta;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);
                Handles.DrawBezier(p0, p3, p1, p2, Color.red, null, 2f);
                p0 = p3;
            }
        }
    }

    private void DrawSegmentLines()
    {
        Vector3 leftPoint, rightPoint, labelPoint;
        Vector3 labelAdjust = new Vector3(0f, 0f, 1f);
        for (int i = 0; i < parallel.ControlPointCount; i += 3)
        {
            int leftI = parallel.ControlPointCount -  1 - i;
            leftPoint = handleTransform.TransformPoint(parallel.GetControlPoint(i));
            rightPoint = handleTransform.TransformPoint(parallel.GetControlPoint(i));
            labelPoint = handleTransform.TransformPoint(parallel.GetControlPoint(i) + labelAdjust);
            switch (parallel.LeftLaneCount)
            {
                case 1:
                    leftPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(0, leftI, false));
                    labelPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(0, leftI, false)
                        + labelAdjust);
                    break;
                case 2:
                    leftPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(1, leftI, false));
                    labelPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(1, leftI, false)
                        + labelAdjust);
                    break;
                case 3:
                    leftPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(2, leftI, false));
                    labelPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(2, leftI, false)
                        + labelAdjust);
                    break;
            }
            switch (parallel.RightLaneCount)
            {
                case 1:
                    rightPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(0, i, true));
                    break;
                case 2:
                    rightPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(1, i, true));
                    break;
                case 3:
                    rightPoint = handleTransform.TransformPoint(
                        parallel.GetLanePoint(2, i, true));
                    break;
            }
            labelPoint.x -= 6f;
            labelPoint.z += 1f;
            Handles.color = Color.white;
            Handles.DrawLine(leftPoint, rightPoint);
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            Handles.Label(labelPoint, "Pt. " + i/3, style);
        }
    }

    private void DrawNodeVisualization()
    {
        Handles.color = Color.yellow;
        float handleSize = 0.3f;
        Vector3 pos;
        //Start poits for each lane
        for (int lane = 0; lane < parallel.RightLaneCount; lane++)
        {
            pos = parallel.GetSegmentedPointLane(lane, 0, 0f, true);
            Handles.RadiusHandle(Quaternion.identity, pos, handleSize);
        }
        for (int lane = 0; lane < parallel.LeftLaneCount; lane++)
        {
            // points on left lane are inversed for easier handling
            pos = parallel.GetSegmentedPointLane(lane, parallel.SegmentCount - 1, 1f, false);
            Handles.RadiusHandle(Quaternion.identity, pos, handleSize);
        }
        for (int seg = 0; seg < parallel.SegmentCount; seg++)
        {
            int lSeg = parallel.SegmentCount - seg - 1;
            int nodesOnSeg = parallel.GetNodesOnSegment(seg);
            //right lanes
            for (int lane = 0; lane < parallel.RightLaneCount; lane++)
            {
                for (int pnt = 1; pnt <= nodesOnSeg; pnt++)
                {
                    pos = parallel.GetSegmentedPointLane(lane, seg, (float)pnt / nodesOnSeg, true);
                    Handles.RadiusHandle(Quaternion.identity, pos, handleSize);
                }
            }
            //left lanes
            for (int lane = 0; lane < parallel.LeftLaneCount; lane++)
            {
                for (int pnt = 0; pnt < nodesOnSeg; pnt++)
                {
                    pos = parallel.GetSegmentedPointLane(lane, lSeg, (float)pnt / nodesOnSeg, false);
                    Handles.RadiusHandle(Quaternion.identity, pos, handleSize);
                }
            }
        }
    }

    private void DrawBusLanes()
    {
        if (parallel.BusLaneRight)
        {
            if (rightBusPoints == null)
            {
                int lane = parallel.RightLaneCount - 1;
                int arraySize = parallel.BusRightEnd - parallel.BusRightStart + 1;
                int pntsLeft = arraySize;
                rightBusPoints = new Vector3[arraySize];
                int index = 0;
                if (parallel.BusRightStart == 0)
                {
                    rightBusPoints[index] = parallel.GetSegmentedPointLane(lane, 0, 0f, true);
                    pntsLeft--;
                    index++;
                }
                //find start segment and node
                int segment = 0;
                int node = parallel.BusRightStart;
                if (node == 0)
                {
                    node++;
                }
                while (node > parallel.GetNodesOnSegment(segment))
                {
                    node -= parallel.GetNodesOnSegment(segment);
                    segment++;
                }
                int pntsOnSegment = parallel.GetNodesOnSegment(segment);
                while(pntsLeft > 0)
                {
                    if (node > parallel.GetNodesOnSegment(segment))
                    {
                        node -= parallel.GetNodesOnSegment(segment);
                        segment++;
                        pntsOnSegment = parallel.GetNodesOnSegment(segment);
                    }
                    rightBusPoints[index] = parallel.GetSegmentedPointLane(lane, segment,
                        (float)node / pntsOnSegment, true);
                    pntsLeft--;
                    node++;
                    index++;
                }
            }
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4f, rightBusPoints);
        }
        if (parallel.BusLaneLeft)
        {
            // Generate points only when changes are made
            if (leftBusPoints == null)
            {
                int lane = parallel.LeftLaneCount - 1;
                int arraySize = parallel.BusLeftEnd - parallel.BusLeftStart + 1;
                int pntsLeft = arraySize;
                leftBusPoints = new Vector3[arraySize];
                int arrayIndex = 0;

                if (parallel.BusLeftStart == 0)
                {
                    leftBusPoints[0] = parallel.GetSegmentedPointLane(lane, 0, 0f, false);
                    arrayIndex++;
                    pntsLeft--;
                }
                List<int> nodesInversed = new List<int>();
                for (int i = parallel.SegmentCount - 1; i >=0; i--)
                {
                    nodesInversed.Add(parallel.GetNodesOnSegment(i));
                }
                int segment = 0;
                int node = parallel.BusLeftStart;
                if (node==0)
                {
                    node++;
                }
                // finding start segment
                while (node > nodesInversed[segment])
                {
                    node -= nodesInversed[segment];
                    segment++;
                }
                int pntsOnSegment = nodesInversed[segment];
                while (pntsLeft > 0)
                {
                    if (node > nodesInversed[segment])
                    {
                        node -= nodesInversed[segment];
                        segment++;
                        pntsOnSegment = nodesInversed[segment];
                        
                    }
                    leftBusPoints[arrayIndex] = parallel.GetSegmentedPointLane(lane, segment,
                        (float)node / pntsOnSegment, false);
                    arrayIndex++;
                    node++;
                    pntsLeft--;

                }
            }
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4f, leftBusPoints);
        }
    }

    private void DrawLaneChanges()
    {
        for (int i = 0; i < laneChangePoints.Count; i += 2)
        {
            Handles.color = Color.green;
            Handles.DrawLine(laneChangePoints[i], laneChangePoints[i + 1]);
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(parallel.GetControlPoint(index));

        if (advancedOptionsOn || parallel.LanesSet)
        {
            return point;
        }
        float size = HandleUtility.GetHandleSize(point);
        if (index == 0)
        {
            size *= 2f; // 1st node bigger
        }
        Handles.color = Color.cyan;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedIndex = index;
            Repaint(); // refresh inspector
        }
        if (selectedIndex == index && index != 0)
        {
            Event e = Event.current;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            Vector3 dRight = GeneralDirection.DirectionRight(parallel.GetDirection((float)index / parallel.CurveCount));
            Vector3 p = handleTransform.InverseTransformPoint(point);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(parallel, "MovePoint");
                EditorUtility.SetDirty(parallel);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Vector3[] leftMoves = new Vector3[3];
                Vector3[] rightMoves = new Vector3[3];

                int leftIndex = parallel.ControlPointCount - 1 - index;
                leftMoves[0] = parallel.GetLanePoint(0, leftIndex, false)
                    - parallel.GetControlPoint(index);
                leftMoves[1] = parallel.GetLanePoint(1, leftIndex, false)
                    - parallel.GetControlPoint(index);
                leftMoves[2] = parallel.GetLanePoint(2, leftIndex, false)
                    - parallel.GetControlPoint(index);
                rightMoves[0] = parallel.GetLanePoint(0, index, true)
                    - parallel.GetControlPoint(index);
                rightMoves[1] = parallel.GetLanePoint(1, index, true)
                    - parallel.GetControlPoint(index);
                rightMoves[2] = parallel.GetLanePoint(2, index, true)
                    - parallel.GetControlPoint(index);

                parallel.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
                parallel.RecalculateLength(selectedIndex);

                if (index % 3 != 0)
                {
                    leftMoves[0] -= parallel.GetLanePoint(0, leftIndex, false);
                    leftMoves[0] += parallel.GetControlPoint(index);
                    leftMoves[1] -= parallel.GetLanePoint(1, leftIndex, false);
                    leftMoves[1] += parallel.GetControlPoint(index);
                    leftMoves[2] -= parallel.GetLanePoint(2, leftIndex, false);
                    leftMoves[2] += parallel.GetControlPoint(index);
                    rightMoves[0] -= parallel.GetLanePoint(0, index, true);
                    rightMoves[0] += parallel.GetControlPoint(index);
                    rightMoves[1] -= parallel.GetLanePoint(1, index, true);
                    rightMoves[1] += parallel.GetControlPoint(index);
                    rightMoves[2] -= parallel.GetLanePoint(2, index, true);
                    rightMoves[2] += parallel.GetControlPoint(index);
                }

                for (int i = 0; i < parallel.RightLaneCount; i++)
                {
                    
                    parallel.AdjustLane(i, index, rightMoves[i], true);
                }

                for (int i = 0; i < parallel.LeftLaneCount; i++)
                {
                    parallel.AdjustLane(i, index, leftMoves[i], false);
                }
            }
        }
        return point;
    }

    private Vector3 ShowPointRight(int lane, int index)
    {
        Vector3 point = handleTransform.TransformPoint(parallel.GetLanePoint(lane, index, true));
        if (!advancedOptionsOn)
        {
            return point;
        }
        if (selected[lane])
        {
            float size = HandleUtility.GetHandleSize(point);
            if (index == 0)
            {
                size *= 2f; // 1st node bigger
            }
            Handles.color = Color.cyan;
            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
            {
                selectedIndex = index;
                Repaint(); // refresh inspector
            }
            if (selectedIndex == index && index != 0 && selectedIndex % 3 != 0)
            {
                Event e = Event.current;
                var controlID = GUIUtility.GetControlID(FocusType.Passive);
                var eventType = e.GetTypeForControl(controlID);

                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, handleRotation);
                Vector3 dRight = GeneralDirection.DirectionRight
                    (parallel.GetDirectionLane(
                        lane, (float)index / parallel.CurveCount, true));
                Vector3 p = handleTransform.InverseTransformPoint(point);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(parallel, "MovePoint");
                    EditorUtility.SetDirty(parallel);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    Vector3[] leftMoves = new Vector3[3];
                    Vector3[] rightMoves = new Vector3[3];

                    int leftIndex = parallel.ControlPointCount - 1 - index;
                    leftMoves[0] = parallel.GetLanePoint(0, leftIndex, false)
                        - parallel.GetLanePoint(lane, index, true);
                    leftMoves[1] = parallel.GetLanePoint(1, leftIndex, false)
                        - parallel.GetLanePoint(lane, index, true);
                    leftMoves[2] = parallel.GetLanePoint(2, leftIndex, false)
                        - parallel.GetLanePoint(lane, index, true);
                    rightMoves[0] = parallel.GetLanePoint(0, index, true)
                        - parallel.GetLanePoint(lane, index, true);
                    rightMoves[1] = parallel.GetLanePoint(1, index, true)
                        - parallel.GetLanePoint(lane, index, true);
                    rightMoves[2] = parallel.GetLanePoint(2, index, true)
                        - parallel.GetLanePoint(lane, index, true);

                    parallel.SetLanePoint(lane, index, handleTransform.InverseTransformPoint(point), true);
                    parallel.RecalculateLaneLength(lane, selectedIndex, true);
                    if (index % 3 != 0)
                    {
                        leftMoves[0] -= parallel.GetLanePoint(0, leftIndex, false);
                        leftMoves[0] += parallel.GetLanePoint(lane, index, true);
                        leftMoves[1] -= parallel.GetLanePoint(1, leftIndex, false);
                        leftMoves[1] += parallel.GetLanePoint(lane, index, true);
                        leftMoves[2] -= parallel.GetLanePoint(2, leftIndex, false);
                        leftMoves[2] += parallel.GetLanePoint(lane, index, true);
                        rightMoves[0] -= parallel.GetLanePoint(0, index, true);
                        rightMoves[0] += parallel.GetLanePoint(lane, index, true);
                        rightMoves[1] -= parallel.GetLanePoint(1, index, true);
                        rightMoves[1] += parallel.GetLanePoint(lane, index, true);
                        rightMoves[2] -= parallel.GetLanePoint(2, index, true);
                        rightMoves[2] += parallel.GetLanePoint(lane, index, true);
                    }

                    for (int i = 0; i < parallel.RightLaneCount; i++)
                    {
                        if (lane != i && selected[i]==true)
                        {
                            parallel.AdjustLane(i, index, rightMoves[i], true);
                        }
                    }

                    for (int i = 0; i < parallel.LeftLaneCount; i++)
                    {
                        if (selected[i + 3])
                        {
                            parallel.AdjustLane(i, index, leftMoves[i], false);
                        }
                    }
                }
            }
        }

        return point;
    }

    private Vector3 ShowPointLeft(int lane, int index)
    {
        int leftIndex = parallel.ControlPointCount - 1 - index;
        Vector3 point = handleTransform.TransformPoint(parallel.GetLanePoint(lane, leftIndex, false));
        if (!advancedOptionsOn)
        {
            return point;
        }
        //*****************
        if (selected[lane + 3])
        {
            float size = HandleUtility.GetHandleSize(point);
            if (index == 0)
            {
                size *= 2f; // 1st node bigger
            }
            Handles.color = Color.cyan;
            if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
            {
                selectedIndex = leftIndex;
                Repaint(); // refresh inspector
            }
            
            if (selectedIndex == leftIndex && selectedIndex % 3 != 0)
            {
                Event e = Event.current;
                var controlID = GUIUtility.GetControlID(FocusType.Passive);
                var eventType = e.GetTypeForControl(controlID);

                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, handleRotation);
                Vector3 dRight = GeneralDirection.DirectionRight(
                    parallel.GetDirectionLane(
                        lane, (float)selectedIndex / parallel.CurveCount, true));
                Vector3 p = handleTransform.InverseTransformPoint(point);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(parallel, "MovePoint");
                    EditorUtility.SetDirty(parallel);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    Vector3[] leftMoves = new Vector3[3];
                    Vector3[] rightMoves = new Vector3[3];

                    leftMoves[0] = parallel.GetLanePoint(0, leftIndex, false)
                        - parallel.GetLanePoint(lane, selectedIndex, false);
                    leftMoves[1] = parallel.GetLanePoint(1, leftIndex, false)
                        - parallel.GetLanePoint(lane, selectedIndex, false);
                    leftMoves[2] = parallel.GetLanePoint(2, leftIndex, false)
                        - parallel.GetLanePoint(lane, selectedIndex, false);
                    rightMoves[0] = parallel.GetLanePoint(0, index, true)
                        - parallel.GetLanePoint(lane, selectedIndex, false);
                    rightMoves[1] = parallel.GetLanePoint(1, index, true)
                        - parallel.GetLanePoint(lane, selectedIndex, false);
                    rightMoves[2] = parallel.GetLanePoint(2, index, true)
                        - parallel.GetLanePoint(lane, selectedIndex, false);

                    parallel.SetLanePoint(lane, selectedIndex, handleTransform.InverseTransformPoint(point), false);
                    parallel.RecalculateLaneLength(lane, selectedIndex, false);
                    if (index % 3 != 0)
                    {
                        leftMoves[0] -= parallel.GetLanePoint(0, leftIndex, false);
                        leftMoves[0] += parallel.GetLanePoint(lane, leftIndex, false);
                        leftMoves[1] -= parallel.GetLanePoint(1, leftIndex, false);
                        leftMoves[1] += parallel.GetLanePoint(lane, leftIndex, false);
                        leftMoves[2] -= parallel.GetLanePoint(2, leftIndex, false);
                        leftMoves[2] += parallel.GetLanePoint(lane, leftIndex, false);
                        rightMoves[0] -= parallel.GetLanePoint(0, index, true);
                        rightMoves[0] += parallel.GetLanePoint(lane, leftIndex, false);
                        rightMoves[1] -= parallel.GetLanePoint(1, index, true);
                        rightMoves[1] += parallel.GetLanePoint(lane, leftIndex, false);
                        rightMoves[2] -= parallel.GetLanePoint(2, index, true);
                        rightMoves[2] += parallel.GetLanePoint(lane, leftIndex, false);
                    }

                    for (int i = 0; i < parallel.RightLaneCount; i++)
                    {
                        if (selected[i])
                        {
                            parallel.AdjustLane(i, index, rightMoves[i], true);
                        }
                    }

                    for (int i = 0; i < parallel.LeftLaneCount; i++)
                    {
                        if (lane != i && selected[i + 3])
                        {
                            parallel.AdjustLane(i, index, leftMoves[i], false);
                        }
                    }
                }
            }
        }
        //*******************
        
        return point;
    }

    private void DrawNodeLabels()
    {
        if (allNodes == null)
        {
            allNodes = GameObject.FindObjectsOfType<Nodes>();
        }
        Handles.color = Color.black;
        if ((linkedToNode == false && laneCountSet) || showNodeLabels)
        {
            foreach (Nodes n in allNodes)
            {
                Handles.Label(n.transform.position, n.gameObject.name);
            }
            SceneView.RepaintAll();
        }
    }

    private void DrawBasicSettingsObjects()
    {
        if (!directionSet)
        {
            SetStartDirection();
        }
        SetStartPositions();
        Handles.DrawBezier(guidePoints[0], guidePoints[3], guidePoints[1], guidePoints[2], Color.yellow, null, 4f);
        Vector3 right = new Vector3(directionVector.z, directionVector.y, -directionVector.x);
        float dist = 0f;
        Vector3 p0, p1, p2, p3;
        for (int i = 0; i < rightLaneCount; i++)
        {
            dist += rSpacing[i];
            p0 = guidePoints[0] + dist * right;
            p1 = guidePoints[1] + dist * right;
            p2 = guidePoints[2] + dist * right;
            p3 = guidePoints[3] + dist * right;
            Handles.DrawBezier(p0, p3, p1, p2, Color.blue, null, 2f);
        }
        dist = 0f;
        for (int i = 0; i < leftLaneCount; i++)
        {
            dist += lSpacing[i];
            p0 = guidePoints[0] - dist * right;
            p1 = guidePoints[1] - dist * right;
            p2 = guidePoints[2] - dist * right;
            p3 = guidePoints[3] - dist * right;
            Handles.DrawBezier(p0, p3, p1, p2, Color.red, null, 2f);
        }

    }

    private void SetStartDirection()
    {
        if (linkedToNode)
        {
            for (int i = 0; i < rightLaneCount; i++)
            {
                if (parallel.startNodes[i] != null)
                {
                    directionSet = true;
                    Vector3 dir;
                    bool hasDir = parallel.startNodes[i].GetDirectionIn(out dir);
                    if (hasDir)
                    {
                        directionVector = dir;
                    }
                    break;
                }
            }
            if (!directionSet)
            {
                for (int i = 0; i < leftLaneCount; i++)
                {
                    if (parallel.endNodes[i + 3] != null)
                    {
                        if (parallel.endNodes[i + 3].OutNodes.Length > 0)
                        {
                            if (parallel.endNodes[i + 3].GetDirectionOut(out Vector3 dir))
                            {
                                directionVector = dir;
                                directionSet = true;
                            }
                        }
                        break;
                    }
                }
            }
        }
        if (!directionSet)
        {
            directionVector = GeneralDirection.DirectionVector(generalDirection);
        }
    }

    private void SetStartPositions()
    {
        if (parallel == null)
        {
            parallel = target as ParallelBezierSplines;
        }
        if (guidePoints == null || guidePoints.Length != 4)
        {
            Vector3 pos = parallel.transform.position;
            guidePoints = new Vector3[] { pos, pos, pos, pos };
        }
        Vector3 left = new Vector3(-directionVector.z, directionVector.y, directionVector.x);
        Vector3 right = new Vector3(directionVector.z, directionVector.y, -directionVector.x);
        if (linkedToNode)
        {
            bool found = false;
            float dist = 0f;
            for (int i = 0; i < rightLaneCount; i++)
            {
                dist += rSpacing[i];
                if (parallel.startNodes[i]!=null)
                {
                    guidePoints[0] = parallel.startNodes[i].transform.position + left * dist;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                dist = 0f;
                for (int i = 0; i < leftLaneCount; i++)
                {
                    dist += lSpacing[i];
                    if (parallel.endNodes[i] != null)
                    {
                        guidePoints[0] = parallel.endNodes[i].transform.position + right * dist;
                        break;
                    }
                }
            }            
        }
        else
        {
            guidePoints[0] = parallel.transform.position + adjustment * right;
        }
        guidePoints[1] = guidePoints[0] + directionVector * (defaultLength / 3f);
        guidePoints[2] = guidePoints[1] + directionVector * (defaultLength / 3f);
        guidePoints[3] = guidePoints[2] + directionVector * (defaultLength / 3f);

    }

    private string GenerateNodes()
    {
        string message = "";
        if (roadNetwork==null)
        {
            message = "Roadnetwork not selected";
            return message;
        }
        if (!CheckName(roadName))
        {
            message = "Name is not valid";
            return message;
        }
        string parentName = roadName;
        //Create parent object
        GameObject parentObject = new GameObject(parentName);
        parentObject.transform.position = parallel.transform.position;
        parentObject.AddComponent(typeof(Road));
        parentObject.transform.parent = roadNetwork.transform;

        // temporary list for node gameobjects for each lane
        List<GameObject> r1_Obs = new List<GameObject>();
        List<GameObject> r2_Obs = new List<GameObject>();
        List<GameObject> r3_Obs = new List<GameObject>();
        List<GameObject> l1_Obs = new List<GameObject>();
        List<GameObject> l2_Obs = new List<GameObject>();
        List<GameObject> l3_Obs = new List<GameObject>();
        // node points for each lane
        List<Vector3> rLane1 = CalculateNodepoints(true, 0);
        List<Vector3> rLane2 = CalculateNodepoints(true, 1);
        List<Vector3> rLane3 = CalculateNodepoints(true, 2);
        List<Vector3> lLane1 = CalculateNodepoints(false, 0);
        List<Vector3> lLane2 = CalculateNodepoints(false, 1);
        List<Vector3> lLane3 = CalculateNodepoints(false, 2);

        // Initialize lanes, nodes, in-nodes, out-nodes
        // right lanes
        // right lane 1
        if (parallel.RightLaneCount > 0)
        {
            string laneName = roadName + "_r1";
            InitializeLaneObject(laneName, ref parentObject, out GameObject laneObject);
            InitializeNodes(0, ref r1_Obs, ref rLane1, laneObject);
        }
        // right lane 2
        if (parallel.RightLaneCount > 1)
        {
            string laneName = roadName + "_r2";
            InitializeLaneObject(laneName, ref parentObject, out GameObject laneObject);
            InitializeNodes(1, ref r2_Obs, ref rLane2, laneObject);
        }
        // right lane 3
        if (parallel.RightLaneCount > 2)
        {
            string laneName = roadName + "_r3";
            InitializeLaneObject(laneName, ref parentObject, out GameObject laneObject);
            InitializeNodes(2, ref r3_Obs, ref rLane3, laneObject);
        }
        // left lanes
        // left lane 1
        if (parallel.LeftLaneCount > 0)
        {
            string laneName = roadName + "_l1";
            InitializeLaneObject(laneName, ref parentObject, out GameObject laneObject);
            InitializeNodes(3, ref l1_Obs, ref lLane1, laneObject);
        }
        // left lane 2
        if (parallel.LeftLaneCount > 1)
        {
            string laneName = roadName + "_l2";
            InitializeLaneObject(laneName, ref parentObject, out GameObject laneObject);
            InitializeNodes(4, ref l2_Obs, ref lLane2, laneObject);
        }
        // left lane 3
        if (parallel.LeftLaneCount > 2)
        {
            string laneName = roadName + "_l3";
            InitializeLaneObject(laneName, ref parentObject, out GameObject laneObject);
            InitializeNodes(5, ref l3_Obs, ref lLane3, laneObject);
        }
        //initialize buslanes
        if (parallel.BusLaneRight)
        {
            switch (parallel.RightLaneCount)
            {
                case 1:
                    for (int i = parallel.BusRightStart; i < parallel.BusRightEnd; i++)
                    {
                        r1_Obs[i].GetComponent<Nodes>().BusLane = true;
                    }
                    break;
                case 2:
                    for (int i = parallel.BusRightStart; i < parallel.BusRightEnd; i++)
                    {
                        r2_Obs[i].GetComponent<Nodes>().BusLane = true;
                    }
                    break;
                case 3:
                    for (int i = parallel.BusRightStart; i < parallel.BusRightEnd; i++)
                    {
                        r3_Obs[i].GetComponent<Nodes>().BusLane = true;
                    }
                    break;
            }
        }
        if (parallel.BusLaneLeft)
        {
            switch (parallel.LeftLaneCount)
            {
                case 1:
                    for (int i = parallel.BusLeftStart; i < parallel.BusLeftEnd; i++)
                    {
                        l1_Obs[i].GetComponent<Nodes>().BusLane = true;
                    }
                    break;
                case 2:
                    for (int i = parallel.BusLeftStart; i < parallel.BusLeftEnd; i++)
                    {
                        l2_Obs[i].GetComponent<Nodes>().BusLane = true;
                    }
                    break;
                case 3:
                    for (int i = parallel.BusLeftStart; i < parallel.BusLeftEnd; i++)
                    {
                        l3_Obs[i].GetComponent<Nodes>().BusLane = true;
                    }
                    break;
            }
        }
        //set parallel lanes
        for (int i=0; i < parallel.NodeCount; i++)
        {
            if (parallel.RightLaneCount > 1)
            {
                // r1 to r2
                r1_Obs[i].GetComponent<Nodes>().ParallelRight = r2_Obs[i].GetComponent<Nodes>();
                // r2 to r1
                r2_Obs[i].GetComponent<Nodes>().ParallelLeft = r1_Obs[i].GetComponent<Nodes>();
            }
            if (parallel.RightLaneCount > 2)
            {
                // r2 to r3
                r2_Obs[i].GetComponent<Nodes>().ParallelRight = r3_Obs[i].GetComponent<Nodes>();
                // r3 to r2
                r3_Obs[i].GetComponent<Nodes>().ParallelLeft = r2_Obs[i].GetComponent<Nodes>();
            }
            if (parallel.LeftLaneCount > 1)
            {
                // l1 to l2
                l1_Obs[i].GetComponent<Nodes>().ParallelRight = l2_Obs[i].GetComponent<Nodes>();
                // l2 to l1
                l2_Obs[i].GetComponent<Nodes>().ParallelLeft = l1_Obs[i].GetComponent<Nodes>();
            }
            if (parallel.LeftLaneCount > 2)
            {
                // l2 to l3
                l2_Obs[i].GetComponent<Nodes>().ParallelRight = l3_Obs[i].GetComponent<Nodes>();
                // l3 to l2
                l3_Obs[i].GetComponent<Nodes>().ParallelLeft = l2_Obs[i].GetComponent<Nodes>();
            }
        }
        // set lane changes
        List<Vector2Int> changes;
        //r1 to r2
        if (parallel.permittedLaneChanges[0])
        {
            changes = IndexedLaneChanges(ref rLane1, ref rLane2, parallel.laneChangeStartIndex[0],
                parallel.laneChangeEndIndex[0]);
            foreach (Vector2Int c in changes)
            {
                r1_Obs[c.x].GetComponent<Nodes>().LaneChangeRight = r2_Obs[c.y].GetComponent<Nodes>();
            }
        }
        // r2 to r1
        if (parallel.permittedLaneChanges[1])
        {
            changes = IndexedLaneChanges(ref rLane2, ref rLane1, parallel.laneChangeStartIndex[1],
                parallel.laneChangeEndIndex[1]);
            foreach (Vector2Int c in changes)
            {
                r2_Obs[c.x].GetComponent<Nodes>().LaneChangeLeft = r1_Obs[c.y].GetComponent<Nodes>();
            }
        }
        // r2 to r3
        if (parallel.permittedLaneChanges[2])
        {
            changes = IndexedLaneChanges(ref rLane2, ref rLane3, parallel.laneChangeStartIndex[2],
                parallel.laneChangeEndIndex[2]);
            foreach (Vector2Int c in changes)
            {
                r2_Obs[c.x].GetComponent<Nodes>().LaneChangeRight = r3_Obs[c.y].GetComponent<Nodes>();
            }
        }
        // r3 to r2
        if (parallel.permittedLaneChanges[3])
        {
            changes = IndexedLaneChanges(ref rLane3, ref rLane2, parallel.laneChangeStartIndex[3],
                parallel.laneChangeEndIndex[3]);
            foreach (Vector2Int c in changes)
            {
                r3_Obs[c.x].GetComponent<Nodes>().LaneChangeLeft = r2_Obs[c.y].GetComponent<Nodes>();
            }
        }
        // l1 to l2
        if (parallel.permittedLaneChanges[4])
        {
            changes = IndexedLaneChanges(ref lLane1, ref lLane2, parallel.laneChangeStartIndex[4],
                parallel.laneChangeEndIndex[4]);
            foreach (Vector2Int c in changes)
            {
                l1_Obs[c.x].GetComponent<Nodes>().LaneChangeRight = l2_Obs[c.y].GetComponent<Nodes>();
            }
        }
        // l2 to l1
        if (parallel.permittedLaneChanges[5])
        {
            changes = IndexedLaneChanges(ref lLane2, ref lLane1, parallel.laneChangeStartIndex[5],
                parallel.laneChangeEndIndex[5]);
            foreach (Vector2Int c in changes)
            {
                l2_Obs[c.x].GetComponent<Nodes>().LaneChangeLeft = l1_Obs[c.y].GetComponent<Nodes>();
            }
        }
        // l2 to l3
        if (parallel.permittedLaneChanges[6])
        {
            changes = IndexedLaneChanges(ref lLane2, ref lLane3, parallel.laneChangeStartIndex[6],
                parallel.laneChangeEndIndex[6]);
            foreach (Vector2Int c in changes)
            {
                l2_Obs[c.x].GetComponent<Nodes>().LaneChangeRight = l3_Obs[c.y].GetComponent<Nodes>();
            }
        }
        // l3 to l2
        if (parallel.permittedLaneChanges[7])
        {
            changes = IndexedLaneChanges(ref lLane3, ref lLane2, parallel.laneChangeStartIndex[7],
                parallel.laneChangeEndIndex[7]);
            foreach (Vector2Int c in changes)
            {
                l3_Obs[c.x].GetComponent<Nodes>().LaneChangeLeft = l2_Obs[c.y].GetComponent<Nodes>();
            }
        }

        ObjectTagger.SetLaneIcons(TagColorScheme.ByLaneNumber, 0, ref r1_Obs);
        ObjectTagger.SetLaneIcons(TagColorScheme.ByLaneNumber, 1, ref r2_Obs);
        ObjectTagger.SetLaneIcons(TagColorScheme.ByLaneNumber, 2, ref r3_Obs);
        ObjectTagger.SetLaneIcons(TagColorScheme.ByLaneNumber, 3, ref l1_Obs);
        ObjectTagger.SetLaneIcons(TagColorScheme.ByLaneNumber, 4, ref l2_Obs);
        ObjectTagger.SetLaneIcons(TagColorScheme.ByLaneNumber, 5, ref l3_Obs);

        message = "Nodes created";
        return message;
    }

    private void InitializeLaneObject(string laneName, ref GameObject parentObject, out GameObject laneObject)
    {
        laneObject = new GameObject(laneName);
        laneObject.transform.position = parallel.transform.position;
        laneObject.transform.parent = parentObject.transform;
        Lane l = laneObject.AddComponent<Lane>();
        l.Traffic = parallel.Traffic;
        l.LaneYield = DriverYield.Normal;
        l.SpeedLimit = parallel.SpeedLimit;
    }

    private void InitializeNodes(int laneIndex, ref List<GameObject>objectList,
        ref List<Vector3> positionList, GameObject laneObject)
    {
        for (int i = 0; i < parallel.NodeCount; i++)
        {
            // If first node is linked to an existing one,
            // link instead of creating a new one. Same with the last node and
            // with the other lanes as well.
            if (i == 0 && parallel.startNodes[laneIndex] != null)
            {
                objectList.Add(parallel.startNodes[laneIndex].gameObject);
            }
            else if (i == parallel.NodeCount - 1 && parallel.endNodes[laneIndex] != null)
            {
                objectList.Add(parallel.endNodes[laneIndex].gameObject);
            }
            else
            {
                string pName = laneObject.name + "_" + i;
                GameObject pObject = new GameObject(pName);
                pObject.transform.position = positionList[i];
                pObject.AddComponent<Nodes>();
                objectList.Add(pObject);
                pObject.transform.parent = laneObject.transform;
                pObject.GetComponent<Nodes>().ParentLane = laneObject.GetComponent<Lane>();
            }
            // set in-nodes and out-nodes
            if (i > 0)
            {
                objectList[i].GetComponent<Nodes>().AddInNode(objectList[i - 1].GetComponent<Nodes>());
                objectList[i - 1].GetComponent<Nodes>().AddOutNode(objectList[i].GetComponent<Nodes>());
            }
        }
        // set node array in lane
        Nodes[] nodeArray = new Nodes[objectList.Count];
        for (int i = 0; i < objectList.Count; i++)
        {
            nodeArray[i] = objectList[i].GetComponent<Nodes>();
        }
        laneObject.GetComponent<Lane>().nodesOnLane = nodeArray;
    }

    private List<Vector2Int> IndexedLaneChanges (ref List<Vector3> fromLane, ref List<Vector3> toLane, int start, int end)
    {
        List<Vector2Int> indexVector = new List<Vector2Int>();
        for (int i = start; i <= end; i++)
        {
            Vector3 p0 = fromLane[i];
            int ind = i + 1;
            while(true)
            {
                if (ind > toLane.Count - 1)
                {
                    break;
                }
                if (Vector3.Distance(fromLane[i], toLane[ind]) > laneChangeDistance)
                {
                    indexVector.Add(new Vector2Int(i, ind));
                    break;
                }
                ind++;
            }
        }
        return indexVector;
    }

    private void OnEnable()
    {
        Tools.current = Tool.View;
        Tools.hidden = true;
        SetCameraAngle();
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    private void SetCameraAngle()
    {
        var sceneView = SceneView.lastActiveSceneView;
        if (parallel == null)
        {
            parallel = target as ParallelBezierSplines;
        }
        sceneView.AlignViewToObject(parallel.transform);
        sceneView.LookAtDirect(parallel.transform.position, Quaternion.Euler(90, 0, 0), 30f);
        sceneView.orthographic = true;
    }
}
