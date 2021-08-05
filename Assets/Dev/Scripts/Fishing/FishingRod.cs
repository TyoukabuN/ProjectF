using System.Collections;
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

    public float m_edgeNormalizeLength = 1f;
    [Range(0,1)]
    public float edgeNormalizeLength = 1f;
    [Range(0, 1)]
    public float edgeDumping = 1f;

    public float lineWidth = 0.1f;

    public bool gizmos = true;

    public List<Edge> Edges {
        get {
            return collisionDetection.Edges;
        }
    }
    public int edgeCount
    {
        get {
            return collisionDetection.edgeCount;
        }
    }
    public Point LastPoint
    {
        get {
            if (edgeCount <= 0)
            {
                return null;
            }
            return collisionDetection.Edges[edgeCount - 1].LastPoint;
        }
    }
    public Point RootPoint
    {
        get
        {
            if (edgeCount <= 0)
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
        if (m_edgeNormalizeLength == edgeNormalizeLength)
            return;


       // var floor = Mathf.Floor(m_stepLengthPct * StepCount);

       // var sign = stepLengthPct - m_stepLengthPct;

        if (true)
        {
            for (int i = edgeCount - 1; i >= 0; i--)
            {
                var edge = collisionDetection.Edges[i];
                float pct = (i + 1) - edgeNormalizeLength * edgeCount;

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

        m_edgeNormalizeLength = edgeNormalizeLength = Mathf.Clamp01(edgeNormalizeLength);
    }

    public void UpdateLineStep()
    {
        if (collisionDetection.edgeCount == fishLineStep)
            return;

        float diff = Mathf.Abs(collisionDetection.edgeCount - fishLineStep);

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
            name = string.Format("point_{0}", edgeCount + 1);
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
        Edge edge = null;
        if (!LastPoint)
        {
            var root = GetPoint("lineRoot");
            root.gameObject.transform.SetParent(transform, false);
            root.gameObject.transform.localPosition = Vector3.zero;
            root.simulate = false;

            edge = collisionDetection.AddEdge(root, GetPoint(), stepLength);
            edge.normalizeLength = edgeNormalizeLength;
            collisionDetection.SetEdgesLength(stepLength);
            return;
        }

        edge = collisionDetection.AddEdge(LastPoint, GetPoint(LastPoint.transform), stepLength);
        edge.normalizeLength = edgeNormalizeLength;
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

        if (edgeCount <= 0)
        {
            if (lineRenderer.positionCount > 0)
                lineRenderer.positionCount = 0;

            return;
        }

        Vector3[] positions = new Vector3[edgeCount + 1];
        positions[0] = collisionDetection.Edges[0].points[0].transform.position;
        for (int i=0; i < edgeCount;i++)
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

    //添加一个渐进力,从最后一个点向第一个点递减
    public void AddDescendingForceToEdges(Vector3 force)
    {
        var unit = force / edgeCount;
        for (int i = edgeCount - 1; i>= 0;i--)
        {
            var edge = Edges[i];
            if (edge == null)
                continue;

            edge.LastPoint.AddForce(unit * (i + 1));
        }
    }

    public void Shot(Vector3 force,bool descending = true)
    {
        while (edgeCount < fishLineStep)
        {
            AddStep();
        }

        ApplyNormalizeLength();

        if (descending)
        {
            AddDescendingForceToEdges(force);
        }
        else if(edgeCount > 0)
        {
            Edges[edgeCount].LastPoint.AddForce(force);
        }

    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(FishingRod))]
[CanEditMultipleObjects]
public class FishingRodEditor : Editor
{
    public Vector3 force = Vector3.zero;
    public float power = 800f;
    public Transform shotTarget = null;
    public bool descending = true;
    public bool lastSimulation = false;
    public bool addRigidBody  = true;
    
    public void AddRigidbody()
    {
        if (addRigidBody)
        {
            FishingRod instance = target as FishingRod;
            instance.LastPoint.gameObject.AddComponent<Rigidbody>();
            var collider = instance.LastPoint.gameObject.AddComponent<BoxCollider>();
            collider.size = collider.size * 0.25f;
        }
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FishingRod instance = target as FishingRod;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            if (GUILayout.Button("General Line"))
            {
                while (instance.edgeCount < instance.fishLineStep)
                {
                    instance.AddStep();
                }
                instance.LastPoint.simulate = lastSimulation;
                instance.ApplyNormalizeLength();
                AddRigidbody();

            }

            if (GUILayout.Button("Add Step"))
            {
                instance.AddStep();
            }
            if (GUILayout.Button("Remove All Step"))
            {
                while (instance.edgeCount > 0)
                { 
                    instance.RemoveStep();
                }
            }
            if (GUILayout.Button("Remove Step"))
            {
                instance.RemoveStep();
            }
            EditorGUILayout.EndVertical();
        }


        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        {
            EditorGUILayout.BeginVertical();
            {
                shotTarget = EditorGUILayout.ObjectField("目标:", shotTarget, typeof(Transform), true) as Transform;
                power = EditorGUILayout.FloatField("力度", power);
                descending = EditorGUILayout.Toggle("传递所有点的递减力",descending);
                lastSimulation = EditorGUILayout.Toggle("最后一点是否【物理模拟】", lastSimulation);
                addRigidBody = EditorGUILayout.Toggle("最后一点是否【使用刚体】", addRigidBody);

                EditorGUILayout.EndVertical();
            }

            if (!shotTarget)
            {
                var temp = GameObject.Find("ShotTarget");
                if (!temp)
                {
                    temp = (new GameObject("ShotTarget"));
                }
                if (temp)
                {
                    shotTarget = temp.transform;
                }
            }

            if (GUILayout.Button("Shot"))
            {
                var dir = shotTarget.position - instance.transform.position;
                force = dir.normalized * power;
                instance.Shot(force, descending);
                instance.LastPoint.simulate = lastSimulation;
                AddRigidbody();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}


#endif

