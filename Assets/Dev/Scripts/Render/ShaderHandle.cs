using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderHandle : MonoBehaviour
{
    public readonly static int Realtime_ID = Shader.PropertyToID("_Realtime");
    public readonly static int FadeOutParam_ID = Shader.PropertyToID("_FadeOutParam");
    public readonly static int SrcBlend_ID = Shader.PropertyToID("_SrcFactor");
    public readonly static int DstBlend_ID = Shader.PropertyToID("_DstFactor");

    private Vector4 realtimeVec4 = Vector4.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.realtimeSinceStartup;
        realtimeVec4.Set(t / 20, t, t * 2, t * 3);
        Shader.SetGlobalVector(Realtime_ID, realtimeVec4);
    }
}
