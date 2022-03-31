using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter)),RequireComponent(typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    [Range(0,10)]
    public int GridSizeX = 4;
    [Range(0, 10)]
    public int GridSizeY = 2;

    public float genaralInteral = 0.05f;
    public bool random = false;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public Image image;
    CanvasRenderer canvasRenderer;
    public Material material;


    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        canvasRenderer = image.GetComponent<CanvasRenderer>();

        if (meshFilter)
            meshFilter.mesh = null;
        //if (meshRenderer)
        //    meshRenderer.materials = new Material[] { };

        meshRenderer.material = ShaderHandle.GetMaterial("Custom_UVChecker");

        ReGenerate();

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
            mesh = new Mesh();

        //meshFilter.mesh = mesh;

        var gInterval = new WaitForSeconds(genaralInteral);
        vertexs.Clear();
        normals.Clear();
        uv.Clear();


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

        rows.Clear();
        cols.Clear();
        for (int i = 0; i < GridSizeY; i++)
            rows.Add(i);
        for (int i = 0; i < GridSizeX; i++)
            cols.Add(i);

        indexTemp.Clear();
        for (int i = 0; i < GridSizeY; i++)
        {
            int index = Random.Range(0, rows.Count);
            indexTemp.Add(rows[index]);
            rows.RemoveAt(index);
        }
        rows.AddRange(indexTemp);
        indexTemp.Clear();
        for (int i = 0; i < GridSizeX; i++)
        {
            int index = Random.Range(0, cols.Count);
            indexTemp.Add(cols[index]);
            cols.RemoveAt(index);
        }
        cols.AddRange(indexTemp);

        randomVertexs.Clear();
        for (int y = 0; y < GridSizeY; y++)
            for (int x = 0; x < GridSizeX; x++)
                randomVertexs.Add(new Vertex(x, y));

        for (int i = 0; i < randomVertexs.Count; i++)
        {
            randomVertexs[i].x = cols[randomVertexs[i].x];
            randomVertexs[i].y = rows[randomVertexs[i].y];
        }


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
                var vertex = randomVertexs[i];
                int row = rows[(int)vertex.y] * (GridSizeX + 1);

                int bIndice = cols[(int)vertex.x] + row;
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

        mesh.normals = normals.ToArray();
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

        mesh.name = "Procedural Mesh";
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
