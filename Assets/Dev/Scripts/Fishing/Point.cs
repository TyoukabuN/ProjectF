using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Point : MonoBehaviour
{
    public Vector3 gravity = new Vector3(0,-9.8f,0);
    public Vector3 acceleration = Vector3.zero;
    public float mass = 1;
    [Range(0,1)]
    public float dumping = 0.5f;
    public Vector3 OldPosition;
    [SerializeField]public bool m_simulate = true;
    public bool simulate
    {
        get
        {
            return m_simulate;
        }
        set {
            if (value)
            {
                OldPosition = transform.position;
            }
            m_simulate = value;
        }
    }

    public void AddForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    public void Tick(float timeStep)
    {
        var point = this;

        var temp = point.transform.position;

        point.transform.position += point.transform.position - point.OldPosition + point.gravity* timeStep * timeStep;

        point.transform.position += point.acceleration * timeStep * timeStep;

        point.acceleration = Vector3.Lerp(point.acceleration, Vector3.zero, dumping);

        point.OldPosition = temp;
    }
    public void Update()
    {
        if (!simulate)
            return;

        Tick(Time.deltaTime);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Point))]
[CanEditMultipleObjects]
public class PointCustom : Editor
{
    public Vector3 force = Vector3.zero;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var instance = target as Point;

        if (GUILayout.Button("current position as oldPosition"))
        {
            foreach (var point in targets)
            {
                var target = point as Point;
                target.OldPosition = target.transform.position;
            }
        }

        EditorGUILayout.BeginHorizontal();
        {
            force = EditorGUILayout.Vector3Field("",force);
            if (GUILayout.Button("施力"))
            {
                instance.AddForce(force);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

