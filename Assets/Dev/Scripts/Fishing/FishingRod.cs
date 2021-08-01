﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FishingRod : MonoBehaviour
{
    /// <summary>
    /// 鱼线有多长
    /// </summary>
    private float fishLineLength = 10.0f;
    /// <summary>
    /// 绳子有多少节
    /// </summary>
    private int m_FishLineStep = 10;

    public float stepLength = 1f;

    public float lineWidth = 0.1f;

    public bool gizmos = true;

    public int StepCount
    {
        get {
            return collisiionDetection.stepCount;
        }
    }
    public Point LastPoint
    {
        get {
            if (StepCount <= 0)
            {
                return null;
            }
            return collisiionDetection.Edges[StepCount - 1].points[1];
        }
    }
    public Point RootPoint
    {
        get
        {
            if (StepCount <= 0)
            {
                return null;
            }
            return collisiionDetection.Edges[0].points[0];
        }
    }

    private CollisionDetection collisiionDetection = new CollisionDetection();
    private LineRenderer lineRenderer;

    public int fishLineStep
    {
        get
        {
            return m_FishLineStep;
        }

        set
        {
            if (value == m_FishLineStep)
                return;

            UpdateLineStep();
        }
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (!lineRenderer)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
    }
    private void Update()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        collisiionDetection.OnUpdate();
        UpdateLineRenderer();
    }

    public void UpdateLineStep()
    {
        if (collisiionDetection.stepCount == m_FishLineStep)
            return;

        float diff = Mathf.Abs(collisiionDetection.stepCount - m_FishLineStep);

        for (int i = 0; i < diff; i++)
        {
            if (diff < 0) {
                RemoveStep();
                continue;
            }
            AddStep();
        }
    }
    public Point GetPoint(string name = "")
    {
        return GetPoint(transform, name);
    }

    public Point GetPoint(Transform parent,string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            name = string.Format("point_{0}", StepCount + 1);
        }
        var gobj = new GameObject(name);
        Point point = gobj.AddComponent<Point>();

        gobj.transform.position = parent.transform.position;
        //
        point.OldPosition = parent.position;

        return point;
    }

    public bool AddStep()
    {
        if (!LastPoint)
        {
            var root = GetPoint("lineRoot");
            root.gameObject.transform.SetParent(transform, false);
            root.gameObject.transform.localPosition = Vector3.zero;
            root.simulate = false;

            return collisiionDetection.AddEdge(root, GetPoint(), stepLength);
        }
        return collisiionDetection.AddEdge(LastPoint, GetPoint(LastPoint.transform), stepLength);
    }

    public bool RemoveStep()
    {
        if (!LastPoint)
        {
            return false;
        }

        var edge = collisiionDetection.Edges[collisiionDetection.Edges.Count - 1];
        collisiionDetection.RemoveEdge(edge);
        return true;
    }

    public void UpdateLineRenderer()
    {
        if (StepCount <= 0)
            return;

        Vector3[] positions = new Vector3[StepCount + 1];
        positions[0] = collisiionDetection.Edges[0].points[0].transform.position;
        for (int i=0; i < StepCount;i++)
        {
            var edge = collisiionDetection.Edges[i];
            positions[i + 1] = edge.points[1].transform.position;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!gizmos)
            return;

        foreach (var edge in collisiionDetection.Edges)
        {
            if (edge.points[0] == null)
                return;

            if (edge.points[1] == null)
                return;

            Gizmos.DrawLine(edge.points[0].transform.position, edge.points[1].transform.position);
            Gizmos.color = Color.white;
        }
    }
#endif

}


#if UNITY_EDITOR
[CustomEditor(typeof(FishingRod))]
[CanEditMultipleObjects]
public class FishingRodEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FishingRod instance = target as FishingRod;

        if (GUILayout.Button("Add Step"))
        {
            instance.AddStep();
        }
        if (GUILayout.Button("Remove Step"))
        {
            instance.RemoveStep();
        }
    }
}


#endif

