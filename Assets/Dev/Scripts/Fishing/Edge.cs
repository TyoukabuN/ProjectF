using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Edge : MonoBehaviour
{
    public Point[] points = new Point[2];
    public float originLength = 1;
    public float tinyValue = 0.001f;

    public void Tick()
    {
        if (points[0] == null)
            return;

        if (points[1] == null)
            return;

        var p1 = points[0];
        var p2 = points[1];

        //p1.Tick(Time.deltaTime);
        //p2.Tick(Time.deltaTime);

        var p1p2 = p2.transform.position - p1.transform.position;

        //var diff = Mathf.Abs(p1p2.magnitude - originLength);
        var diff = (p1p2.magnitude - originLength);

        if (!p2.simulate)
        {
            p1.transform.position += p1p2.normalized * diff * 1.0f;
        }
        else if (!p1.simulate)
        {
            p2.transform.position -= p1p2.normalized * diff * 1.0f;
        }
        else
        {
            p1.transform.position += p1p2.normalized * diff * 0.5f;
            p2.transform.position -= p1p2.normalized * diff * 0.5f;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (points[0] == null)
            return;

        if (points[1] == null)
            return;

        Gizmos.color = Color.white;
        if (Selection.activeGameObject == this.gameObject)
        {
            Gizmos.color = Color.yellow;
        }

        Gizmos.DrawLine(this.points[0].transform.position, this.points[1].transform.position);
        Gizmos.color = Color.white;

    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(Edge))]
[CanEditMultipleObjects]
public class EdgeCustom : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Add To Collision Detection"))
        {
            foreach (var edge in targets)
            {
                CollisionDetection.AddEdge(edge as Edge);
            }
        }
        if (GUILayout.Button("Remove From Collision Detection"))
        {
            foreach (var edge in targets)
            {
                CollisionDetection.RemoveEdge(edge as Edge);
            }
        }
    }
}

#endif

