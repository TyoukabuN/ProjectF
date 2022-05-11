using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GenObjsNormalMap : MonoBehaviour
{
    public Camera RefCamera;
    public DepthTextureMode DepthTextureMode = DepthTextureMode.DepthNormals;
    public Shader Shader;

    //private Material m_Material;
    //public Material material{
    //    get {
    //        if (shader == null)
    //            return null;
    //        if (m_Material == null)
    //            m_Material = new Material(shader);

    //        return m_Material;
    //    }
    //}

    private CommandBuffer buffer;
    private void Awake()
    {
        if (RefCamera == null)
            RefCamera = this.TryGetComponent<Camera>();

        RefCamera.depthTextureMode = DepthTextureMode;
    }
    void Update()
    {
        if (Shader == null)
            return;

        if (RefCamera == null)
            RefCamera = this.TryGetComponent<Camera>();
        
        //RefCamera.SetReplacementShader(Shader,null);
    }
}
