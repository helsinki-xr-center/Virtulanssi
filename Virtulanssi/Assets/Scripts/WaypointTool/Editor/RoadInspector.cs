using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Road))]
public class RoadInspector : Editor
{
    Road road;
    List<Vector3> linePositions;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(road.name, EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        bool showLanes = road.showLanes;
        showLanes = EditorGUILayout.ToggleLeft("Show lanes?", showLanes);
        if (showLanes != road.showLanes)
        {
            road.showLanes = showLanes;
            SceneView.RepaintAll();
        }
    }

    private void FetchLinePositions()
    {
        linePositions = new List<Vector3>();
        Lane[] allLanes = road.gameObject.GetComponentsInChildren<Lane>();
        for (int i = 0; i < allLanes.Length; i++)
        {
            for (int j = 0; j < allLanes[i].nodesOnLane.Length - 1; j++)
            {
                linePositions.Add(allLanes[i].nodesOnLane[j].transform.position);
                linePositions.Add(allLanes[i].nodesOnLane[j + 1].transform.position);
                }
        }
    }

    private void ShowLinePositions()
    {
        Handles.color = Color.yellow;
        for (int i = 0; i < linePositions.Count; i += 2)
        {
            Handles.DrawLine(linePositions[i], linePositions[i + 1]);
        }
    }

    private void OnSceneGUI()
    {
        if (road.showLanes)
        {
            if (linePositions == null || linePositions.Count == 0)
            {
                FetchLinePositions();
            }
            ShowLinePositions();
        }
    }

    private void OnEnable()
    {
        road = target as Road;
    }

    private void OnDisable()
    {

    }
}
