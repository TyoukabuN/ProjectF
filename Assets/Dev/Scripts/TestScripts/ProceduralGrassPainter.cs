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

    List<Vector3> grassPosition = new List<Vector3>();
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
                var genCenter = hitInfo.point;
                for (int i = 0; i < AmountPreClick; i++)
                {
                    var random = Random.insideUnitCircle;
                    var wpos = genCenter + new Vector3(random.x, 0, random.y) * Radius;
                    //
                    var distance = 200.0f;
                    RaycastHit hitInfo2;
                    if (Physics.Raycast(wpos + Vector3.up * distance, Vector3.down, out hitInfo2, distance + 1.0f))
                    {
                        wpos = hitInfo2.point;
                        var normal = hitInfo2.normal;
                        var angle = Random.Range(0.0f, 360.0f) ;
                        var rotation = Quaternion.AngleAxis(30f , Vector3.up);
                        //var rotation = new Quaternion(0, Mathf.Sin(30f * 0.5f * Mathf.Deg2Rad), 0, Mathf.Cos(30f * 0.5f * Mathf.Deg2Rad));
                        GenGrass(wpos, rotation);
                    }
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
    List<Vector3> normals = new List<Vector3>();
    public void GenGrass(Vector3 wpos)
    {
        GenGrass(wpos, Quaternion.identity);
    }
    public void GenGrass(Vector3 wpos,Quaternion rotation)
    {
        if (vertexs == null)
            return;

        var originVertexCounts = vertexs.Count;
        var originNormalCounts = normals.Count;
        var originTriangleCounts = triangles.Count;
        for (int i = 0; i < MetaGrassMesh.vertices.Length; i++)
        {
            var vertex = MetaGrassMesh.vertices[i];
            vertex = rotation * vertex;
            vertex = vertex + wpos;
            vertexs.Add(vertex);
        }
        for (int i = 0; i < MetaGrassMesh.triangles.Length; i++)
        {
            int indice = MetaGrassMesh.triangles[i] + originVertexCounts;
            triangles.Add(indice);
        }        
        for (int i = 0; i < MetaGrassMesh.normals.Length; i++)
        {
            normals.Add(MetaGrassMesh.normals[i]);
        }
        

        if (!mesh)
        {
            mesh = new Mesh();
            mesh.name = "艹";
        }
        mesh.vertices = vertexs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        meshFilter.mesh = mesh;
    }

    public void Clear()
    {
        vertexs = new List<Vector3>();
        triangles = new List<int>();
        mesh = null;
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }
    public void ReStart()
    {
        Clear();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
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

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        {
            if (GUILayout.Button("ReStart", GUILayout.Width(80)))
            {
                instance.ReStart();
            }
            if (GUILayout.Button("Clear",GUILayout.Width(80)))
            {
                instance.Clear();
            }
            EditorGUILayout.EndHorizontal();
        }

    }
}
