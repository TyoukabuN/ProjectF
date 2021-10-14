using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class ShaderHandle
{
    private Dictionary<string, Shader> m_shaders;
    
    private Dictionary<string, Material> m_material;

    public void SetupRenderAsset()
    {
#if UNITY_EDITOR
        string prefix = "Assets/Art/Shader";
        var allPaths = AssetDatabase.GetAllAssetPaths();
        var renderAssets =  ListPool<string>.Get();

        foreach (string path in allPaths)
            if (path.IndexOf(prefix) >= 0)
                renderAssets.Add(path);

        foreach (string path in renderAssets)
        {
            var asset = AssetDatabase.LoadAssetAtPath(path,typeof(Object));
            if (asset == null)
                continue;

            if (asset is Shader)
                AddShader(asset as Shader);
            else if(asset is Material)
                AddMaterial(asset as Material);
        }

        ListPool<string>.Release(renderAssets);
#endif
    }

    public void AddShader(Shader asset)
    {
        if (m_shaders.ContainsKey(asset.name))
            return;

        m_shaders.Add(asset.name, asset);
    }
    //Custom_UVChecker
    public void AddMaterial(Material asset)
    {
        if (m_material.ContainsKey(asset.name))
            return;

        m_material.Add(asset.name, asset);
    }

    public static Shader GetShader(string name)
    {
        Shader asset;
        if (current.m_shaders.TryGetValue(name,out asset))
            return asset;

        return null;
    }

    public static Material GetMaterial(string name)
    {
        Material asset;
        if (current.m_material.TryGetValue(name, out asset))
            return asset;

        return null;
    }
}
