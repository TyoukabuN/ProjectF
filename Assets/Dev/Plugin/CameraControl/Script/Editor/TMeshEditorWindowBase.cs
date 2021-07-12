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
    public abstract class TMeshEditorWindowBase<TVertex, Trangle, Mesh> : EditorWindow  where TVertex: TMesh.TVertex where Trangle:TTrangle where Mesh:TMesh.TMeshBase
    {
        protected void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            VertexProcesser.Dispose();
            TrangleProcesser.Dispose();
        }

        public TMeshEditorWindowBase()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        protected void Init()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        protected void OnUndoRedoPerformed()
        {
            Repaint();
        }

        static Vector2 scroll_value = Vector2.zero;

        string checkerDisplayKey = "TCameraMesh Checker Display Dialog";

        public static AnimBoolHandle animBool_vertex;
        public static AnimBoolHandle animBool_trangle;
        public static AnimBoolHandle animBool_editor;
        public static AnimBoolHandle animBool_other;

        public static bool FixedCheckerPositionToMeshSurface = true;
        protected virtual void OnEnable()
        {
            animBool_vertex = new AnimBoolHandle("TCameraMesh_animBool_vertex", false);
            animBool_vertex.valueChanged.AddListener(Repaint);
            animBool_trangle = new AnimBoolHandle("TCameraMesh_animBool_trangle", false);
            animBool_trangle.valueChanged.AddListener(Repaint);
            animBool_editor = new AnimBoolHandle("TCameraMesh_animBool_editor", true);
            animBool_editor.valueChanged.AddListener(Repaint);

            animBool_other = new AnimBoolHandle("TCameraMesh_animBool_other", false);
            animBool_other.valueChanged.AddListener(Repaint);
        }

        protected virtual void OnDisable()
        { 

        }

        public static bool vertexEditorMode = false;
        public static bool trangleEditorMode = false;


        protected void OnSceneGUI(SceneView sceneView)
        {
            OnEditorModeHotkey(sceneView);
            OnEditorModeSelect(sceneView);
        }
        /// <summary>
        /// 编辑模式下的快捷键
        /// </summary>
        /// <param name="sceneView"></param>
        protected static void OnEditorModeHotkey(SceneView sceneView)
        {
            if (!vertexEditorMode)
                return;

            int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            if (Event.current.type == EventType.KeyDown &&
            Event.current.control == true &&
             Event.current.keyCode == KeyCode.Q
            )
            {
                GeneralVertex<TVertex>();
            }
            if (Event.current.type == EventType.KeyDown &&
                Event.current.control == true &&
                 Event.current.keyCode == KeyCode.X
                )
            {
                CombineVerticesToTrangle();
            }
        }

        /// <summary>
        /// 编辑模式下的选择过滤
        /// </summary>
        /// <param name="sceneView"></param>
        protected static void OnEditorModeSelect(SceneView sceneView)
        {
            if (Selection.objects.Length <= 0)
            {
                return;
            }
            List<Object> temp = new List<Object>();

            if (vertexEditorMode)
            {
                for (int i = 0; i < Selection.objects.Length; i++)
                {
                    Object obj = Selection.objects[i];
                    if (obj.name.LastIndexOf("CVertex") >= 0)
                    {
                        temp.Add(obj);
                    }
                }

                if (Selection.activeObject != null && !temp.Contains(Selection.activeObject))
                {
                    temp.Add(Selection.activeObject);
                }
            }

            if (trangleEditorMode)
            {
                for (int i = 0; i < Selection.objects.Length; i++)
                {
                    Object obj = Selection.objects[i];
                    if (obj.name.LastIndexOf("CTrangle") >= 0)
                    {
                        temp.Add(obj);
                    }
                }

                if (Selection.activeObject != null && !temp.Contains(Selection.activeObject))
                {
                    temp.Add(Selection.activeObject);
                }
            }

            if (vertexEditorMode || trangleEditorMode)
            {
                Selection.objects = temp.ToArray();

                if (Selection.activeObject.name.LastIndexOf("CVertex") >= 0)
                {
                    if (vertexEditorMode && trangleEditorMode)
                    {
                    }
                    else if (vertexEditorMode)
                    {
                    }
                    else if (trangleEditorMode)
                    {
                        Selection.activeObject = null;
                    }
                }

                if (Selection.activeObject.name.LastIndexOf("CTrangle") >= 0)
                {
                    if (vertexEditorMode && trangleEditorMode)
                    {
                    }
                    else if (vertexEditorMode)
                    {
                        Selection.activeObject = null;
                    }
                    else if (trangleEditorMode)
                    {
                    }
                }

                if (Selection.activeObject != null && temp.Contains(Selection.activeObject))
                {

                }
                else
                {
                    Selection.activeObject = temp.Count > 0 ? temp[0] : null;
                }
            }
        }

        static void GeneralVertex<T>() where T: TMesh.TVertex
        {
            var tCameraVertexs = GameObject.FindObjectsOfType<T>();

            //Selection.activeObject = SceneView.currentDrawingSceneView;
            var sceneView = SceneView.currentDrawingSceneView;
            if (sceneView == null)
            {
                sceneView = SceneView.lastActiveSceneView;
            }
            Camera sceneCam = sceneView.camera;
            Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 30f));


            var name = "CVertex";
            if (tCameraVertexs.Length > 0)
            {
                name = string.Format("CVertex ({0})", tCameraVertexs.Length);
            }

            var gobj = new GameObject(name, new System.Type[] { typeof(T) });

            Undo.RegisterCreatedObjectUndo(gobj, "Create TCameraVertex");

            gobj.transform.position = spawnPos;

            util.SetIcon(gobj, util.Icon.DiamondYellow);

            Selection.activeObject = gobj;

            Mesh tCamearMesh = null;
            if (util.TryGetCameraMesh(out tCamearMesh))
            {
                gobj.transform.SetParent(tCamearMesh.transform, false);

                /*
                 * 将刚生成出来的顶点移动到最后的三角形的顶点所在平面上去
                 * 如果没有就移动了
                 */
                if (tCamearMesh.TCameraTrangles.Count > 0)
                {
                    var trangle = tCamearMesh.TCameraTrangles[tCamearMesh.TCameraTrangles.Count - 1];
                    Vector3 a = trangle[2].transform.position - trangle[0].transform.position;
                    Vector3 b = trangle[1].transform.position - trangle[0].transform.position;
                    Vector3 c = sceneCam.transform.position - trangle[0].transform.position;
                    Vector3 p = spawnPos - trangle[0].transform.position;

                    var normal = Vector3.Cross(a, b);
                    var cos = Vector3.Dot(c.normalized, normal.normalized);
                    var perpendicular = normal.normalized * c.magnitude * cos;

                    var parallel = c - perpendicular;
                    //var project = parallel + trangle[0].transform.position;

                    parallel = parallel + trangle[0].transform.position;
                    //var project = Vector3.Cross(normal,Vector3.Cross(c, normal)) + trangle[0].transform.position;

                    var pc = parallel - sceneCam.transform.position;
                    var pp = spawnPos - sceneCam.transform.position;
                    cos = Vector3.Dot(pc.normalized, pp.normalized);
                    var pp2Len = pc.magnitude / cos;

                    gobj.transform.position = pp.normalized * pp2Len + sceneCam.transform.position;
                }

            }
        }

        protected Transform cameraTrans;
        protected Transform Root;
        protected Transform AxiY;
        protected Transform AxiX;
        protected Vector2 sign = Vector2.one;
        protected Camera storyCamera = null;
        protected Transform storyAvatar = null;

        public VertexProcesser pTCameraVertex;
        public TrangleProcesser pTCameraTrangle;


        /// <summary>
        /// OnGUI之前的准备
        /// </summary>
        protected void BeforeOnGUI()
        {
            if (pTCameraVertex == null)
            {
                pTCameraVertex = VertexProcesser.Get();
            }
            if (pTCameraTrangle == null)
            {
                pTCameraTrangle = TrangleProcesser.Get();
            }
        }

        protected void DrawMeshConstructTool()
        {
            string buttonStr = string.Empty;

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                animBool_vertex.target = EditorGUILayout.BeginToggleGroup("顶点相关", animBool_vertex.target);
                if (EditorGUILayout.BeginFadeGroup(animBool_vertex.faded))
                {
                    buttonStr = vertexEditorMode ? "生成Camera顶点<Ctrl +Q>" : "生成Camera顶点";
                    if (GUILayout.Button(new GUIContent(buttonStr, "<顶点>编辑模式下,激活快捷键<Ctrl + Q>")))
                    {
                        GeneralVertex<TVertex>();
                    }

                    if (GUILayout.Button("选中所有顶点"))
                    {
                        var tCameraVertexs = GameObject.FindObjectsOfType<TVertex>();

                        var gobjs = new List<Object>();
                        for (int i = 0; i < tCameraVertexs.Length; i++)
                        {
                            gobjs.Add(tCameraVertexs[i].gameObject);
                        }

                        Selection.objects = gobjs.ToArray();
                    }

                }

                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                animBool_trangle.target = EditorGUILayout.BeginToggleGroup("三角形相关", animBool_trangle.target);

                if (EditorGUILayout.BeginFadeGroup(animBool_trangle.faded))
                {


                    if (GUILayout.Button("将所有三角形Gobj移动至重心"))
                    {
                        MoveAllTrangleGobjToCentroid();
                    }

                    if (GUILayout.Button("将三角形加入网格"))
                    {
                        if (Selection.gameObjects.Length <= 0)
                        {
                            EditorUtility.DisplayDialog("提醒", "请选择至少1个三角形", "知道了");
                            return;
                        }
                        var tCameraTrangles = new List<TTrangle>();
                        for (int i = 0; i < Selection.gameObjects.Length; i++)
                        {
                            var gobj = Selection.gameObjects[i];
                            var vertex = gobj.GetComponent<TTrangle>();
                            if (vertex == null)
                                continue;

                            tCameraTrangles.Add(vertex);
                        }

                        Mesh tCamearMesh = null;

                        if (!util.TryGetCameraMesh(out tCamearMesh))
                        {
                            return;
                        }

                        UnityEditor.Undo.RecordObject(tCamearMesh, "Add Trangle");

                        for (int i = 0; i < tCameraTrangles.Count; i++)
                        {
                            var trangle = tCameraTrangles[i];
                            if (!tCamearMesh.AddTrangle(trangle))
                            {
                                continue;
                            }
                        }
                        EditorUtility.SetDirty(tCamearMesh);
                    }

                    if (GUILayout.Button("选中所有三角形"))
                    {
                        var objs = GameObject.FindObjectsOfType<Trangle>();

                        var gobjs = new List<Object>();
                        for (int i = 0; i < objs.Length; i++)
                        {
                            gobjs.Add(objs[i].gameObject);
                        }

                        Selection.objects = gobjs.ToArray();
                    }
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                animBool_editor.target = EditorGUILayout.BeginToggleGroup("编辑模式", animBool_editor.target);

                if (EditorGUILayout.BeginFadeGroup(animBool_editor.faded))
                {
                    GUI.color = vertexEditorMode ? new Color(0f, 0.95f, 0.95f, 1f) : Color.white;
                    buttonStr = vertexEditorMode ? "关闭<顶点>编辑模式" : "开启<顶点>编辑模式";
                    if (GUILayout.Button(new GUIContent(buttonStr, "<顶点>编辑模式下,框选只会选择到<顶点>,而且激活生成顶点快捷键<Ctrl + Q> 和顶 点合并快捷键<Ctrl + X>")))
                    {
                        vertexEditorMode = !vertexEditorMode;
                    }
                    GUI.color = Color.white;

                    GUI.color = trangleEditorMode ? new Color(0f, 0.95f, 0.95f, 1f) : Color.white;
                    buttonStr = trangleEditorMode ? "关闭<三角形>编辑模式" : "开启<三角形>编辑模式";
                    if (GUILayout.Button(new GUIContent(buttonStr, "开启后只会框选到<三角形>")))
                    {
                        trangleEditorMode = !trangleEditorMode;
                    }
                    GUI.color = Color.white;
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
            }

            buttonStr = vertexEditorMode ? "合并顶点成三角形<Ctrl +X>" : "合并顶点成三角形";
            if (GUILayout.Button(new GUIContent(buttonStr, "<顶点>编辑模式下,激活快捷键<Ctrl +X>")))
            {
                CombineVerticesToTrangle();
            }

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        protected virtual void OnDrawTool()
        { 

        }

        protected void DrawOtherTool()
        {
            //其他
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                animBool_other.target = EditorGUILayout.BeginToggleGroup("其他", animBool_other.target);

                if (EditorGUILayout.BeginFadeGroup(animBool_other.faded))
                {
                    if (GUILayout.Button("帮助文档"))
                    {
                        OpenDocument();
                    }

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        if (GUILayout.Button("创建镜头检测器<Checker>"))
                        {
                            var objs = GameObject.FindObjectsOfType<SimpleController>();
                            int displaySet = EditorPrefs.GetInt(checkerDisplayKey, 0);
                            if (objs.Length > 0)
                            {
                                if (objs.Length > 0)
                                {
                                    util.SetIcon(objs[0].gameObject, util.Icon.CirclePurple);
                                }

                                if (displaySet != 2)
                                {
                                    var res = EditorUtility.DisplayDialogComplex("提醒", "场景中已经存在镜头检测器了，帮你标成圆形紫色，并选中它了", "选中", "关闭", "不再提示");
                                    Debug.Log(res);
                                    EditorPrefs.SetInt(checkerDisplayKey, res);
                                }
                                Selection.activeGameObject = objs[0].gameObject;
                                return;
                            }

                            var checker = new GameObject("Checker", new Type[] { typeof(SimpleController) });
                            Selection.activeGameObject = checker;
                            Undo.RegisterCreatedObjectUndo(checker, "General Checker");
                            util.SetIcon(checker, util.Icon.CirclePurple);


                            Mesh mesh;
                            if (util.TryGetCameraMesh(out mesh))
                            {
                                mesh.SetTarget(checker.transform);
                                checker.transform.SetParent(mesh.transform, false);
                                checker.transform.position = Vector3.zero;
                                //如果有顶点，放到第一个顶点那里去
                                var vertices = mesh.GetAllVertices();
                                if (vertices.Count > 0 && vertices[0] != null)
                                {
                                    checker.transform.position = vertices[0].transform.position;
                                }
                            }
                        }

                        FixedCheckerPositionToMeshSurface = EditorPrefs.GetBool("FixedCheckerPositionToMeshSurface", true);
                        FixedCheckerPositionToMeshSurface = EditorGUILayout.ToggleLeft("贴合网格表面", FixedCheckerPositionToMeshSurface, GUILayout.MaxWidth(86));
                        if (SimpleController.current != null)
                        {
                            SimpleController.current.FixedPositionToMeshsSurface = FixedCheckerPositionToMeshSurface;
                            EditorPrefs.SetBool("FixedCheckerPositionToMeshSurface", FixedCheckerPositionToMeshSurface);
                        }
                    }
                    EditorGUILayout.EndHorizontal();


                    if (GUILayout.Button("选择网格对象"))
                    {
                        var objs = GameObject.FindObjectsOfType<Mesh>();

                        var gobjs = new List<Object>();
                        for (int i = 0; i < objs.Length; i++)
                        {
                            gobjs.Add(objs[i].gameObject);
                        }

                        Selection.objects = gobjs.ToArray();
                    }

                    if (GUILayout.Button("显示/隐藏所有标记"))
                    {
                        MeshObjectIconVisualize(!maskSwitch);
                    }


                    if (GUILayout.Button("显示/隐藏网格"))
                    {
                        Mesh tCamearMesh = null;

                        if (util.TryGetCameraMesh(out tCamearMesh))
                        {
                            MeshVisualize(!tCamearMesh.GizmosOn);
                        }
                    }

                    //if (GUILayout.Button("测试"))
                    //{
                    //    Debug.Log(GetScriptStorePath());
                    //}
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
            }
        }


        //DrawMeshConstructTool();
        //👇👇👇
        //OnDrawTool();
        //👇👇👇
        //DrawOtherTool();
        protected void OnGUI()
        {
            scroll_value = EditorGUILayout.BeginScrollView(scroll_value);

            BeforeOnGUI();

            DrawMeshConstructTool();

            OnDrawTool();

            DrawOtherTool();

            EditorGUILayout.EndScrollView();
        }

        protected void MeshObjectIconVisualize(bool enable)
        {
            if (enable)
            {
                MaskAllVertex();
                MaskAllTrangle();
                MoveAllTrangleGobjToCentroid();
                maskSwitch = true;
                return;
            }

            maskSwitch = false;
            var tCameraVertexs = GameObject.FindObjectsOfType<TVertex>();

            var gobjs = new List<Object>();
            for (int i = 0; i < tCameraVertexs.Length; i++)
            {
                var gobj = tCameraVertexs[i].gameObject;

                Undo.RecordObject(gobj, "Clear All TCameraVertex Mask");

                util.CleanIcon(gobj);

                EditorUtility.SetDirty(gobj);
            }

            var objs = GameObject.FindObjectsOfType<Trangle>();

            gobjs = new List<Object>();
            for (int i = 0; i < objs.Length; i++)
            {
                var gobj = objs[i].gameObject;

                Undo.RecordObject(gobj, "Clear All TCameraTrangle Mask");

                util.CleanIcon(gobj);

                EditorUtility.SetDirty(gobj);
            }
        }

        protected void MeshVisualize(bool enable)
        {
            Mesh tCamearMesh = null;

            if (util.TryGetCameraMesh(out tCamearMesh))
            {
                tCamearMesh.GizmosOn = enable;
                SceneView.RepaintAll();
            }
        }

        protected void DrawList<T>(string title, Processer<T> processer) where T : Component
        {
            DrawList<T>(new GUIContent(title), processer);
        }

        protected void DrawList<T>(GUIContent uIContent, Processer<T> processer) where T : Component
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(uIContent.text, GUILayout.Width(45));
                List<T> tempObj = DragAreaGetObject.GetOjbect<T>("(拖到这里来，可以多选)");
                foreach (var comp in tempObj)
                {
                    if (!processer.targets.Contains(comp))
                    {
                        Undo.RecordObject(processer, "TCameraEditorWindow add " + typeof(T).Name);
                        processer.targets.Add(comp);
                        EditorUtility.SetDirty(processer);
                    }
                }

                if (GUILayout.Button("清理", GUILayout.Width(40)))
                {
                    Undo.RecordObject(processer, "TCameraEditorWindow clear " + typeof(T).Name);
                    processer.targets.Clear();
                    EditorUtility.SetDirty(processer);
                }
                EditorGUILayout.EndHorizontal();

                if (processer.targets.Count > 0)
                {
                    for (int i = 0; i < processer.targets.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(49);
                        processer.targets[i] = EditorGUILayout.ObjectField(processer.targets[i], typeof(T), true) as T;
                        if (GUILayout.Button("删除", GUILayout.Width(40)))
                        {
                            Undo.RecordObject(processer, "TCameraEditorWindow delete " + typeof(T).Name);
                            processer.targets.RemoveAt(i);
                            i--;
                            EditorUtility.SetDirty(processer);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        protected static void OpenDocument()
        {
            WWW www = new WWW("https://docs.qq.com/doc/DY0JqTVFyWGRFSGdi");
            Application.OpenURL(www.url);
        }
        protected static void CombineVerticesToTrangle()
        {
            if (Selection.gameObjects.Length <= 0)
            {
                EditorUtility.DisplayDialog("提醒", "请选择至少3个顶点", "知道了");
                return;
            }

            var tCameraVertexList = new List<TVertex>();
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                var gobj = Selection.gameObjects[i];
                var vertex = gobj.GetComponent<TVertex>();
                if (vertex == null)
                    continue;

                if (tCameraVertexList.Count == 3)
                    break;

                tCameraVertexList.Add(vertex);
            }

            if (tCameraVertexList.Count < 3)
            {
                EditorUtility.DisplayDialog("提醒", "请选择至少3个顶点", "知道了");
                return;
            }

            if (util.IsTrangleExists<TVertex,Mesh>(tCameraVertexList.ToArray()))
            {
                EditorUtility.DisplayDialog("提醒", "已存在拥有相同顶点的三角形", "知道了");
                return;
            }


            Trangle trangle;
            if (util.TryNewTrangleFormVertices<Trangle, Mesh>(tCameraVertexList.ToArray(), out trangle))
            {
                util.SetIcon(trangle.gameObject, util.Icon.DiamondTeal);
            }
        }

        protected static void MoveAllTrangleGobjToCentroid()
        {
            var objs = GameObject.FindObjectsOfType<Trangle>();

            //for (int i = 0; i < objs.Length; i++)
            //{
            //    var gobj = objs[i].gameObject;
            //    util.SetIcon(gobj, util.Icon.DiamondTeal);
            //}

            objs = GameObject.FindObjectsOfType<Trangle>();
            for (int i = 0; i < objs.Length; i++)
            {
                objs[i].MoveToCentroid();
            }
        }

        protected static void MaskAllTrangle()
        {
            var objs = GameObject.FindObjectsOfType<Trangle>();

            var gobjs = new List<Object>();
            for (int i = 0; i < objs.Length; i++)
            {
                var gobj = objs[i].gameObject;
                util.SetIcon(gobj, util.Icon.DiamondTeal);
            }
        }
        protected static bool maskSwitch = false;
        protected static void MaskAllVertex()
        {
            var tCameraVertexs = GameObject.FindObjectsOfType<TVertex>();

            var gobjs = new List<Object>();
            for (int i = 0; i < tCameraVertexs.Length; i++)
            {
                var gobj = tCameraVertexs[i].gameObject;
                Undo.RecordObject(gobj, "Mask All TCameraVertex");
                util.SetIcon(gobj, util.Icon.DiamondYellow);
                EditorUtility.SetDirty(gobj);
            }
        }

    }

    public class AnimBoolHandle : AnimBool
    {
        protected string m_RecordKey = string.Empty;
        public string RecordKey
        {
            get { return m_RecordKey; }
            protected set { m_RecordKey = value; }
        }
        public AnimBoolHandle(string recordKey, bool value) : base(value)
        {
            value = EditorPrefs.GetBool(recordKey, value);
            target = value;
            m_RecordKey = recordKey;
        }
        public new bool target
        {
            get { return base.target; }
            set
            {
                if (value != target)
                {
                    EditorPrefs.SetBool(RecordKey, value);
                }
                base.target = value;
            }
        }
    }

    //[CreateAssetMenuAttribute(menuName = "VertexProcesser", fileName= "VertexProcesser")]
    public class VertexProcesser : Processer<TCameraVertex>
    {
        public static VertexProcesser Get()
        {
            return Get<VertexProcesser>();
        }
    }
    //[CreateAssetMenuAttribute(menuName = "TrangleProcesser", fileName = "TrangleProcesser")]
    public class TrangleProcesser : Processer<TTrangle>
    {
        public static TrangleProcesser Get()
        {
            return Get<TrangleProcesser>();
        }
    }
    public abstract class Processer<T> : ScriptableObject where T : Component
    {
        public T template = null;
        public List<T> targets = new List<T>();
        protected static string scriptableObjectPath = string.Empty;

        public static T2 Get<T2>() where T2 : ScriptableObject
        {

            string path = util.FindTCameraRoot();
            if (!AssetDatabase.IsValidFolder(util.GetScriptStorePath()))
            {
                path = AssetDatabase.CreateFolder(path, "ScriptableObject");
            }
            path = System.IO.Path.Combine(util.GetScriptStorePath(), "{0}.asset");
            path = string.Format(path, typeof(T2).Name);

            T2 objScript = AssetDatabase.LoadAssetAtPath(path, typeof(T2)) as T2;

            if (objScript == null)
            {
                objScript = ScriptableObject.CreateInstance<T2>();
                AssetDatabase.CreateAsset(objScript, path);
                AssetDatabase.SaveAssets();
            }
            scriptableObjectPath = path;
            return objScript;
        }

        public static bool Dispose()
        {
            if (string.IsNullOrEmpty(scriptableObjectPath))
            {
                return false;
            }

            return AssetDatabase.DeleteAsset(scriptableObjectPath);
        }
    }


    public class DragAreaGetObject : Editor
    {

        public static List<T> GetOjbect<T>(string meg = null) where T : Component
        {
            Event aEvent;
            aEvent = Event.current;
            List<T> temps = new List<T>();

            GUI.contentColor = Color.white;
            UnityEngine.Object temp = null;

            var dragArea = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            GUIContent title = new GUIContent(meg);
            if (string.IsNullOrEmpty(meg))
            {
                title = new GUIContent("拖到这里来");
            }

            GUI.Box(dragArea, title, EditorStyles.objectField);
            switch (aEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dragArea.Contains(aEvent.mousePosition))
                    {
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (aEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();



                        for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                        {
                            temp = DragAndDrop.objectReferences[i];

                            if (temp is GameObject)
                            {
                                var comp = (temp as GameObject).GetComponent<T>();
                                if (comp != null)
                                {
                                    temps.Add(comp);
                                }
                            }
                        }
                    }

                    Event.current.Use();
                    break;
                default:
                    break;
            }

            return temps;
        }
    }
}

