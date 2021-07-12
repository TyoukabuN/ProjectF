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
    public class TCameraEditorWindow : TMeshEditorWindowBase<TCameraVertex, TCameraTrangle, TCameraMesh>
    {
        static TCameraEditorWindow current;

        private static void WindowSwitch(bool enabled)
        {
            if (!current)
            {
                current = EditorWindow.GetWindow<TCameraEditorWindow>();
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
                    current = EditorWindow.GetWindow<TCameraEditorWindow>();
                    current.Init();
                    enabled = true;
                }
            }
        }

        [MenuItem("TMesh/Camera Mesh %#T")]
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
            //剧情镜头校对
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                animBool_storyCamera.target = EditorGUILayout.BeginToggleGroup("剧情镜头校对", animBool_storyCamera.target);

                if (EditorGUILayout.BeginFadeGroup(animBool_storyCamera.faded))
                {
                    storyCamera = EditorGUILayout.ObjectField("剧情镜头", storyCamera, typeof(Camera), true) as Camera;
                    storyAvatar = EditorGUILayout.ObjectField("剧情Avatar", storyAvatar, typeof(Transform), true) as Transform;
                    //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    //{
                    //tcamTrangle = EditorGUILayout.ObjectField("三角形", tcamTrangle, typeof(TCameraTrangle), true) as TCameraTrangle;
                    //tcamVertex = EditorGUILayout.ObjectField("顶点", tcamVertex, typeof(TCameraVertex), true) as TCameraVertex;

                    DrawList("顶点", pTCameraVertex);
                    DrawList("三角形", pTCameraTrangle);
                    //sign = EditorGUILayout.Vector2Field("符号<Sign>", sign);
                    //EditorGUILayout.EndVertical();
                    //}
                    if (GUILayout.Button("校对"))
                    {
                        if (!storyCamera)
                            return;
                        if (!storyAvatar)
                            return;

                        cameraTrans = null;
                        Root = null;
                        AxiY = null;
                        AxiX = null;


                        if (Root == null)
                        {
                            var obj = new GameObject("Root");
                            Root = obj.transform;
                        }
                        Root.transform.localPosition = Vector3.zero;

                        if (AxiY == null)
                        {
                            var obj = new GameObject("AxiY");
                            obj.transform.SetParent(Root);
                            obj.transform.position = storyAvatar.position;
                            AxiY = obj.transform;
                        }
                        // AxiY.transform.localPosition = Vector3.zero;

                        if (AxiX == null)
                        {
                            var obj = new GameObject("AxiX");
                            obj.transform.SetParent(AxiY);
                            AxiX = obj.transform;
                        }
                        AxiX.transform.localPosition = Vector3.zero;

                        if (cameraTrans == null)
                        {
                            var gobj = new GameObject("tempCamera");
                            cameraTrans = gobj.transform;
                        }
                        cameraTrans.transform.SetParent(AxiX);
                        cameraTrans.transform.localPosition = Vector3.zero;
                        cameraTrans.transform.localRotation = Quaternion.identity;

                        AxiY.transform.localEulerAngles = new Vector3(0, storyCamera.transform.localEulerAngles.y, 0);
                        AxiX.transform.localEulerAngles = new Vector3(storyCamera.transform.localEulerAngles.x, 0, 0);

                        float disZ = Vector3.Distance(storyAvatar.position, storyCamera.transform.position);
                        var vecPctoP1 = storyAvatar.position - storyCamera.transform.position;
                        vecPctoP1.Normalize();
                        var angle1 = Vector3.Angle(vecPctoP1, storyCamera.transform.forward);
                        disZ *= Mathf.Cos(angle1 * Mathf.Deg2Rad);
                        cameraTrans.transform.localPosition = new Vector3(0, 0, -disZ);

                        AxiX.transform.position += storyCamera.transform.position - cameraTrans.transform.position;

                        var eularAngle = new Vector3(storyCamera.transform.localEulerAngles.x, storyCamera.transform.localEulerAngles.y, disZ);
                        var pivotPosition = AxiX.transform.localPosition;

                        bool isModifyTrangle = false;
                        bool isModifyVertex = false;
                        if (pTCameraTrangle.targets.Count > 0)
                        {
                            foreach (var trangle in pTCameraTrangle.targets)
                            {
                                foreach (TCameraVertex vertex in trangle.vertices)
                                {
                                    if (vertex)
                                    {
                                        vertex.EularAngle = eularAngle;
                                        vertex.PivotPosition = pivotPosition;
                                        isModifyVertex = true;
                                    }
                                }
                            }
                            isModifyTrangle = true;
                        }

                        if (pTCameraVertex.targets.Count > 0)
                        {
                            foreach (TCameraVertex vertex in pTCameraVertex.targets)
                            {
                                if (vertex)
                                {
                                    vertex.EularAngle = eularAngle;
                                    vertex.PivotPosition = pivotPosition;
                                }
                            }
                            isModifyVertex = true;
                        }


                        var res = EditorUtility.DisplayDialog("校对结束",
                            string.Format("avatar世界坐标:{0}\n剧情镜头角度:{1}\n剧情镜头偏移:{2}\n有无修改三角形：{3}\n有无修改顶点：{4}\n",
                            storyAvatar.transform.position.ToString("f5"), eularAngle.ToString("f5"), pivotPosition.ToString("f5"), isModifyTrangle ? "有" : "无", isModifyVertex ? "有" : "无"),
                            "ok");

                        if (res)
                        {
                            DestroyImmediate(Root.gameObject);
                        }
                    }

                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
            }
        }

    }

}

