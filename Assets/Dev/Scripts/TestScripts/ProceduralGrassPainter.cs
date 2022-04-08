using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class ProceduralGrassPainter : MonoBehaviour
{
    public Mesh MetaGrassMesh;
    public int AmountPreClick = 10;
    public float Radius = 0.5f;
    void Awake()
    {

    }
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current == null)
            return;

        if (sceneView == null)
            sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null)
            return;

        //Debug.Log(Event.current.mousePosition);
        if (Event.current.type == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                //Gizmos.DrawSphere(hitInfo.point, 1.0f);
                //Debug.Log(hitInfo.point);
                for (int i = 0; i < AmountPreClick; i++)
                {
                    var random = Random.insideUnitCircle;
                    GenGrass(hitInfo.point + new Vector3(random.x,0, random.y) * Radius);
                }
            }
        }

        if (Event.current.isMouse)
        {
            //Debug.Log(Event.current.type);
            //Debug.Log(Event.current.rawType);
        }
    }
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    List<Vector3> vertexs = new List<Vector3>();
    List<int> triangles = new List<int>();

    public void GenGrass(Vector3 wpos)
    {
        if (vertexs == null)
            return;

        var originVertexCounts = vertexs.Count;
        var originTriangleCounts = triangles.Count;
        for (int i = 0; i < MetaGrassMesh.vertices.Length; i++)
        {
            var vertex = MetaGrassMesh.vertices[i];
            vertexs.Add(wpos + vertex);
        }
        for (int i = 0; i < MetaGrassMesh.triangles.Length; i++)
        {
            int indice = MetaGrassMesh.triangles[i] + originVertexCounts;
            triangles.Add(indice);
        }
        

        if (!mesh)
        {
            mesh = new Mesh();
            mesh.name = "艹";
        }
        mesh.vertices = vertexs.ToArray();
        mesh.triangles = triangles.ToArray();

        meshFilter.mesh = mesh;
    }

    public void Clear()
    {
        vertexs = new List<Vector3>();
        triangles = new List<int>();
        mesh = null;
    }
    void Update()
    {
        if(Event.current != null)
            Debug.Log(Event.current.mousePosition);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var sceneView = SceneView.currentDrawingSceneView;
            if (sceneView == null)
            {
                sceneView = SceneView.lastActiveSceneView;
            }
            Camera sceneCam = sceneView.camera;
            Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 30f));
        }
    }

}

[CustomEditor(typeof(ProceduralGrassPainter))]
public class ProceduralGrassPainterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var instance = target as ProceduralGrassPainter;
        if (!target) return;

        if (GUILayout.Button("Clear"))
        {
            instance.Clear();
        }
    }
}
