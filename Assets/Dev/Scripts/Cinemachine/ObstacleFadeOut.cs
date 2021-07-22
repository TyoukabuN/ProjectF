using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObstacleFadeOut : MonoBehaviour
{
    /// <summary>
    /// 检测步长
    /// </summary>
    public float RecoverTimeStep = 0.5f;
    /// <summary>
    /// 计时器
    /// </summary>
    public float RecoverCounter = 0.0f;

    /// <summary>
    /// 触发冷却信息
    /// </summary>
    public struct ColdDownInfo
    {
        public Renderer renderer;
        public float timeStamp;
        public ColdDownInfo(Renderer renderer, float timeStamp)
        {
            this.renderer = renderer;
            this.timeStamp = timeStamp;
        }
    };
    /// <summary>
    /// 触发冷却列表
    /// </summary>
    public List<ColdDownInfo> coldDownList = new List<ColdDownInfo>();
    /// <summary>
    /// 冷切时间
    /// </summary>
    public float TriggerInterval = 0.2f;

    /// <summary>
    /// 检测用正方体在【XY】方向上的大小
    /// </summary>
    public Vector2 ExtentsXY = new Vector2(2, 2);
    /// <summary>
    /// 动画时间
    /// </summary>
    public float animeTime = 1.0f;
    /// <summary>
    /// 检测用正方体在【Z】方向上的大小的偏移
    /// </summary>
    public float lengthOffset = 0f;

    /// <summary>
    /// 摄像机
    /// </summary>
    private Camera camera;

    /// <summary>
    /// 动画中Renderer对象
    /// </summary>
    [SerializeField]private List<Renderer> Renderer = new List<Renderer>();

    public float OriginAlpha = 1.0f;
    public float TargetAlpha = 0.2f;

    private Vector3 tempVec3 = Vector3.zero;
    private Vector4 tempVec4 = Vector4.zero;

    public LayerMask targetLayout = 0;

    private void Awake()
    {
        if (!camera)
        {
            camera = GetComponent<Camera>();
        }

        if (targetLayout == 0)
        { 
            targetLayout = 1 << LayerMask.NameToLayer("Default");
        }
    }

    void Update()
    {
        if (!camera)
            return;

        RecoverCounter += Time.time;
        if (RecoverCounter >= RecoverTimeStep)
        {
            RecoverCounter = 0.0f;
            ScanObstacle();
        }

        for (int i = 0; i < coldDownList.Count; i++)
        {
            var info = coldDownList[i];
            if (Time.realtimeSinceStartup - info.timeStamp >= TriggerInterval)
            {
                coldDownList.RemoveAt(i);
                i--;
            }
        }
    }

#if UNITY_EDITOR
    private GUIStyle GizmosGUIStyle = null;
    void OnDrawGizmos()
    {
        if (!camera)
            return;

        var position = transform.position + transform.forward * (length - 0.5f);

        Handles.DrawCube(1,position, transform.rotation, ExtentsXY.x);
    }
#endif

    /// <summary>
    /// 检测用正方体在【Z】方向上的大小
    /// </summary>
    public float length
    {
        get {
            if (PlayerController.current == null)
                return 0;

            var dir = transform.position - PlayerController.current.transform.position;
            return dir.magnitude + lengthOffset;
        }
    }

    /// <summary>
    /// 检测场景中可透明化的对象
    /// </summary>
    /// <returns></returns>
    public bool ScanObstacle()
    {
        if (!camera)
            return false;

        var position = transform.position + transform.forward * length / 2;
        tempVec3.Set(ExtentsXY.x, ExtentsXY.y, length / 2);
        var res = Physics.OverlapBox(position, tempVec3, transform.rotation, targetLayout);
        foreach (var collider in res)
        {
            var renderer = collider.GetComponent<Renderer>();
            if (coldDownList.Any(info => info.renderer.gameObject.GetInstanceID() == renderer.gameObject.GetInstanceID()))
                continue;

            if (renderer && !Renderer.Contains(renderer))
            {
                Renderer.Add(renderer);
                Transparent(renderer);
            }
        }
        for (int i=0;i<Renderer.Count;i++)
        {
            var renderer = Renderer[i];
            if (coldDownList.Any(info => info.renderer.gameObject.GetInstanceID() == renderer.gameObject.GetInstanceID()))
                continue;

            if (!res.Any(collider => collider.gameObject.GetInstanceID() == renderer.gameObject.GetInstanceID()))
            {
                Transparent(renderer, true);
                Renderer.RemoveAt(i);
                i--;
            }
        }

        Debug.Log(res.Length);
        return true;
    }

    /// <summary>
    /// 透明化
    /// </summary>
    /// <param name="renderer">Renderer</param>
    /// <param name="recover">这次为恢复透明度操作</param>
    public void Transparent(Renderer renderer,bool recover = false)
    {
        if (!renderer)
            return;

        coldDownList.Add(new ColdDownInfo(renderer, Time.realtimeSinceStartup));

        foreach (var mat in renderer.materials)
        {
            tempVec4.x = Time.realtimeSinceStartup;
            tempVec4.y = tempVec4.x + animeTime;
            tempVec4.z = OriginAlpha;
            tempVec4.w = TargetAlpha;
            if (recover)
            {
                tempVec4.z = TargetAlpha;
                tempVec4.w = OriginAlpha;
            }

            mat.SetVector(ShaderHandle.FadeOutParam_ID, tempVec4);
            mat.SetInt(ShaderHandle.SrcBlend_ID, (int)BlendMode.SrcAlpha);
            mat.SetInt(ShaderHandle.DstBlend_ID, (int)BlendMode.OneMinusSrcAlpha);
            mat.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObstacleFadeOut))]
public class ObstacleFadeOutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ObstacleFadeOut instance = target as ObstacleFadeOut;

        if (!instance)
            return;

        if (GUILayout.Button("检测"))
        {
            instance.ScanObstacle();
        }
    }
}
#endif
