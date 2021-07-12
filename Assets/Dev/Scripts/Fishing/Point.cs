using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Point : MonoBehaviour
{
    public Vector3 Acceleration;
    public Vector3 OldPosition;
    public bool simulate = false;

    public void Tick(float timeStep)
    {
        var point = this;

        var temp = point.transform.position;

        point.transform.position += point.transform.position - point.OldPosition + point.Acceleration* timeStep * timeStep;

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
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("store current postion"))
        {
            foreach(var point in targets)
            {
                var target = point as Point;
                target.OldPosition = target.transform.position;
            }
        }
    }
}
#endif
