using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TMesh
{ 
    public class TCameraEditorUtility
    {
        public static bool TryGetCameraMesh<Mesh>(out Mesh mesh) where Mesh:TMesh.TMeshBase
        {
            mesh = GameObject.FindObjectOfType<Mesh>();
            var type = typeof(Mesh);

            if (mesh == null)
            {
                string name = string.Empty;
                if (type == typeof(TCameraMesh))
                {
                    name = "TCameraMesh";
                }
                else if (type == typeof(TEventMesh))
                {
                    name = "TEventMesh";
                }

                GameObject gobj = new GameObject(name);
                gobj.transform.position = Vector3.zero;
                mesh = gobj.AddComponent<Mesh>();
                SetIcon(gobj, Icon.DiamondPurple);
            }

            if (mesh != null)
            {
                if (type == typeof(TCameraMesh))
                {
                    TCameraMesh.currentTCameraMesh = mesh as TCameraMesh;
                }
                else if(type == typeof(TEventMesh))
                {
                    TEventMesh.current = mesh as TEventMesh;
                }
            }

            return mesh != null;
        }


        protected static TVertex[] book;
        protected static Dictionary<TVertex, bool> book2;
        protected static bool TryGetClockwiseOrderLoop(TVertex[] vertices,int step = 0)
        {
            if (step == 3)
            {
                return TCameraUtility.IsPositiveOrder(book);
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                if (book2.ContainsKey(vertex))
                    continue;

                book[step] = vertex;
                book2[vertex] = true;

                step++;
                if (TryGetClockwiseOrderLoop(vertices, step))
                {
                    return true;
                }
                book2.Remove(vertex);
                step--;
            }

            return false;
        }

        public static bool TryGetClockwiseOrder(TVertex[] vertices,out TVertex[] res)
        {
            book = new TVertex[3];
            book2 = new Dictionary<TVertex, bool>();
            var res1 = TryGetClockwiseOrderLoop(vertices);
            
            res = book;

            return res1;
        }

        public static bool TryNewTrangleFormVertices<Trangle,Mesh>(TVertex[] vertices, out Trangle trangle) where Trangle : TTrangle where Mesh : TMesh.TMeshBase
        {
            trangle = null;
            Mesh tCamearMesh = null;

            if (!TryGetCameraMesh(out tCamearMesh))
            {
                return false;
            }

            TVertex[] positiveOrder;
            if (!TryGetClockwiseOrder(vertices,out positiveOrder))
            {
                return false;
            }

            var tCameraTrangle = GameObject.FindObjectsOfType<Trangle>();

            var name = "CTrangle";
            if (tCameraTrangle.Length > 0)
            {
                name = string.Format("CTrangle ({0})", tCameraTrangle.Length);
            }


            var gobj = new GameObject(name,typeof(Trangle));
            UnityEditor.Undo.RegisterCreatedObjectUndo(gobj, "New Trangle");

            trangle = gobj.GetComponent(typeof(Trangle)) as Trangle;
            trangle.vertices = positiveOrder;

            gobj.transform.SetParent(tCamearMesh.transform, false);
            trangle.MoveToCentroid();


            UnityEditor.Undo.RecordObject(tCamearMesh, "Add Trangle");

            if (!tCamearMesh.AddTrangle(trangle))
            {
                GameObject.DestroyImmediate(trangle);
                return false;
            }
            EditorUtility.SetDirty(tCamearMesh);
            return true;
        }

        public static bool IsTrangleExists<TVertex,Mesh>(TVertex[] tCameraVertices) where TVertex: TMesh.TVertex where Mesh : TMesh.TMeshBase
        {
            Mesh tCamearMesh = null;

            if (!TryGetCameraMesh(out tCamearMesh))
            {
                return true;
            }

            if (tCamearMesh.TCameraTrangles.Count <= 0)
            {
                return false;
            }

            var dict = new Dictionary<TVertex, bool>();
            for (int i = 0; i < tCameraVertices.Length; i++)
            {
                var vertex = tCameraVertices[i];
                dict[vertex] = true;
            }

            for (int i = 0; i < tCamearMesh.TCameraTrangles.Count; i++)
            {
                var trangle = tCamearMesh.TCameraTrangles[i];

                if (trangle.vertices.All((vertex) => dict.ContainsKey(vertex as TVertex)))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetScriptStorePath()
        {
            var root = FindTCameraRoot();

            return System.IO.Path.Combine(root, "ScriptableObject");
        }
        public static string FindTCameraRoot()
        {
            string res = string.Empty;
            var guids = AssetDatabase.FindAssets(string.Format("{0} t:Script", "TCameraMesh"));
            if (guids.Length <= 0)
            {
                return string.Empty;
            }

            res = AssetDatabase.GUIDToAssetPath(guids[0]);

            res = res.Replace("/Script/TCameraMesh.cs", string.Empty);

            return res;
        }

        public enum LabelIcon
        {
            Gray = 0,
            Blue,
            Teal,
            Green,
            Yellow,
            Orange,
            Red,
            Purple
        }

        public enum Icon
        {
            CircleGray = 0,
            CircleBlue,
            CircleTeal,
            CircleGreen,
            CircleYellow,
            CircleOrange,
            CircleRed,
            CirclePurple,
            DiamondGray,
            DiamondBlue,
            DiamondTeal,
            DiamondGreen,
            DiamondYellow,
            DiamondOrange,
            DiamondRed,
            DiamondPurple
        }

        protected static GUIContent[] labelIcons;
        protected static GUIContent[] largeIcons;

        public static void SetIcon(GameObject gObj, LabelIcon icon)
        {
            if (labelIcons == null)
            {
                labelIcons = GetTextures("sv_label_", string.Empty, 0, 8);
            }

            SetIcon(gObj, labelIcons[(int)icon].image as Texture2D);
        }

        public static void SetIcon(GameObject gObj, Icon icon)
        {
            if (largeIcons == null)
            {
                largeIcons = GetTextures("sv_icon_dot", "_pix16_gizmo", 0, 16);
            }

            SetIcon(gObj, largeIcons[(int)icon].image as Texture2D);
        }

        protected static void SetIcon(GameObject gObj, Texture2D texture)
        {
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, new object[] { gObj, texture });
        }

        public static void CleanIcon(GameObject gObj)
        {
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, new object[] { gObj, null });
        }

        protected static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
        {
            GUIContent[] guiContentArray = new GUIContent[count];

            for (int index = 0; index < count; ++index)
            {
                guiContentArray[index] = EditorGUIUtility.IconContent(baseName + (object)(startIndex + index) + postFix);
            }

            return guiContentArray;
        }

    }
}
