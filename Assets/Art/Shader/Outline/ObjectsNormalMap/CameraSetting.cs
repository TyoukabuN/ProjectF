using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraSetting : PostProcessBase
{
    public DepthTextureMode depthTextureMode = DepthTextureMode.Depth;
    private new Camera camera;

    void Update()
    {
        camera = GetComponent<Camera>();
        if (camera)
            camera.depthTextureMode = depthTextureMode;
    }

}
