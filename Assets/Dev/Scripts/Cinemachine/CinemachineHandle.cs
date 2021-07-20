using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CinemachineHandle : MonoBehaviour
{
    public CinemachineBrain brain;

    public List<CinemachineVirtualCamera> virtualCameraList = new List<CinemachineVirtualCamera>();
}

[CustomEditor(typeof(CinemachineHandle))]
public class CinemachineHandleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CinemachineHandle instance = target as CinemachineHandle;
        foreach(var virtualCamera in instance.virtualCameraList)
        {
            EditorGUILayout.BeginHorizontal();
            { 
                EditorGUILayout.ObjectField(virtualCamera, typeof(CinemachineVirtualCamera), true);
                if (GUILayout.Button("切换"))
                {
                    virtualCamera.Priority = 1;
                }
                else
                {
                    virtualCamera.Priority = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
