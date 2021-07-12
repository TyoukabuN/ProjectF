using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class TCameraToolBar : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static TCameraToolBar()
    { 
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;

        EditorApplication.hierarchyChanged -= OnSceneChanged;
        EditorApplication.hierarchyChanged += OnSceneChanged;
    }

    static void OnSceneChanged()
    {
        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
    }

    public static int SelectedTool = 0;
    static void OnSceneGUI(SceneView sceneView)
    {
#if TYOU_LAB
        Handles.BeginGUI();

        var position = sceneView.position;
        GUILayout.BeginArea(new Rect(0, position.height - 35, position.width, 20), EditorStyles.toolbar);

        SelectedTool = GUILayout.SelectionGrid(
            SelectedTool,
            new string[] { "顶点预设", "三角形预设", "网格预设" },
            3,
            EditorStyles.toolbarButton,
            GUILayout.Width(300)
            ) ;

        GUILayout.EndArea();
        Handles.EndGUI();
#endif

    }
}
