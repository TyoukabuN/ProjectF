using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIEffectHelper : MonoBehaviour
{
    [HideInInspector]public int _XTime_ID = Shader.PropertyToID("_XTime");
    [HideInInspector]private Vector4 tempVec4 = Vector4.zero;


    public void Update()
    {
        float t = Time.realtimeSinceStartup;
        tempVec4.Set(t / 20, t, t * 2, t * 3);
        Shader.SetGlobalVector(Shader.PropertyToID("_XTime"), tempVec4);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(UIEffectHelper))]
public class CustomImageEditor:Editor
{
    public Vector4 s_temp_v4 = Vector4.zero;
    private Graphic graphic;
    public float animeTime = 1;
    public float animeDelay = 1;
    public Color color;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIEffectHelper instance = target as UIEffectHelper;

            graphic = instance.GetComponent<Graphic>();

        if (GUILayout.Button("Do Dissolve"))
        {
            graphic.material.SetFloat("_DissolvePower", Time.realtimeSinceStartup);

        }
    }

}
#endif
