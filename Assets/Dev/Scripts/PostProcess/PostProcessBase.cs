using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessBase : MonoBehaviour
{
    protected Shader shader;
    protected Material material;

    public virtual string ShaderName {  get { return string.Empty; } }

    // Start is called before the first frame update
    protected bool IsShaderVaild()
    {
#if UNITY_EDITOR
        if (shader == null)
            shader = Shader.Find(ShaderName);
#endif
        if (shader == null)
            shader = ShaderHandle.GetShader(ShaderName);
        if (shader == null)
            return false;

        return true;
    }

    protected bool IsMaterialVaild()
    {
        if (!IsShaderVaild())
            return false;

        if (material == null)
            material = new Material(shader);
        if (material == null)
            return false;

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
