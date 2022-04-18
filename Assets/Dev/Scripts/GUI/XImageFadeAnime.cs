using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Graphic))]
public class XImageFadeAnime : BaseMeshEffect
{
    public float duration = 0.5f;
    public float delay = 0.0f;
    public bool blendAlpha = false;
    public OnCompleteEvent onComplete = new OnCompleteEvent();

    private Graphic graphic;
    private Vector4 s_temp_v4 = Vector4.zero;
    private string matName = "FadeAnimeViaTexture";
    private int timer_id = -1;

    [SerializeField]
    Sprite sprite;

    protected override void OnDisable()
    {
        ClearTimer();
    }

    void AddTimer(float second)
    {
        TimerManager.DelTimer(timer_id);
        timer_id = TimerManager.AddTimer(OnComplete, second);
    }

    public void ClearTimer()
    {
        TimerManager.DelTimer(this.timer_id);
        this.timer_id = -1;
    }
    public bool VaildateMaterial()
    {
        if (graphic == null)
            return false;

        if (graphic.material && graphic.material.name.IndexOf(matName) >= 0)
            return true;

        var mat = ShaderHandle.GetMaterial(matName);
        if (!mat) {
            Debug.LogWarning(string.Format("Find not material: <color=yellow>{0}</color> from XShader", matName));
            return false;
        }

        graphic.material = mat;
        return true;
    }

    public bool VaildateSprite()
    {
        if (!(graphic is Image))
            return false;

        sprite = (graphic as Image).sprite;

        return sprite != null;
    }

    public bool VaildateGraphic()
    {
        if (graphic == null)
            graphic = gameObject.GetComponent<Graphic>();

        return graphic != null;
    }
    public bool Vaildate()
    {
        //VaildateSprite();
        
        if (!VaildateGraphic())
            return false;

        if (!VaildateMaterial())
            return false;

        graphic.SetVerticesDirty();

        return true;
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="fadeIn">是否淡入</param>
    /// <param name="WithCompleleCall">是否需要完成回调</param>
    /// <returns></returns>
    public bool Play(bool fadeIn = true,bool WithCompleleCall = false)
    {
        if (WithCompleleCall)
            return PlayWithCompleleCallBack(duration, delay, fadeIn);

        return Play(duration, delay, fadeIn);
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="duration">动画时间</param>
    /// <param name="delay">延迟动画播放的秒数</param>
    /// <param name="fadeIn">是否淡入</param>
    /// <returns></returns>
    public bool Play(float duration,float delay = 0 ,bool fadeIn = true)
    {
        if (!Vaildate())
            return false;

        s_temp_v4.x = Time.realtimeSinceStartup + delay;
        s_temp_v4.y = s_temp_v4.x + duration;
        s_temp_v4.z = fadeIn?1:0;
        s_temp_v4.w = blendAlpha ? 1 :0;

        graphic.material.SetVector("_FadeParam", s_temp_v4);

        return true;
    }

    /// <summary>
    /// 播放动画带完成回调
    /// </summary>
    /// <param name="duration">动画时间</param>
    /// <param name="delay">延迟动画播放的秒数</param>
    /// <param name="fadeIn">是否淡入</param>
    /// <returns></returns>
    public bool PlayWithCompleleCallBack(float duration, float delay = 0, bool fadeIn = true)
    {
        if (Play(duration, delay, fadeIn))
        { 
            AddTimer(duration + delay);
            return true;
        }

        return false;
    }

    void OnComplete()
    {
        if (!IsActive())
            return;

        onComplete.Invoke();
    }

    Vector4 s_uv1 = new Vector4(0, 0, 1, 1);
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() && graphic != null || !Application.isPlaying)
            return;

        if (graphic && graphic.canvas)
            graphic.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;

        int vertCount = vh.currentVertCount;
        var vert = new UIVertex();
        Vector4 uv1 = s_uv1;//sprite != null ? DataUtility.GetOuterUV(sprite) : Vector4.zero;
        Debug.Log(uv1);
        for (int i = 0; i < vertCount; ++i)
        {
            vh.PopulateUIVertex(ref vert, i);

            if (i == 0)
                vert.uv1.Set(uv1.x, uv1.y);
            else if (i == 1)
                vert.uv1.Set(uv1.x, uv1.w);
            else if (i == 2)
                vert.uv1.Set(uv1.z, uv1.w);
            else if (i == 3)
                vert.uv1.Set(uv1.z, uv1.y);

            vh.SetUIVertex(vert, i);
        }
    }

    public class OnCompleteEvent : UnityEvent {  }
}




#if UNITY_EDITOR

[CustomEditor(typeof(XImageFadeAnime))]
public class ImageFadeAnimeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        XImageFadeAnime instance = target as XImageFadeAnime;

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("For Test");
        if (GUILayout.Button("Do FadeIn"))
        {
            instance.Play(true);
        }

        if (GUILayout.Button("Do FadeOut"))
        {
            instance.Play(false);
        }

        if (GUILayout.Button("Do FadeIn WithCompleleCallBack"))
        {
            instance.onComplete.AddListener(() => Debug.Log("FadeIn Finish!"));
            instance.Play(true,true);
        }

        if (GUILayout.Button("Do FadeOut WithCompleleCallBack"))
        {
            instance.onComplete.AddListener(() => Debug.Log("FadeOut Finish!"));
            instance.Play(false,true);
        }
        EditorGUILayout.EndVertical();
    }

}
#endif
