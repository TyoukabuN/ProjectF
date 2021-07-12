/*
 * By TyoukabuN
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TMesh
{
    [DisallowMultipleComponent]
    [HelpURL("https://docs.qq.com/doc/DY0JqTVFyWGRFSGdi")]
    public class TEventMesh : TMeshBase
    {
        [HideInInspector]
        public static TEventMesh current;

        public float ConeGizmoSize = 0.1f;
        public float PosOffset = 0.1f;

        public float YOffset = 0.1f;

        public void Awake()
        {
            if (current == null)
            {
                current = this;
            }
        }

        protected override bool TrangleCheckAndProcess(TTrangle tri)
        {
            var vertices = tri.Vertices.ToArray();

            float[] weight;
            if (TCameraUtility.IsInsideTrangle(vertices, Target.position, out weight))
            {
                if (PowerOn)
                {
                    if (CurrentTrangle != tri)
                    {
                        var etri = tri as TEventTrangle;
                        if (!etri.AnyRelationship(CurrentTrangle as TEventTrangle))
                        {
                            tri.OnEnterTrangle();
                        }
                    }

                }
                return true;
            }

            return false;
        }



        protected override void Update()
        {
            Profiler.BeginSample("TCamreraMesh");

            if (NoneTriangle())
            {
                Profiler.EndSample();
                return;
            }

            CleanUp();

            if (!Target)
            {
                Profiler.EndSample();
                return;
            }

            if (PerformanceOptimizationOn)
            {
                PerformanceOptimizationSetup();
            }

            if (CurrentTrangle)
            {
                if (IsTrangleValid(CurrentTrangle) && TrangleCheckAndProcess(CurrentTrangle))
                {
                    Profiler.EndSample();
                    return;
                }
            }


            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i];

                if (IsTrangleValid(tri) && TrangleCheckAndProcess(tri))
                {
                    var etri = tri as TEventTrangle;
                    if (!etri.AnyRelationship(CurrentTrangle as TEventTrangle))
                    {
                        if (CurrentTrangle)
                        {
                            CurrentTrangle.OnExitTrangle();
                        }
                    }
                    CurrentTrangle = tri;

                    Profiler.EndSample();
                    return;
                }
            }

            if (CurrentTrangle)
            {
                CurrentTrangle.OnExitTrangle();
                CurrentTrangle = null;
            }

            Profiler.EndSample();
        }

#if UNITY_EDITOR

        Dictionary<TTrangle, Mesh> tempMesh = new Dictionary<TTrangle, Mesh>();

        void OnDrawGizmos()
        {
            if (!GizmosOn)
                return;

            if (TCameraTrangles.Count < 1)
                return;

            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i];

                if (tri == null)
                    break;

                if (tri.Vertices.Count < 3)
                    break;

                if (tempMesh == null)
                {
                    tempMesh = new Dictionary<TTrangle, Mesh>();
                }

                Mesh mesh = null;
                if (!tempMesh.ContainsKey(tri))
                {
                    tempMesh[tri] = new Mesh();
                }
                mesh = tempMesh[tri];
                mesh.vertices = tri.Vertices.ToArray();
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                Gizmos.color = Color.red;
                if (Target != null)
                {
                    if (TCameraUtility.IsInsideTrangleS2(mesh.vertices, Target.position))
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawMesh(mesh);
                    }
                }


                Gizmos.color = Color.white;

                if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 ||
                    SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
                {
                    Gizmos.DrawWireMesh(mesh);
                }
                else
                {
                    Gizmos.DrawLine(mesh.vertices[0], mesh.vertices[1]);
                    Gizmos.DrawLine(mesh.vertices[0], mesh.vertices[2]);
                    Gizmos.DrawLine(mesh.vertices[1], mesh.vertices[2]);
                }

            }

            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i] as TEventTrangle;

                if (!tri.transform.parent)
                    continue;

                if (!tri.transform.parent.GetComponent<TEventTrangle>())
                    continue;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(tri.transform.position, tri.transform.parent.position);
                Gizmos.color = Color.white;

                //UnityEditor.SceneView.currentDrawingSceneView.camera.TODO: relationShip with camera zoom
                Handles.color = Color.yellow;
                var dir2This = tri.transform.position - tri.transform.parent.position;
                var rot = Quaternion.LookRotation(dir2This.normalized);
                Handles.ConeHandleCap(10, tri.transform.position - dir2This * PosOffset, rot, ConeGizmoSize, EventType.Repaint);
                Gizmos.color = Color.white;


            }

            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i] as TEventTrangle;

                if (tri.transform.childCount <= 0)
                    continue;

                Handles.color = new Color(1, 0.5423231f, 0);
                var dir2This = tri.transform.position - tri.transform.parent.position;
                var rot = Quaternion.Euler(90, 0, 0);
                Handles.ConeHandleCap(11, tri.transform.position + Vector3.up * YOffset, rot, ConeGizmoSize * 1.2f, EventType.Repaint);
                Gizmos.color = Color.white;

            }

        }
#endif

    }

}
