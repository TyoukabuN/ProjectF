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
    public float fishLineLength = 10.0f;
    /// <summary>
    /// 绳子有多少节
    /// </summary>
    public float fishLineStep = 10f;

    public float stepLength {
        get {
            return fishLineLength / fishLineStep;
        }
    }

    public float m_stepLengthPct = 1f;
    [Range(0,1)]
    public float stepLengthPct = 1f;
    [Range(0, 1)]
    public float edgeDumping = 1f;

    public float lineWidth = 0.1f;

    public bool gizmos = true;

    public int StepCount
    {
        get {
            return collisionDetection.stepCount;
        }
    }
    public Point LastPoint
    {
        get {
            if (StepCount <= 0)
            {
                return null;
            }
            return collisionDetection.Edges[StepCount - 1].points[1];
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
            return collisionDetection.Edges[0].points[0];
        }
    }

    private CollisionDetection collisionDetection = new CollisionDetection();
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (!lineRenderer)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
    }

    public float frac(float value)
    {
        return value - Mathf.Floor(value);
    }

    private void Update()
    {
        ApplyNormalizeLength();
        UpdateLineRenderer();

        for (int i = 0; i < collisionDetection.Edges.Count; i++)
        {
            var edge = collisionDetection.Edges[i];
            edge.dumping = edgeDumping;
        }

        collisionDetection.OnUpdate();
    }

    public void ApplyNormalizeLength()
    {
        if (m_stepLengthPct == stepLengthPct)
            return;


       // var floor = Mathf.Floor(m_stepLengthPct * StepCount);

        var sign = stepLengthPct - m_stepLengthPct;

        if (true)
        {
            for (int i = StepCount - 1; i >= 0; i--)
            {
                var edge = collisionDetection.Edges[i];
                float pct = (i + 1) - stepLengthPct * StepCount;

                if (pct < 1 && pct > 0)
                    edge.normalizeLength = Mathf.Clamp01(Mathf.Abs(1 - pct));
                else if (pct >= 1)
                    edge.normalizeLength = 0f;
                else
                    edge.normalizeLength = 1f;
            }

        }

        //if (sign > 0)
        //{ 
        //    for (int i = 0; i < StepCount; i++)
        //    {
        //        var edge = collisionDetection.Edges[StepCount - i - 1];
        //        float pct = stepLengthPct * StepCount - i;

        //        if (pct < 1 && pct > 0)
        //            edge.normalizeLength = Mathf.Clamp01(Mathf.Abs(1 - pct));
        //        else if (pct <= 0)
        //            edge.normalizeLength = 0f;
        //        else
        //            edge.normalizeLength = 1f;
        //    }
        //}

        m_stepLengthPct = stepLengthPct = Mathf.Clamp01(stepLengthPct);
    }

    public void UpdateLineStep()
    {
        if (collisionDetection.stepCount == fishLineStep)
            return;

        float diff = Mathf.Abs(collisionDetection.stepCount - fishLineStep);

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

    public void AddStep()
    {
        if (!LastPoint)
        {
            var root = GetPoint("lineRoot");
            root.gameObject.transform.SetParent(transform, false);
            root.gameObject.transform.localPosition = Vector3.zero;
            root.simulate = false;

            collisionDetection.AddEdge(root, GetPoint(), stepLength);
            collisionDetection.SetEdgesLength(stepLength);
            return;
        }

        collisionDetection.AddEdge(LastPoint, GetPoint(LastPoint.transform), stepLength);
        collisionDetection.SetEdgesLength(stepLength);
    }

    public bool RemoveStep()
    {
        if (!LastPoint)
        {
            return false;
        }

        var edge = collisionDetection.Edges[collisionDetection.Edges.Count - 1];
        collisionDetection.RemoveEdge(edge,true);
        return true;
    }

    public void UpdateLineRenderer()
    {
        if (!lineRenderer)
            return;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        if (StepCount <= 0)
            return;

        Vector3[] positions = new Vector3[StepCount + 1];
        positions[0] = collisionDetection.Edges[0].points[0].transform.position;
        for (int i=0; i < StepCount;i++)
        {
            var edge = collisionDetection.Edges[i];
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

        foreach (var edge in collisionDetection.Edges)
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

        if (GUILayout.Button("General Line"))
        {
            while (instance.StepCount < instance.fishLineStep)
            {
                instance.AddStep();
            }
        }

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

