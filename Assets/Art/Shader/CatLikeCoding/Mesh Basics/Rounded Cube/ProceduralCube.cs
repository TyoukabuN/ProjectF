using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProceduralCube : MonoBehaviour
{
    //这里size不是顶点数而是正方形数//可以理解成edge数
    public int xSize = 3;
    public int ySize = 3;
    public int zSize = 3;
    // 
    public float genaralInteral = 0.05f;
    public Material material;
    public Vector3[] vertices;
    public int[] triangles;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;


    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter)
            meshFilter.mesh = null;

        ReGenerate();
    }
    public void ReGenerate()
    {
        if (material != null)
            meshRenderer.material = material;
        Generate();
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cubbe";
        WaitForSeconds wait = new WaitForSeconds(genaralInteral);

        CreateVertices();
        CreateTriangle();

        mesh.RecalculateNormals();
    }


    private void CreateVertices()
    {
        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1)) * 2;

        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

        int v = 0;
        for (int y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[v++] = new Vector3(x, y, 0);
            }
            for (int z = 1; z <= zSize; z++)
            {
                vertices[v++] = new Vector3(xSize, y, z);
            }
            for (int x = xSize - 1; x >= 0; x--)
            {
                vertices[v++] = new Vector3(x, y, zSize);
            }
            for (int z = zSize - 1; z > 0; z--)
            {
                vertices[v++] = new Vector3(0, y, z);
            }
        }

        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                vertices[v++] = new Vector3(x, ySize, z);
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                vertices[v++] = new Vector3(x, 0, z);
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
    //v01  v11
    //
    //v00  v10
    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        //clockwise is front
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }
    private void CreateTriangle()
    {
        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
        triangles = new int[quads * 6];
        int ring = (xSize + zSize) * 2;//周长
        int t = 0, v = 0;
        for (int y = 0; y < ySize; y++, v++)
        { 
            for (int q = 0; q < ring - 1; q++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            }
            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
        }
        t = CreateTopFace(triangles,t, ring);
        t = CreateBottomFace(triangles, t, ring);

        mesh.triangles = triangles;
    }
    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        int v = 1;
        int MidDiff = (xSize - 1) * (zSize - 1);
        int vMid = ring * (ySize + 1) + MidDiff;
        t = SetQuad(triangles, t, ring - 1, vMid,0,1);

        for (int x = 1; x <= xSize - 2; x++, vMid++,v++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, x,x + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        int vMin = ring - 2;
        vMid -= xSize - 2;
        for (int z = 1; z <= zSize - 2; z++, vMin--,vMid++, v++)
        {
            t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);

            for (int x = 1; x <= xSize - 2; x++, vMid++, v++)
            {
                t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid , vMid + 1);
            }
        }

        return 1;
    }

    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        int v = ring * ySize;
        for (int x = 0; x <= xSize - 2; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;


        for (int z = 1; z <= zSize - 2; z++, vMin--, vMid++, vMax++)
        { 
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);//first quad

            //middle grid
            for (int x = 1; x <= xSize - 2; x++,vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
            }

            //other side quad
            t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
        }

        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vMin, vMid, vTop , vTop - 1);

        for (int x = 1; x <= xSize - 2; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop - 1, vTop - 2);
        }
        t = SetQuad(triangles, t, vMid, vTop - 3, vTop - 1, vTop - 2);

        return t;
    }

#if UNITY_EDITOR
    private GUIStyle GizmosGUIStyle = null;
    private void OnDrawGizmos()
    {
        if (GizmosGUIStyle == null)
        {
            GUIStyle style = new GUIStyle();
            style.richText = true;
            GizmosGUIStyle = style;
        }

        if (vertices == null)
            return;

        for(int i=0;i<vertices.Length;i++)
        {
            Handles.Label(vertices[i], i.ToString(), GizmosGUIStyle); ;
            var vertex = vertices[i];
            Gizmos.DrawSphere(vertex, 0.05f);
        }
    }
#endif

}
#if UNITY_EDITOR
[CustomEditor(typeof(ProceduralCube))]
public class ProceduralCubeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var instance = target as ProceduralCube;
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
#endif

