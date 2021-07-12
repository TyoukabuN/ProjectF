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
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    [DisallowMultipleComponent]
    [HelpURL("https://docs.qq.com/doc/DY0JqTVFyWGRFSGdi")]
    public class TCameraMesh : TMeshBase
    {
        [HideInInspector]
        public static TCameraMesh currentTCameraMesh;
      
        /// <summary>
        /// will pass some args of tCameraVertex
        /// </summary>
        public CameraMeshEvent OnPositionChanged;
        public CameraMeshEventWithSplitArgs OnPositionChangedWithSplitArgs;
        /// <summary>
        /// will pass some args of tCameraVertex
        /// </summary>
        public CameraMeshComplexEvent OnComplexEvent;

        public void Awake()
        {
            if (currentTCameraMesh == null)
            { 
                currentTCameraMesh = this;
            }
            if (OnPositionChanged == null)
            {
                OnPositionChanged = new CameraMeshEvent();
            }
            if (OnPositionChangedWithSplitArgs == null)
            {
                OnPositionChangedWithSplitArgs = new CameraMeshEventWithSplitArgs();
            }
            if (OnComplexEvent == null)
            {
                OnComplexEvent = new CameraMeshComplexEvent();
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
                    var tVertices = tri.vertices;
                    var eulerAngles = (tVertices[0] as TCameraVertex).EularAngle * weight[0] +
                        (tVertices[1] as TCameraVertex).EularAngle * weight[1] +
                        (tVertices[2] as TCameraVertex).EularAngle * weight[2];

                    var pivotPosition = (tVertices[0] as TCameraVertex).PivotPosition * weight[0] +
                        (tVertices[1] as TCameraVertex).PivotPosition * weight[1] +
                        (tVertices[2] as TCameraVertex).PivotPosition * weight[2];

                    //Add Other args

                    if (OnPositionChanged != null)
                    {
                        OnPositionChanged.Invoke(eulerAngles, pivotPosition);
                    }
                    if (OnPositionChangedWithSplitArgs != null)
                    {
                        OnPositionChangedWithSplitArgs.Invoke(eulerAngles.x, eulerAngles.y, eulerAngles.z);
                    }
                    if (OnComplexEvent != null)
                    {
                        OnComplexEvent.Invoke(eulerAngles, pivotPosition, weight);
                    }
                }
                return true;
            }
            return false;
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
                    }
                }

                Gizmos.DrawMesh(mesh);

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
        }
#endif


        public class CameraMeshEvent : UnityEvent<Vector3,Vector3> { }
        public class CameraMeshEventWithSplitArgs : UnityEvent<float, float, float> { }
        public class CameraMeshComplexEvent : UnityEvent<Vector3, Vector3, float[]> { }
    }
}
