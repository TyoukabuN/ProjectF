using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class GenObjsNormalMap : PostProcessBase
{
    public Color OutlineColor = Color.black;

    private Camera camera;
    private CommandBuffer buffer;
    public override string ShaderName { get { return "TyoukabuN/DisplayNormalMap"; } }

    private void Awake()
    {
        camera = GetComponent<Camera>();
        camera.depthTextureMode = DepthTextureMode.DepthNormals;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (camera == null)
            return;

        if (!IsMaterialVaild())
            return;

        material.SetColor("_OutlineColor", OutlineColor);
        Graphics.Blit(source, destination, material, 0);
    }
}
