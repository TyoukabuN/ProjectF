using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Reflection;
using System;
using util = TMesh.TCameraEditorUtility;
using Object = UnityEngine.Object;


namespace TMesh
{
    public class TEventMeshEditorWindow : TMeshEditorWindowBase<TEventVertex,TEventTrangle,TEventMesh>
    {
        static TEventMeshEditorWindow current;

        private static void WindowSwitch(bool enabled)
        {
            if (!current)
            {
                current = EditorWindow.GetWindow<TEventMeshEditorWindow>();
                current.Init();
            }
            else
            {
                try
                {
                    current.Close();
                    current = null;
                }
                catch
                {
                    current = EditorWindow.GetWindow<TEventMeshEditorWindow>();
                    current.Init();
                    enabled = true;
                }
            }
        }

        [MenuItem("TMesh/Event Mesh %#G")]
        static void Open()
        {
            if (!current)
            {
                WindowSwitch(true);
            }
            else
            {
                WindowSwitch(false);
            }
        }

        public static AnimBoolHandle animBool_storyCamera;

        protected override void OnEnable()
        {
            base.OnEnable();
            animBool_storyCamera = new AnimBoolHandle("TCameraMesh_animBool_storyCamera", true);
            animBool_storyCamera.valueChanged.AddListener(Repaint);

            MeshVisualize(true);
            MeshObjectIconVisualize(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            MeshVisualize(false);
            MeshObjectIconVisualize(false);
        }

        //DrawMeshConstructTool();
        //👇👇👇
        //OnDrawTool();
        //👇👇👇
        //DrawOtherTool();
        protected override void OnDrawTool()
        {

        }

    }
}

