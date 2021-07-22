using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SandFadeHelper : EditorWindow
{
    [MenuItem("Tools/VFX/沙化效果测试工具")]
    static void Open()
    {
        var handle = GetWindow<SandFadeHelper>();
        handle.init();
    }

    void init()
    {
        EditorApplication.playModeStateChanged -= playModeStateChanged;
        EditorApplication.playModeStateChanged += playModeStateChanged;

    }

    private void playModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredPlayMode)
        {
            mpb = null;
            DestroyImmediate(tempEff);
            tempEff = null;
        }
    }

    public GameObject modelGobj;
    public float modelHeight = 0;
    public Color sandColor = Color.white;
    public float delay = 0;
    public float time = 2.5f;
    public float posY = 0;
    public float height = 3;
    [Range(0f,3f)]
    public float sandColorPctOffset = 0;
    public uint sandUIntColor = 0;

    public uint sandUInt2ARGBColor = 0;

    public AnimationClip animeClip;
    public GameObject effectPref;
    public Transform effectRoot;
    private GameObject tempEff;
    private float delayDisplayTime = 0;
    private float counterOfDisplay = 0;


    private static Vector4 s_temp_v4 = Vector4.zero;
    private static int _SandFadeParameter_ID = Shader.PropertyToID("_SandFadeParameter");
    private static int _MainTex_ID = Shader.PropertyToID("_MainTex");
    private static int _SandColor_ID = Shader.PropertyToID("_SandColor");
    private static int _SandColorPctOffset_ID = Shader.PropertyToID("_SandColorPctOffset");
    private static Vector4 _RealtimeV4 = Vector4.zero;

    private MaterialPropertyBlock mpb;

    public readonly static int objectPickerControlID = 233;

    void OnGUI()
    {
        if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == objectPickerControlID)
        {
            modelGobj = EditorGUIUtility.GetObjectPickerObject() as GameObject;
            Repaint();
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            modelGobj = EditorGUILayout.ObjectField("模型", modelGobj, typeof(GameObject),true) as GameObject;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            animeClip = EditorGUILayout.ObjectField("动画Clip：", animeClip, typeof(AnimationClip), true) as AnimationClip;
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            effectPref = EditorGUILayout.ObjectField("特效：", effectPref, typeof(GameObject), true) as GameObject;
            effectRoot = EditorGUILayout.ObjectField("特效挂点：", effectRoot, typeof(Transform), true) as Transform;
            delayDisplayTime = EditorGUILayout.FloatField("显示延迟：", delayDisplayTime);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            sandColor = EditorGUILayout.ColorField("沙子颜色：", sandColor);
            sandColorPctOffset = EditorGUILayout.FloatField("沙子颜色变化的阻力：", sandColorPctOffset);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            delay = EditorGUILayout.FloatField("延迟：", delay);
            time = EditorGUILayout.FloatField("沙化时间：", time);
            posY = EditorGUILayout.FloatField("模型底部的世界坐标的Y：", posY);
            height = EditorGUILayout.FloatField("模型的高度：", height);
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("播放"))
        {
            if (!Application.isPlaying)
            {
                if (EditorUtility.DisplayDialog("Tips", "请先运行游戏", "运行", "取消"))
                {
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                }
                return;
            }

            if (!CheckModelIsExistAndTips())
            {
                return;
            }

            mpb = mpb==null?new MaterialPropertyBlock(): mpb;

            int res = ChangeMaterial() ? PassPropertiesToMaterial() ? GenerateEffect() ? PlayDeadAnime() ? 0 : 4 : 3 : 2 : 1;

            Debug.Log("play reture code: " + res);
        }


        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {

            EditorGUILayout.BeginHorizontal();
            {
                
                sandUIntColor = uint.Parse(EditorGUILayout.TextField("沙子UINT格式颜色：", sandUIntColor.ToString()));
                if (GUILayout.Button("COPY"))
                {
                    EditorGUIUtility.systemCopyBuffer = sandUIntColor.ToString();
                }
                if (GUILayout.Button("UNIT"))
                {
                    ColorToUint(sandColor, out sandUIntColor);
                    Repaint();
                }
                if (GUILayout.Button("ARGB"))
                {
                    UintToColor(sandUIntColor, out sandColor);
                    Repaint();
                }

            EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.FloatField("高度：", modelHeight);
                if (GUILayout.Button("复制"))
                {
                    EditorGUIUtility.systemCopyBuffer = modelHeight.ToString();
                }
                if (GUILayout.Button("计算模型高度"))
                {
                    CalcularModelHeight();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        //GUILayout.Label("Tips：");
        GUILayout.TextArea("Tips①:在不知道模型高度时,请在死亡动画结束的时候,计算模型高度\n" +
                            "Tips②:从UINT转到ARGB,会赋值给上面的沙子颜色\n" +
                            "Tips③:COPY的是UNIT颜色,ARGB可以在上面沙子颜色那复制");

    }

    void Update()
    {
        float t = Time.realtimeSinceStartup;
        _RealtimeV4.Set(t / 20, t, t * 2, t * 3);
        Shader.SetGlobalVector(ShaderHandle.Realtime_ID, _RealtimeV4);


        if (delayDisplayTime > 0 && counterOfDisplay > 0)
        {
            counterOfDisplay -= Time.deltaTime;
            if (counterOfDisplay <= 0)
            {
                effectRoot.gameObject.SetActive(false);
                effectRoot.gameObject.SetActive(true);
            }
        }
    }

    public bool PassPropertiesToMaterial()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = modelGobj.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!skinnedMeshRenderer)
        {
            return false;
        }

        s_temp_v4.x = Time.realtimeSinceStartup + this.delay;
        s_temp_v4.y = s_temp_v4.x + this.time;
        s_temp_v4.z = this.posY;
        s_temp_v4.w = this.height;
        mpb.SetVector(_SandFadeParameter_ID, s_temp_v4);
        mpb.SetColor(_SandColor_ID, sandColor);
        mpb.SetFloat(_SandColorPctOffset_ID, sandColorPctOffset);
        skinnedMeshRenderer.SetPropertyBlock(mpb);

        return true;
    }


    public bool CalcularModelHeight()
    {
        if (!CheckModelIsExistAndTips())
        {
            return false;
        }

        var render = modelGobj.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!render)
            return false;

        var mesh = new Mesh();
        render.BakeMesh(mesh);

        Vector2 rangeY = new Vector2(float.MaxValue, float.MinValue);
        foreach (var vertex in mesh.vertices)
        {
            var wpos = render.transform.TransformPoint(vertex);
            if (wpos.y < rangeY.x)
            {
                rangeY.x = wpos.y;
            }

            if (wpos.y > rangeY.y)
            {
                rangeY.y = wpos.y;
            }
        }

        modelHeight = rangeY.y - rangeY.x;
        height = modelHeight;
        Repaint();

        return true;
    }
    public bool ChangeMaterial()
    {
        if (!CheckModelIsExistAndTips())
        {
            return false;
        }

        var render = modelGobj.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!render)
            return false;

        if (render.material && render.material.shader.name == "X_Shader/C_Charactar/SandFadeEffect")
            return true;

        string assetPath = "Assets/Shaders/M_Materials/SandFadeEffect.mat";
        var sandMat = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material)) as Material;
        if (!sandMat)
            return false;

        mpb.SetTexture(_MainTex_ID, render.material.GetTexture(_MainTex_ID));
        render.material = sandMat;
        render.SetPropertyBlock(mpb);

        return true;
    }

    public bool GenerateEffect()
    {
        counterOfDisplay = delayDisplayTime;

        if (!effectPref)
            return true;

        if (tempEff)
        {
            tempEff.SetActive(delayDisplayTime <= 0);
            return true;
        }

        var eff = GameObject.Instantiate(effectPref) as GameObject;
        if (eff)
        {
            tempEff = eff;
            tempEff.transform.SetParent(effectRoot);
            tempEff.transform.localPosition = Vector3.zero;

            tempEff.SetActive(delayDisplayTime <= 0);
        }
        return true;
    }
    public bool PlayDeadAnime()
    {
        if (!CheckModelIsExistAndTips())
        {
            return false;
        }

        var animator = modelGobj.GetComponent<Animator>();
        if (animeClip)
        {
            if (animator)
                DestroyImmediate(animator);

            var animation = modelGobj.GetComponent<Animation>();
            if (!animation)
                animation = modelGobj.AddComponent<Animation>();

            if (!animeClip.legacy)
                animeClip.legacy = true;

            if(!animation.GetClip("dead"))
                animation.AddClip(animeClip, "dead");

            if (animation.isPlaying)
            {
                animation.Rewind();
            } else
            { 
                animation.Play("dead");
            }
            return true;
        }

        if (!animator)
            return false;

        animator.SetTrigger("deathTrigger");
        animator.SetInteger("deathInt", 1);
        return true;
    }

    public bool CheckModelIsExistAndTips()
    {
        if (modelGobj)
            return true;

        if (EditorUtility.DisplayDialog("Tips", "大哥,先放个模型进来,求求了!", "知道了", "就不"))
        {
            EditorGUIUtility.ShowObjectPicker<GameObject>(modelGobj, true, string.Empty, objectPickerControlID);
        }
        return false;
    }

    public void UintToColor(uint value, out Color color)
    {
        color.a = (float)((value & 0xff000000) >> 24) / 0xff;
        color.r = (float)((value & 0xff0000) >> 16) / 0xff;
        color.g = (float)((value & 0xff00) >> 8) / 0xff;
        color.b = (float)((value & 0xff)) / 0xff;
    }

    public void ColorToUint(Color color, out uint value)
    {
        var a = (uint)(color.a * 0xff);
        var r = (uint)(color.r * 0xff);
        var g = (uint)(color.g * 0xff);
        var b = (uint)(color.b * 0xff);
        value = (a << 24) | (r << 16) | (g << 8) | b;
    }

}
