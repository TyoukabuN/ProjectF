using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter)),RequireComponent(typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    [Range(0,20)]
    public int GridSizeX = 4;
    [Range(0, 20)]
    public int GridSizeY = 2;

    public float genaralInteral = 0.05f;
    public bool random = false;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public Image image;
    CanvasRenderer canvasRenderer;
    public Material material;

    public bool DisplayVertex = false;


    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        canvasRenderer = image.GetComponent<CanvasRenderer>();

        if (meshFilter)
            meshFilter.mesh = null;
        //if (meshRenderer)
        //    meshRenderer.materials = new Material[] { };

        if (material != null)
            meshRenderer.material = material;
        else
            meshRenderer.material = ShaderHandle.GetMaterial("Custom_UVCheckerUnLit");

        //ReGenerate();

    }

    void Update()
    {
    }

    public List<Vector3> vertexs = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uv = new List<Vector2>();
    public List<int> rows = new List<int>();
    public List<int> cols = new List<int>();
    public class Vertex
    {
        public int x;
        public int y;
        public Vertex(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public List<Vertex> randomVertexs = new List<Vertex>();
    public List<Vertex> vertexTemp = new List<Vertex>();
    public List<int> indexTemp = new List<int>();

    bool working = false;
    public void ReGenerate()
    {
        if (working) return;
        working = true;
        StartCoroutine(Generate());
    }

    public IEnumerator Generate()
    {
        if (!mesh)
        { 
            mesh = new Mesh();
            mesh.name = "Procedural Mesh";
        }


        //meshFilter.mesh = mesh;

        var gInterval = new WaitForSeconds(genaralInteral);
        vertexs.Clear();
        normals.Clear();
        uv.Clear();

        if (GridSizeX <= 0 || GridSizeY <= 0)
        {
            mesh.name = "Procedural Mesh";
            meshFilter.mesh = mesh;
        }

        for (float y = 0; y < GridSizeY + 1; y++)
        {
            for (float x = 0; x < GridSizeX + 1; x++)
            { 
                vertexs.Add(new Vector3(x,y));
                uv.Add(new Vector2(x / (float)GridSizeX, y / (float)GridSizeY));
                normals.Add(new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
        mesh.vertices = vertexs.ToArray();
        mesh.uv = uv.ToArray();
        mesh.normals = normals.ToArray();

        randomVertexs.Clear();
        for (int y = 0; y < GridSizeY; y++)
            for (int x = 0; x < GridSizeX; x++)
                randomVertexs.Add(new Vertex(x, y));
        
        for (int i = 0; i < GridSizeY * GridSizeX; i++)
        {
            int index = UnityEngine.Random.Range(0, randomVertexs.Count);
            vertexTemp.Add(randomVertexs[index]);
            randomVertexs.RemoveAt(index);
        }

        randomVertexs.AddRange(vertexTemp);
        vertexTemp.Clear();

        var trangles = ListPool<int>.Get();
        var temp = ListPool<int>.Get();
        if (!random)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                int row = y * (GridSizeX + 1);
                for (int x = 0; x < GridSizeX; x++)
                {
                    int bIndice = x + row;
                    temp.Clear();
                    temp.Add(bIndice);
                    temp.Add(bIndice + GridSizeX + 1);
                    temp.Add(bIndice + 1);
                    temp.Add(bIndice + 1);
                    temp.Add(bIndice + GridSizeX + 1);
                    temp.Add(bIndice + GridSizeX + 2);
                    trangles.AddRange(temp);

                    mesh.triangles = trangles.ToArray();
                    mesh.RecalculateNormals();

                    yield return gInterval;
                }
            }
        }
        else
        {
            for (int i = 0; i < randomVertexs.Count; i++)
            {
                int bIndice = 0;
                var vertex = randomVertexs[i];
                if (vertex.y >= GridSizeY && vertex.x >= GridSizeX)
                    continue;

                int row = vertex.y * (GridSizeX + 1);
                bIndice = vertex.x + row;

                temp.Clear();
                temp.Add(bIndice);
                temp.Add(bIndice + GridSizeX + 1);
                temp.Add(bIndice + 1);
                temp.Add(bIndice + 1);
                temp.Add(bIndice + GridSizeX + 1);
                temp.Add(bIndice + GridSizeX + 2);
                trangles.AddRange(temp);

                mesh.triangles = trangles.ToArray();
                mesh.RecalculateNormals();
                meshFilter.mesh = mesh;

                yield return gInterval;
            }
        }

        //other version
        //use in OpenGL
        //int[] triangles = new int[GridSizeX * GridSizeY * 6];
        //for (int ti = 0, vi = 0, y = 0; y < GridSizeY; y++, vi++)
        //{ 
        //    for (int x = 0; x < GridSizeX; x++, ti += 6,vi++)
        //    {
        //        triangles[ti] = vi;
        //        triangles[ti + 1] = triangles[ti + 4] = vi + GridSizeX + 1;
        //        triangles[ti + 2] = triangles[ti + 3] = vi + 1;
        //        triangles[ti + 5] = vi + GridSizeX + 2;
        //    }
        //}

        meshFilter.mesh = mesh;

        ListPool<int>.Release(trangles);
        ListPool<int>.Release(temp);
        ListPool<Vector2>.Release(uv);

        //if (canvasRenderer)
        //{
        //    canvasRenderer.SetMaterial(material, 0);
        //    canvasRenderer.SetMesh(mesh);
        //}

        working = false;
        yield break;
    }
    private void OnDrawGizmos()
    {
        if (!DisplayVertex)
            return;

        foreach (var vertex in vertexs)
        {
            Gizmos.DrawSphere(vertex,0.1f);
        }
    }
}

[CustomEditor(typeof(ProceduralGrid))]
public class ProceduralGridEditor:Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var instance = target as ProceduralGrid;
        if (!target) return;

        if (GUILayout.Button("Refresh Grid"))
        {
            instance.ReGenerate();
        }
    }
}
