using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObstacleFadeOut : MonoBehaviour
{
    public float RecoverTimeStep = 0.5f;
    public float RecoverCounter = 0.0f;

    public Vector2 ExtentsXY = new Vector2(2, 2);
    public float fadeOutDuration = 1.0f;
    public float lengthOffset = 0f;

    private Camera camera;
    [SerializeField]private List<Renderer> Renderer = new List<Renderer>();
    private Vector3 tempVec3 = Vector3.zero;
    private Vector4 tempVec4 = Vector4.zero;

    private void Awake()
    {
        if (!camera)
        {
            camera = GetComponent<Camera>();
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

    public float length
    {
        get {
            if (PlayerController.current == null)
                return 0;

            var dir = transform.position - PlayerController.current.transform.position;
            return dir.magnitude + lengthOffset;
        }
    }

    public bool ScanObstacle()
    {
        if (!camera)
            return false;

        var position = transform.position + transform.forward * length / 2;
        tempVec3.Set(ExtentsXY.x, ExtentsXY.y, length / 2);
        var res = Physics.OverlapBox(position, tempVec3, transform.rotation);
        foreach (var collider in res)
        {
            var renderer = collider.GetComponent<Renderer>();
            if (renderer && !Renderer.Contains(renderer))
            {
                Renderer.Add(renderer);
                Transparent(renderer);
            }
        }
        for (int i=0;i<Renderer.Count;i++)
        {
            var renderer = Renderer[i];
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

    public void Transparent(Renderer renderer,bool recover = false)
    {
        if (!renderer)
            return;

        foreach (var mat in renderer.materials)
        {
            tempVec4.x = Time.realtimeSinceStartup;
            tempVec4.y = tempVec4.x + fadeOutDuration;
            tempVec4.z = 1;
            tempVec4.w = 0.2f;
            if (recover)
            {
                tempVec4.z = 0.2f;
                tempVec4.w = 1;
            }
            mat.SetVector(ShaderHandle.FadeOutParam_ID, tempVec4);
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
