using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    [Range(0, 10)]
    public int GridSizeX = 4;
    [Range(0, 10)]
    public int GridSizeY = 2;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public Image image;
    CanvasRenderer canvasRenderer;
    public Material material;


    void Awake()
    {
        //meshFilter = GetComponent<MeshFilter>();
        //meshRenderer = GetComponent<MeshRenderer>();
        if(image)
            canvasRenderer = image.GetComponent<CanvasRenderer>();

        if (meshFilter)
            meshFilter.mesh = null;
        //if (meshRenderer)
        //    meshRenderer.materials = new Material[] { };

        ReGenerate();

        //meshRenderer.material = ShaderHandle.GetMaterial("Custom_UVChecker");
    }

    void Update()
    {
    }

    public List<Vector3> vertexs = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();

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

        if (!meshFilter)
            yield return null;
        //meshFilter.mesh = mesh;

        var gInterval = new WaitForSeconds(0.05f);
        vertexs.Clear();
        normals.Clear();

        if (GridSizeX <= 0 || GridSizeY <= 0)
        {
            mesh.name = "Procedural Mesh";
            meshFilter.mesh = mesh;
        }

        var uv = ListPool<Vector2>.Get();
        for (int y = 0; y < GridSizeY + 1; y++)
        {
            for (int x = 0; x < GridSizeX + 1; x++)
            {
                vertexs.Add(new Vector3(x, y));
                uv.Add(new Vector2(x / GridSizeX, y / GridSizeY));
                normals.Add(new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
        mesh.vertices = vertexs.ToArray();
        mesh.uv = uv.ToArray();

        var trangles = ListPool<int>.Get();
        var temp = ListPool<int>.Get();
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
        meshFilter.mesh = mesh;
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
        meshFilter.mesh = mesh;

        ListPool<int>.Release(trangles);
        ListPool<int>.Release(temp);
        ListPool<Vector2>.Release(uv);

        if (canvasRenderer)
        {
            canvasRenderer.SetMaterial(material, 0);
            canvasRenderer.SetMesh(mesh);
        }

        working = false;
        yield break;
    }
    private void OnDrawGizmos()
    {
        foreach (var vertex in vertexs)
        {
            Gizmos.DrawSphere(vertex, 0.1f);
        }
    }
}

[CustomEditor(typeof(ProceduralGrid))]
public class ProceduralGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var instance = target as ProceduralGrid;
        if (!target) return;

        if (GUILayout.Button("Refresh Grid"))
        {
            if (EditorApplication.isPlaying)
                instance.ReGenerate();
            else
                EditorApplication.isPlaying = true;
        }
    }
}
