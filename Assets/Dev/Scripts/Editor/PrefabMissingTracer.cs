using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Object = UnityEngine.Object;

public class PrefabMissingTracer : EditorWindow
{

    [MenuItem("Tools/Prefab/搜索Missing Prefab")]
    static void Open()
    {
        GetWindow<PrefabMissingTracer>();
    }

    Object temp;

    private bool AnyMissingInSide(Transform trans)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            var child = trans.GetChild(i);
            if (AnyMissingInSide(child))
            {
                return true;
            }
        }

        var prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(trans.gameObject);
        if (prefabInstanceStatus == PrefabInstanceStatus.MissingAsset)
        {
            return true;
        }

        return false;
    }

    private void OnGUI()
    {
        //temp = EditorGUILayout.ObjectField("Test Object", temp,typeof(Object),true);

        foreach (var missingObj in MissingList)
        {
            EditorGUILayout.ObjectField("", missingObj, typeof(Object), true);
        }

        if (GUILayout.Button("search"))
        {
            //AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(temp));
            //Debug.Log(PrefabUtility.IsPrefabAssetMissing(temp));
            //Debug.Log(PrefabUtility.GetPrefabInstanceStatus(temp));
            //var instantiate = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(temp));
            //Debug.Log(AnyMissingInSide(instantiate.transform));

            Search();
        }
    }
    private List<string> allPrefabPath = new List<string>();
    private List<Object> MissingList = new List<Object>();
    void Search()
    {
        allPrefabPath.Clear();
        MissingList.Clear();

        var temp = AssetDatabase.GetAllAssetPaths();


        List<string> temp2 = new List<string>();
        for (int i = 0; i < temp.Length; i++)
        {
            var assetPath = temp[i];
            var ext = Path.GetExtension(assetPath);
            var fileNanem = Path.GetFileName(assetPath);

            if (ext == ".prefab")
            {
                allPrefabPath.Add(assetPath);
            }
        }

        float FinishCount = 0.0f;
        float onDependCount = allPrefabPath.Count;
        for (int i = 0; i < allPrefabPath.Count; i++)
        {
            var assetPath = allPrefabPath[i];
            EditorUtility.DisplayProgressBar("Tracing...", string.Format("{0} {1}/{2}", assetPath, FinishCount, onDependCount), FinishCount / onDependCount);

            var instantiate = PrefabUtility.LoadPrefabContents(assetPath);
            if (instantiate == null)
            {
                continue;
            }

            if (AnyMissingInSide(instantiate.transform))
            {
                MissingList.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)));
                Debug.Log(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)));
            }

            PrefabUtility.UnloadPrefabContents(instantiate);
            FinishCount++;
        }

        EditorUtility.ClearProgressBar();
    }
}
