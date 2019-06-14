using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadNetwork))]
public class RoadNetworkInspector : Editor
{
    RoadNetwork network;
    List<Vector3> points;
    List<Vector3> allPoints;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(network.gameObject.name, EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        bool showLines = network.showLines;
        showLines = EditorGUILayout.ToggleLeft("Show lines?", showLines);
        if (showLines != network.showLines)
        {
            network.showLines = showLines;
            if (network.showLines == true)
            {
                network.showDetailed = false;
            }
            SceneView.RepaintAll();
        }
        showLines = network.showDetailed;
        showLines = EditorGUILayout.ToggleLeft("Shoe point-to-point?", showLines);
        if (showLines != network.showDetailed)
        {
            network.showDetailed = showLines;
            if (network.showDetailed == true)
            {
                network.showLines = false;
            }
            SceneView.RepaintAll();
        }
    }

    private void FetchPoints()
    {
        points = new List<Vector3>();
        Road[] allRoads = network.gameObject.GetComponentsInChildren<Road>();
        for (int i = 0; i < allRoads.Length; i++)
        {
            Road road = allRoads[i];
            Lane[] allLanes = road.gameObject.GetComponentsInChildren<Lane>();
            for (int j = 0; j < allLanes.Length; j++)
            {
                Lane l = allLanes[j];
                points.Add(l.nodesOnLane[0].transform.position);
                points.Add(l.nodesOnLane[l.nodesOnLane.Length - 1].transform.position);
            }
        }
    }

    private void FetchAllPoints()
    {
        allPoints = new List<Vector3>();
        Road[] allRoads = network.gameObject.GetComponentsInChildren<Road>();
        for (int i = 0; i < allRoads.Length; i++)
        {
            Road road = allRoads[i];
            Lane[] allLanes = road.gameObject.GetComponentsInChildren<Lane>();
            for (int j = 0; j < allLanes.Length; j++)
            {
                Lane l = allLanes[j];
                for (int k = 0; k < l.nodesOnLane.Length - 1; k++)
                {
                    allPoints.Add(l.nodesOnLane[k].transform.position);
                    allPoints.Add(l.nodesOnLane[k + 1].transform.position);
                }
            }
        }
    }

    private void ShowLines()
    {
        Handles.color = Color.green;
        for (int i = 0; i < points.Count; i += 2)
        {
            Handles.DrawLine(points[i], points[i + 1]);
        }
    }

    private void ShowAllLines()
    {
        Handles.color = Color.green;
        for (int i = 0; i < allPoints.Count; i += 2)
        {
            Handles.DrawLine(allPoints[i], allPoints[i + 1]);
        }
    }

    private void OnSceneGUI()
    {
        if (network.showLines)
        {
            if (points == null || points.Count == 0)
            {
                FetchPoints();
            }
            ShowLines();
        }
        if (network.showDetailed)
        {
            if (allPoints == null || allPoints.Count == 0)
            {
                FetchAllPoints();
            }
            ShowAllLines();
        }
    }

    private void OnEnable()
    {
        network = target as RoadNetwork;
    }

    private void OnDisable()
    {

    }
}
