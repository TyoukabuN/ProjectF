using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMesh;
using System;


namespace TMesh
{
    [ExecuteAlways]
    public class SimpleController : MonoBehaviour
    {
        public static SimpleController current;

        public Camera Camera;
        public Transform AxiY;
        public Transform AxiX;

        protected string HorizontalAxi = "Horizontal";
        protected string VerticalAxi = "Vertical";
        public float Speed = 0.4f;
#if TYOU_LAB
        [SerializeField]
#endif
        protected bool m_FixedPositionToMeshsSurface = true;
        public bool FixedPositionToMeshsSurface
        {
            get
            {
                return m_FixedPositionToMeshsSurface;
            }
            set
            {
                m_FixedPositionToMeshsSurface = value;
            }
        }
        void OnEnable()
        {
            current = this;

            TCameraMesh mesh;
            if (TMesh.TCameraUtility.TryGetCameraMesh(out mesh))
            {
                mesh.SetTarget(transform);
                //mesh.OnPositionChanged.RemoveListener(OnPositionChanged);
                //mesh.OnPositionChanged.AddListener(OnPositionChanged);
                if (mesh.OnComplexEvent == null)
                {
                    mesh.OnComplexEvent = new TCameraMesh.CameraMeshComplexEvent();
                }
                mesh.OnComplexEvent.RemoveListener(OnComplexEvent);
                mesh.OnComplexEvent.AddListener(OnComplexEvent);
            }

            TEventMesh eventMesh;
            if (TMesh.TCameraUtility.TryGetCameraMesh(out eventMesh))
            {
                eventMesh.SetTarget(transform);
            }

            if (AxiY == null)
            {
                var obj = new GameObject("AxiY");
                obj.transform.SetParent(transform);
                AxiY = obj.transform;
            }
            AxiY.transform.localPosition = Vector3.zero;

            if (AxiX == null)
            {
                var obj = new GameObject("AxiX");
                obj.transform.SetParent(AxiY);
                AxiX = obj.transform;
            }
            AxiX.transform.localPosition = Vector3.zero;

            if (Camera == null)
            {
                Camera = Camera.main;
                if (Camera == null)
                {
                    var gobj = new GameObject("tempCamera");
                    var camera = gobj.AddComponent<Camera>();
                    Camera = camera;
                }
            }

            Camera.transform.SetParent(AxiX);
            Camera.transform.localPosition = Vector3.zero;
            Camera.transform.localRotation = Quaternion.identity;
        }

        void OnPositionChanged(Vector3 cameraEular, Vector3 pivotPosition)
        {
            AxiY.transform.localEulerAngles = new Vector3(0, cameraEular.y, 0);
            AxiX.transform.localEulerAngles = new Vector3(cameraEular.x, 0, 0);
            AxiX.localPosition = pivotPosition;

            Camera.transform.localEulerAngles = Vector3.zero;
            Camera.transform.localPosition = new Vector3(0, 0, -cameraEular.z);
        }

        void OnComplexEvent(Vector3 cameraEular, Vector3 pivotPosition, float[] weight)
        {
            OnPositionChanged(cameraEular, pivotPosition);

            if (FixedPositionToMeshsSurface)
            {
                TCameraMesh mesh;
                if (TMesh.TCameraUtility.TryGetCameraMesh(out mesh))
                {
                    try
                    {
                        if (mesh.CurrentTrangle == null)
                            return;

                        var tri = mesh.CurrentTrangle;
                        transform.position = tri.vertices[0].transform.position * weight[0] +
                            tri.vertices[1].transform.position * weight[1] +
                            tri.vertices[2].transform.position * weight[2];
                    }
                    catch (Exception e)
                    {
                        Debug.Log(123);
                    }
                    
                }
            }
        }
#if UNITY_EDITOR
        void Update()
        {
            var sceneView = UnityEditor.SceneView.currentDrawingSceneView;
            if (sceneView == null)
            {
                sceneView = UnityEditor.SceneView.lastActiveSceneView;
            }
            Camera sceneCam = sceneView.camera;

            var h = Input.GetAxis(HorizontalAxi);
            var v = Input.GetAxis(VerticalAxi);

            var forward = new Vector3(sceneCam.transform.forward.x,0, sceneCam.transform.forward.z);
            forward.Normalize();

            var right = new Vector3(sceneCam.transform.right.x, 0, sceneCam.transform.right.z);
            right.Normalize();
            

            transform.Translate(forward * v * Speed + right * h * Speed);
        }
#endif
    }

}