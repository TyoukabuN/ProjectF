using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetting : MonoBehaviour
{
    public DepthTextureMode DepthTextureMode = DepthTextureMode.DepthNormals;

    // Start is called before the first frame update
    void OnEnable()
    {
        var camera = GetComponent<Camera>();

        camera.depthTextureMode = DepthTextureMode;
    }
}
