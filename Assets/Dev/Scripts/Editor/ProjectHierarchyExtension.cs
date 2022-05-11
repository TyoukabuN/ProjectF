using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


[InitializeOnLoad]
public class ProjectHierarchyExtension
{
    static ProjectHierarchyExtension()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        //EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }
    static Dictionary<string, GUIContent> s_ProjectFolderDesc = new Dictionary<string, GUIContent>()
    {
        {"Assets/Art/Shader",new GUIContent("着色器")},

        {"Assets/Art/Shader/GUI",new GUIContent("UI相关效果")},
        {"Assets/Art/Shader/GUI/FadeAnimeViaTexture",new GUIContent("基于Tex的Image淡入淡出")},

        {"Assets/Art/Shader/HoleTwisty",new GUIContent("黑洞扭曲")},
        {"Assets/Art/Shader/Hologram",new GUIContent("全息投影")},
        {"Assets/Art/Shader/ScanLine",new GUIContent("圆形扫描线(非Projector)")},
        {"Assets/Art/Shader/DitheringFaderOut",new GUIContent("基于镜头距离的Dithering淡出效果")},
        {"Assets/Art/Shader/UVChecker",new GUIContent("UV检查用")},
        {"Assets/Art/Shader/Decal",new GUIContent("贴花/水印")},
        {"Assets/Art/Shader/ObjectsNormalMap",new GUIContent("后处理描边(法线差异+深度差异)")},
    };
    private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (!AssetDatabase.IsValidFolder(path))
        {
            return;
        }
        GUIContent content;
        if (s_ProjectFolderDesc.TryGetValue(path, out content))
        {
            EditorGUI.LabelField(selectionRect, content, GetLabelStyle());
        }
    }
    static GUIStyle label;
    static GUIStyle label2;
    static GUIStyle GetLabelStyle(bool isHierarchy = false)
    {
        GUIStyle style = null;
        if (!isHierarchy)
        {
            if (label == null)
            {
                label = new GUIStyle(EditorStyles.label);
                label.alignment = TextAnchor.MiddleRight;
                label.padding.right = 10;
                label.normal.textColor = Color.gray;
            }
            style = label;
        }
        else
        {
            if (label2 == null)
            {
                label2 = new GUIStyle(EditorStyles.label);
                label2.alignment = TextAnchor.MiddleRight;
                label2.padding.right = 10;
                label2.normal.textColor = Color.gray;
            }
            style = label2;
        }

        return style;
    }
}
