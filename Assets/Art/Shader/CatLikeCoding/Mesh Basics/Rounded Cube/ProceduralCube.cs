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
    public int roundness = 2;
    public Texture GridTexture;
    // 
    public float genaralInteral = 0.05f;
    [HideInInspector] public Material material;
    [HideInInspector] public Vector3[] vertices;
    [HideInInspector] public Vector3[] normals;
    [HideInInspector] public Color32[] cubeUV;
    [HideInInspector] public int[] triangles;
    [HideInInspector] public int[] trianglesZ;
    [HideInInspector] public int[] trianglesX;
    [HideInInspector] public int[] trianglesY;

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
        normals = new Vector3[vertices.Length];
        cubeUV = new Color32[vertices.Length];

        int v = 0;
        for (int y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                SetVertex(v++, x, y, 0);
            }
            for (int z = 1; z <= zSize; z++)
            {
                SetVertex(v++, xSize, y, z);
            }
            for (int x = xSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, zSize);
            }
            for (int z = zSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }

        for (int z = 1; z < zSize; z++)
            for (int x = 1; x < xSize; x++)
                SetVertex(v++, x, ySize, z);

        for (int z = 1; z < zSize; z++)
            for (int x = 1; x < xSize; x++)
                SetVertex(v++, x, 0, z);


        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors32 = cubeUV;
        mesh.RecalculateNormals();
    }
    private void SetVertex(int i, int x, int y, int z)
    {
        Vector3 inner = vertices[i] = new Vector3(x, y, z);

        inner.x = Mathf.Clamp(inner.x, roundness, xSize - roundness);
        inner.y = Mathf.Clamp(inner.y, roundness, ySize - roundness);
        inner.z = Mathf.Clamp(inner.z, roundness, zSize - roundness);

        normals[i] = (vertices[i] - inner).normalized;
        vertices[i] = inner + normals[i] * roundness;
        cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z,0);
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
        //2个面每个quad有2个triangle,3个vertex indice定义1个triangle
        int[] trianglesZ = new int[(xSize * ySize * 6 * 2)]; 
        int[] trianglesX = new int[(zSize * ySize * 6 * 2)]; 
        int[] trianglesY = new int[(xSize * zSize * 6 * 2)];
        int tZ = 0, tX = 0, tY = 0, v = 0;
        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
        triangles = new int[quads * 6];
        int ring = (xSize + zSize) * 2;//周长
        for (int y = 0; y < ySize; y++, v++)
        {
            for (int q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < zSize; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < zSize - 1; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
        }
        tY = CreateTopFace(trianglesY, tY, ring);
        tY = CreateBottomFace(trianglesY, tY, ring);

        var matInfos = new MaterialInfo[3] { 
            new MaterialInfo("Z",trianglesZ,Color.white),
            new MaterialInfo("X",trianglesX,Color.red),
            new MaterialInfo("Y",trianglesY,Color.green),
        };

        mesh.subMeshCount = matInfos.Length;
        var materials = new Material[matInfos.Length];
        for (int i = 0; i < matInfos.Length; i++)
        {
            var info = matInfos[i];
            mesh.SetTriangles(info.triangles, i);
            var mat = new Material(ShaderHandle.GetShader("Custom/Rounded Cube Grid Self"));
            mat.name = info.name;
            mat.SetColor("_Color", info.color);
            mat.SetTexture("_MainTex", GridTexture);
            mat.EnableKeyword(info.faceKeyWord);
            materials[i] = mat;
            Renderer render = GameObject.FindObjectOfType(typeof(Renderer)) as Renderer;
            Shader.Find("Standard");
        }
        meshRenderer.materials = materials;

        //mesh.triangles = triangles;
    }
    struct MaterialInfo {
        public MaterialInfo(string name, int[] triangles, Color color, string namePrefix = "Material ")
        {
            this.name = namePrefix + name;
            this.triangles = triangles;
            this.color = color;
            this.faceKeyWord = "_FACES_" + name;
        }
        public string name;
        public int[] triangles;
        public Color color;
        public string faceKeyWord;
    }
    private void CreateTriangle2()
    {
        int[] trianglesZ = new int[(xSize * ySize * 6 * 2)]; //2个面每个quad有2个triangle,
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
        t = CreateTopFace(triangles, t, ring);
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
        int vMax = v + 3;
        for (int z = 1; z <= zSize - 2; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);

            for (int x = 1; x <= xSize - 2; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + xSize - 1, vMax, vMid, vMax - 1);
        }

        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

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
    [HideInInspector] public bool DisplayIndice = true;
    [HideInInspector] public bool DisplayNormal = true;

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
            var vertex = vertices[i];

            if(DisplayIndice) Handles.Label(vertex, i.ToString(), GizmosGUIStyle);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(vertex, 0.05f);

            if (DisplayNormal)
            { 
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(vertices[i], normals[i]);
            }
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
        if (!instance) return;

        if (GUILayout.Button("Refresh Grid"))
        {
            if (EditorApplication.isPlaying)
                instance.ReGenerate();
            else
                EditorApplication.isPlaying = true;
        }

        EditorGUILayout.Space();

        instance.DisplayIndice = EditorGUILayout.Toggle("显示顶点序号", instance.DisplayIndice);
        instance.DisplayNormal = EditorGUILayout.Toggle("显示法线", instance.DisplayNormal);
    }
}
#endif

