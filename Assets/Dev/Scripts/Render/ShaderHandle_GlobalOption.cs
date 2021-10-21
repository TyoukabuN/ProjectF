using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ShaderHandle
{
    void Update_GlobalOption()
    {
        UpdateShaderTime();
    }

    void UpdateShaderTime()
    {
        float t = Time.realtimeSinceStartup;
        realtimeVec4.Set(t / 20, t, t * 2, t * 3);
        Shader.SetGlobalVector(Realtime_ID, realtimeVec4);
    }

    private Texture2D m_ParamTexture;
    public Texture2D ParamTexture
    {
        get {
            return m_ParamTexture;
        }
    }
    public void Global_InitParamTexture()
    {
        if (m_ParamTexture != null)
        {
            GameObject.DestroyImmediate(m_ParamTexture);
            m_ParamTexture = null;
        }
        m_ParamTexture = new Texture2D(32, 32);
        Shader.SetGlobalTexture("_ParamTexture", m_ParamTexture);
    }
    public void Global_SetParamTexture()
    {
        Texture2D tex = new Texture2D(32, 32);
        Shader.SetGlobalTexture("_ParamTexture", tex);
    }
}
