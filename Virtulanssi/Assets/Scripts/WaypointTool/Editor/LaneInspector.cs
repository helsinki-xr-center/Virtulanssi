using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Lane))]
public class LaneInspector : Editor
{
    Lane lane;
    List<Vector3> lines;
    List<Vector3> otherLines;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(lane.gameObject.name, EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        lane.Traffic = (TrafficSize)EditorGUILayout.EnumPopup("Traffic", lane.Traffic);
        lane.SpeedLimit = (SpeedLimits)EditorGUILayout.EnumPopup("Speed limit", lane.SpeedLimit);
        lane.LaneYield = (DriverYield)EditorGUILayout.EnumPopup("Driver yield", lane.LaneYield);
        EditorGUILayout.Separator();

        bool drawPoints = lane.pointToPointLine;
        drawPoints = EditorGUILayout.ToggleLeft("Draw point-to-point?", drawPoints);
        if (drawPoints != lane.pointToPointLine)
        {
            lane.pointToPointLine = drawPoints;
            SceneView.RepaintAll();
        }
        drawPoints = lane.drawAllLanes;
        drawPoints = EditorGUILayout.ToggleLeft("Draw other lines?", drawPoints);
        if (drawPoints != lane.drawAllLanes)
        {
            lane.drawAllLanes = drawPoints;
            SceneView.RepaintAll();
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Nodes on lane: " + lane.nodesOnLane.Length, EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        for (int i = 0; i < lane.nodesOnLane.Length; i++)
        {
            EditorGUILayout.LabelField(lane.nodesOnLane[i].gameObject.name);
        }
    }

    private void FetchLinePositions()
    {
        lines = new List<Vector3>();
        for (int i = 0; i < lane.nodesOnLane.Length - 1; i++)
        {
            lines.Add(lane.nodesOnLane[i].transform.position);
            lines.Add(lane.nodesOnLane[i + 1].transform.position);
        }
    }

    private void FetchOtherLines()
    {
        otherLines = new List<Vector3>();
        Lane[] allLanes = lane.gameObject.transform.parent.GetComponentsInChildren<Lane>();
        for (int i = 0; i < allLanes.Length; i++)
        {
            if (allLanes[i] != lane)
            {
                for (int j = 0; j < allLanes[i].nodesOnLane.Length - 1; j++)
                {
                    otherLines.Add(allLanes[i].nodesOnLane[j].transform.position);
                    otherLines.Add(allLanes[i].nodesOnLane[j + 1].transform.position);
                }
            }
        }
    }

    private void DrawLines()
    {
        Handles.color = Color.yellow;
        for (int i = 0; i < lines.Count; i += 2)
        {
            Handles.DrawLine(lines[i], lines[i + 1]);
        }
    }

    private void DrawOtherLines()
    {
        Handles.color = Color.grey;
        for (int i = 0; i < otherLines.Count; i += 2)
        {
            Handles.DrawLine(otherLines[i], otherLines[i + 1]);
        }
    }

    private void OnSceneGUI()
    {
        if (lane.pointToPointLine)
        {
            if (lines == null || lines.Count == 0)
            {
                FetchLinePositions();
            }
            DrawLines();
        }
        if (lane.drawAllLanes)
        {
            if (otherLines == null || otherLines.Count == 0)
            {
                FetchOtherLines();
            }
            DrawOtherLines();
        }
    }

    private void OnEnable()
    {
        lane = target as Lane;
    }

    private void OnDisable()
    {
        
    }
}
