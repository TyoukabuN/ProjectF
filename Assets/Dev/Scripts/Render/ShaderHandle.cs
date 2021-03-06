using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ShaderHandle : MonoSingleton<ShaderHandle>
{
    public readonly static int Realtime_ID = Shader.PropertyToID("_Realtime");
    public readonly static int FadeOutParam_ID = Shader.PropertyToID("_FadeOutParam");
    public readonly static int SrcBlend_ID = Shader.PropertyToID("_SrcFactor");
    public readonly static int DstBlend_ID = Shader.PropertyToID("_DstFactor");

    private Vector4 realtimeVec4 = Vector4.zero;
    // Start is called before the first frame update
    void Awake()
    {
        SetupRenderAsset();
    }

    // Update is called once per frame
    void Update()
    {
        Update_GlobalOption();
    }
}
