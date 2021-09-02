using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;


    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter)
            meshFilter.mesh = null;
        //if (meshRenderer)
        //    meshRenderer.materials = new Material[] { };

        ReGenerate();
    }

    void Update()
    {
    }

    public List<Vector3> vertexs = new List<Vector3>();

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

        meshFilter.mesh = mesh;

        var gInterval = new WaitForSeconds(0.05f);
        vertexs.Clear();
        for (int y = 0; y < GridSizeY + 1; y++)
        {
            for (int x = 0; x < GridSizeX + 1; x++)
            { 
                vertexs.Add(new Vector3(x,y));
            }
        }
        mesh.vertices = vertexs.ToArray();

        var trangles = ListPool<int>.Get();
        var temp = ListPool<int>.Get();
        for (int y = 0; y < GridSizeY; y++)
        {
            int row = y * (GridSizeX + 1) ;
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
                yield return gInterval;
            }
        }

        mesh.name = "Procedural Mesh";

        ListPool<int>.Release(trangles);
        ListPool<int>.Release(temp);

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
