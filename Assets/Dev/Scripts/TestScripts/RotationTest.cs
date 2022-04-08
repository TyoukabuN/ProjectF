using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteAlways]
public class RotationTest : MonoBehaviour
{
    [Header("<叉乘向量>")]
    public Transform Vector1;
    public Transform Vector2;
    [Header("<四元数数值>")]
    public bool MulWithQuaternion = false;
    public Transform RotationAxi;
    public float Angle = 0;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    List<Vector3> vertexs = new List<Vector3>();
    List<int> trangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uv;
    // Start is called before the first frame update
    void OnEnable()
    {
    }    
    void Update()
    {
        Test();
    }
    public void Init()
    {
        meshFilter = GetComponent<MeshFilter>()??gameObject.AddComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
    }
    public void Test()
    {
        if (Vector1 == null || Vector2 == null)
            return;

        Init();

        var cross = Vector3.Cross(Vector1.forward, Vector2.forward);

        //Debug.Log(refTransform1.forward);
        //Debug.Log(refTransform2.forward);
        //Debug.Log(cross);
        //Debug.Log(cross.magnitude);

        vertexs.Clear();
        vertexs.Add(transform.position);
        vertexs.Add(Vector1.position + Vector1.forward);
        vertexs.Add(Vector1.forward + Vector2.forward);
        vertexs.Add(Vector2.position + Vector2.forward);

        if (MulWithQuaternion && RotationAxi)
        {
            var axi = RotationAxi.up * Mathf.Sin(Angle / 2 * Mathf.Deg2Rad);
            var q = new Quaternion(axi.x, axi.y, axi.z, Mathf.Cos(Angle/2 * Mathf.Deg2Rad));
            for (int i=0;i< vertexs.Count;i++)
                if (vertexs[i] != null)
                    vertexs[i] = q.normalized * vertexs[i];
        }
        
        trangles.Clear();
        
        if (!mesh)
        {
            mesh = new Mesh();
            mesh.name = "平行四边形";
        }
        mesh.vertices = vertexs.ToArray();
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        //
        if (uv == null)
        {
            uv = new List<Vector2>() {
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0)
            };
        }
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(RotationTest))]
public class MathematicTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var handle = target as RotationTest;
        if (handle == null)
            return;
        GUILayout.Space(10);
        EditorGUILayout.LabelField("<向量信息>");
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.TextField("向量①:", handle.Vector1.forward.ToString());
            EditorGUILayout.TextField("向量②:", handle.Vector2.forward.ToString());
            var cross = Vector3.Cross(handle.Vector1.forward, handle.Vector2.forward);
            EditorGUILayout.Vector3Field("CrossProduct:", cross);
            EditorGUILayout.TextField("CrossProduct:", cross.ToString());
            EditorGUILayout.TextField("CrossProductMagnitude:", cross.magnitude.ToString());


            if (GUILayout.Button("复位"))
            {
                handle.RotationAxi.rotation = Quaternion.identity;
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif