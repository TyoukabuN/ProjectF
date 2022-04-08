using System.Collections;
using System.Collections.Generic;
using TMath;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AxisAngle : MonoBehaviour
{
    public Vector3 Axi;
    public float Angle;
    public Transform RefAxiTransform;

    public void ApplyRotate()
    { 

    }
    public void ApplyRefRotate()
    {

    }
    public void Reset()
    {
        transform.rotation = Quaternion.identity;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AxisAngle))]
public class AxisAngleEditor:Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var handle = target as AxisAngle;
        if (handle == null)
            return;

        GUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            if (GUILayout.Button("Apply Rotate"))
            {
                handle.ApplyRotate();
            }
            if (GUILayout.Button("Apply Ref"))
            {
                handle.ApplyRefRotate();
            }
            if (GUILayout.Button("Reset"))
            {
                handle.ApplyRefRotate();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif