using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RenderSortingTest : MonoBehaviour
{
    public GameObject gameObject1;
    public string layer1 = "Default";
    public int orderInLayer1 = 0;
    public int renderQueue1 = (int)RenderQueue.Geometry;
    public bool ZWrite1 = false;
    [Space(10)]
    public GameObject gameObject2;
    public string layer2 = "Default";
    public int orderInLayer2 = 0;
    public int renderQueue2 = (int)RenderQueue.Geometry;
    public bool ZWrite2 = false;
}

#if UNITY_EDITOR
[CustomEditor(typeof(RenderSortingTest))]
public class RenderSortingTestEditor:Editor
{
    private int _SrcBlend = Shader.PropertyToID("_SrcBlend");
    private int _DstBlend = Shader.PropertyToID("_DstBlend");
    private int _ZWrite = Shader.PropertyToID("_ZWrite");
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RenderSortingTest instance = target as RenderSortingTest;

        if (GUILayout.Button("Setup"))
        {
            UnityEngine.Rendering.CullMode
            var renderer = instance.gameObject1.GetComponentInChildren<Renderer>();
            renderer.sortingLayerID = SortingLayer.NameToID(instance.layer1);
            renderer.sortingOrder = instance.orderInLayer1;
            renderer.material.renderQueue = instance.renderQueue1;
            renderer.material.SetFloat(_SrcBlend, (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetFloat(_DstBlend, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetFloat(_ZWrite, instance.ZWrite1?1:0);

            renderer = instance.gameObject2.GetComponentInChildren<Renderer>();
            renderer.sortingLayerID = SortingLayer.NameToID(instance.layer2);
            renderer.sortingOrder = instance.orderInLayer2;
            renderer.material.renderQueue = instance.renderQueue2;
            renderer.material.SetFloat(_SrcBlend, (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetFloat(_DstBlend, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetFloat(_ZWrite, instance.ZWrite2 ? 1 : 0);
        }
    }
}
#endif



