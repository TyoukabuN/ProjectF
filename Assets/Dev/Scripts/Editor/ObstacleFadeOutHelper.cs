using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ObstacleFadeOutHelper : EditorWindow
{
    [MenuItem("Tools/VFX/视线阻碍物虚化效果帮助工具")]
    static void Open()
    {
        var handle = GetWindow<ObstacleFadeOutHelper>();
        handle.init();
    }

    void init()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        Selection.selectionChanged += OnSelectionChanged;
    }


    public void OnSelectionChanged()
    {
        selectionInfo.SetDirty();
        if (ErrorFontGUIStyle != null)
        {
            ErrorFontGUIStyle.fontSize = selectionInfo.fontSize;
        }
        Repaint();
    }

    private SelectedObjectInfo selectionInfo = new SelectedObjectInfo();

    struct SelectedObjectInfo
    {
        public GameObject gameObject;
        public MeshFilter meshfilter;
        public MeshRenderer renderer;
        public MeshCollider meshCollider;
        public string error;
        public int fontSize;
        public bool isShaderSupport
        {
            get {
                if (!string.IsNullOrEmpty(error))
                    return false;

                if (!gameObject)
                    return false;

                if (!renderer)
                    renderer = gameObject.GetComponentInChildren<MeshRenderer>();

                if (!renderer || !renderer.sharedMaterial)
                    return false;

                var isSceneShader = renderer.sharedMaterial.shader.name.IndexOf("X_Shader/B_Scene/") >= 0;

                return isSceneShader;
            }
        }
        public void Clear()
        {
            gameObject = null;
            meshfilter = null;
            renderer = null;
            meshCollider = null;
            error = string.Empty;
            fontSize = 11;
        }

        public void SetDirty()
        {
            Clear();

            gameObject = Selection.activeGameObject;
            if (!gameObject)
            { 
                return;
            }

            //Debug.Log(PrefabUtility.GetCorrespondingObjectFromSource(gameObject));
            //Debug.Log(PrefabUtility.GetPrefabInstanceHandle(gameObject));
            //Debug.Log(gameObject.scene);
            //Debug.Log(PrefabUtility.GetPrefabInstanceStatus(gameObject));
            //Debug.Log(PrefabUtility.GetPrefabAssetType(gameObject));
            //Debug.Log("==========================================================");
            var prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            var prefabAssetType = PrefabUtility.GetPrefabAssetType(gameObject);
            //if (prefabAssetType != PrefabAssetType.MissingAsset && prefabInstanceStatus == PrefabInstanceStatus.NotAPrefab)
            if (PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == null && 
                PrefabUtility.GetPrefabInstanceHandle(gameObject) == null &&
                prefabInstanceStatus == PrefabInstanceStatus.NotAPrefab &&
                prefabAssetType != PrefabAssetType.NotAPrefab)
            {
                error = "不能选中预制体";
                return;
            }


            meshfilter = gameObject.GetComponentInChildren<MeshFilter>();
            renderer = gameObject.GetComponentInChildren<MeshRenderer>();
            meshCollider = gameObject.GetComponentInChildren<MeshCollider>();
        }

        public bool Vaild()
        {
            if (!(gameObject && meshfilter && meshCollider))
                return false;

            if (meshCollider.gameObject.layer != LayerMask.NameToLayer("Layer29"))
                return false;

            return true;
        }
        public bool DoEffectSetup()
        {
            if (Vaild())
                return true;

            if (meshfilter == null)
                return false;

            if (meshfilter.sharedMesh == null)
                return false;

            if (!meshCollider)
                meshCollider = renderer.gameObject.AddComponent<MeshCollider>();

            if (meshCollider)
            { 
                meshCollider.sharedMesh = meshfilter.sharedMesh;
                meshCollider.gameObject.layer = LayerMask.NameToLayer("Layer29");
            }

            return Vaild();
        }

    }


    private GUIStyle ErrorFontGUIStyle = null;

    // Update is called once per frame
    void OnGUI()
    {
        GUILayout.Label("选中对象信息:");
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.ObjectField(selectionInfo.gameObject, typeof(GameObject), true);
            EditorGUILayout.ObjectField(selectionInfo.meshfilter, typeof(MeshFilter), true);
            EditorGUILayout.ObjectField(selectionInfo.meshCollider, typeof(MeshCollider), true);
            EditorGUILayout.Toggle("Shader是否支持虚化效果： ", selectionInfo.isShaderSupport,GUILayout.ExpandWidth(true));
            EditorGUILayout.Toggle("是否已支持虚化效果： ", selectionInfo.Vaild());
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("设置成可成可虚化对象"))
        {
            if (!selectionInfo.DoEffectSetup())
            {
                selectionInfo.error = "设置失败！";
                selectionInfo.fontSize += 2;
                if (ErrorFontGUIStyle != null)
                {
                    ErrorFontGUIStyle.fontSize = selectionInfo.fontSize;
                }
            }
            else
            {
                //EditorUtility.DisplayDialog("TIPS", "设置成功","ok");
            }
        }

        if (!string.IsNullOrEmpty(selectionInfo.error))
        { 
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (ErrorFontGUIStyle == null)
                {
                    GUIStyle style = new GUIStyle();
                    style.richText = true;
                    style.fontSize = 10;
                    ErrorFontGUIStyle = style;
                }
                GUILayout.Label(string.Format("<color=red>错误：{0}</color>" , selectionInfo.error), ErrorFontGUIStyle);
            }
        }
    }
}
