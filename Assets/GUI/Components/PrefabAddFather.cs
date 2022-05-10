using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;

public class PrefabAddFather : EditorWindow
{
    private string path;

    private Vector3 childObjPos = Vector3.zero;
   
    private Vector3 childObjRot = Vector3.zero;
    
    private Vector3 childObjScale = Vector3.zero;

    private bool flodout = false;

    private bool isSetPos = false;
    private bool isSetRot = false;
    private bool isSetScale = false;
    private void OnGUI()
    {
        GUILayout.Label("*把需要修改的预制集中放到一个文件夹里操作");
        GUILayout.Space(20);
        if (GUILayout.Button("选择路径"))
        {
            path = EditorUtility.OpenFolderPanel("选择", "", "");
        }
        GUILayout.Label(path);
        flodout = EditorGUILayout.Foldout(flodout, "子物体参数设置");
        if (flodout)
        {
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            GUILayout.Label("子物体Position:");
            isSetPos = EditorGUILayout.Toggle(isSetPos);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.Vector3Field("Position", childObjPos);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(300));
            GUILayout.Label("子物体Rotation:");
            isSetRot = EditorGUILayout.Toggle(isSetRot);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.Vector3Field("Rotation", childObjRot);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(300));
            GUILayout.Label("子物体Scale:");
            isSetScale = EditorGUILayout.Toggle(isSetScale);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.Vector3Field("Scale", childObjScale);
            GUILayout.EndHorizontal();
        }
        
        if (GUILayout.Button("开始"))
        {
            SearchPrefab(path,CreatePrefab);
        }
        if (GUILayout.Button("还原"))
        {
            SearchPrefab(path, ResetPrefab);
        }
        if (GUILayout.Button("二进制"))
        {
            Save(()=> { });
            SaveByJson();
            SaveXML();
        }
        if (GUILayout.Button("读二进制"))
        {
            Read("G:/DataT.lob",(data) => { Debug.LogError(data.name); });
            ReadByJson();
            ReadByXML();
        }
    }
    [MenuItem("Tools/预制批量新增父物体")]

    private static void SetWindow()
    {
        PrefabAddFather window = GetWindow<PrefabAddFather>();
        window.titleContent = new GUIContent("批量预制");
        window.Show();
    }

    private void CreatePrefab(GameObject obj)
    {
        if (obj)
        {
            string p = AssetDatabase.GetAssetPath(obj);
            
            obj = Instantiate(obj);
            if (obj.transform.Find("root"))
            {
                Debug.Log(obj.name + " 已跳过");
                DestroyImmediate(obj);
                return;
            }
            GameObject tempObj = new GameObject(obj.name);
            tempObj = GameObject.Instantiate(tempObj);
            tempObj.transform.localPosition = Vector3.zero;
            obj.name = "root";
            obj.transform.SetParent(tempObj.transform);
            if (isSetPos)
            {
                obj.transform.position = childObjPos;
            }
            if (isSetRot)
            {
                obj.transform.SetLocalRotation(childObjRot.x,childObjRot.y,childObjRot.z);
            }
            if (isSetScale)
            {
                obj.transform.localScale = childObjScale;
            }
            PrefabUtility.SaveAsPrefabAsset(tempObj, p);
            DestroyImmediate(obj);
            DestroyImmediate(tempObj);
        }
    }

    private void ResetPrefab(GameObject obj)
    {
        if (obj)
        {
            string p = AssetDatabase.GetAssetPath(obj);
            obj = Instantiate(obj);
            Transform root = obj.transform.Find("root");
            if (root)
            {
                Transform[] childs =  root.transform.FindAllChilds();
                for (int i = 0; i < childs.Length; i++)
                {
                    childs[i].SetParent(obj.transform);
                }
                DestroyImmediate(root.gameObject);
            }
            PrefabUtility.SaveAsPrefabAsset(obj, p);
            DestroyImmediate(obj);
        }
    }
    private void SearchPrefab(string path,UnityAction<GameObject> action)
    {
        string[] absolutePaths = System.IO.Directory.GetFiles(path, "*.prefab", System.IO.SearchOption.AllDirectories);
        try
        {
            EditorUtility.DisplayProgressBar("修改", "执行中……", 0 / absolutePaths.Length);
            for (int i = 0; i < absolutePaths.Length; i++)
            {
                EditorUtility.DisplayProgressBar("修改", "执行中……", i / absolutePaths.Length);
                string objPath = "Assets" + absolutePaths[i].Remove(0, Application.dataPath.Length);
                objPath = objPath.Replace("\\", "/");

                GameObject prefab = AssetDatabase.LoadAssetAtPath(objPath, typeof(GameObject)) as GameObject;
                if (action != null)
                {
                    action.Invoke(prefab);
                }
            }
            EditorUtility.ClearProgressBar();
        }
        catch (System.Exception)
        {
            Debug.LogError("cuol");
            EditorUtility.ClearProgressBar();
        }
        
    }
    [System.Serializable]
    public class Data
    {
        public string name = "bbbb";
        public int num = 123;
        public Dictionary<int, Data> dataDic = new Dictionary<int, Data>();
    }
    
    private void Save(UnityAction ua)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream("G:/DataT.lob", FileMode.OpenOrCreate);
        Data data = new Data();
        data.dataDic.Add(1, data);
        data.name = "sss";
        bf.Serialize(fs, data);
        fs.Close();
    }
    private void Read(string path,UnityAction<Data> data)
    {
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(path, FileMode.Open);
            Data data1 = bf.Deserialize(fs) as Data;
            if (data != null)
            {
                data.Invoke(data1);
            }
            fs.Close();
            
        }
        
    }
    private void SaveByJson()
    {
        Data data = new Data();
        data.name = "json";
        string js = JsonUtility.ToJson(data);
        StreamWriter sw = new StreamWriter("G:/DataT.jss");
        sw.Write(js);
        sw.Close();
    }
    private void ReadByJson()
    {
        if (File.Exists("G:/DataT.jss"))
        {
            StreamReader sr = new StreamReader("G:/DataT.jss");
            string jsStr = sr.ReadToEnd();
            sr.Close();
            Data data = JsonUtility.FromJson<Data>(jsStr);
            Debug.LogError(data.name);
            
        }
    }
    private void SaveXML()
    {
        Data data = new Data();
        data.name = "xml";
        XmlDocument xmlDocument = new XmlDocument();

        #region CreateXML elements

        XmlElement root = xmlDocument.CreateElement("Save");
        //创建一个参数名为"Save"的xml元素，这个元素名字叫root
        //这么取在脚本里就更好理解了
        root.SetAttribute("FileName", "File_01");

        XmlElement coinElement = xmlDocument.CreateElement("name");
        coinElement.InnerText = data.name.ToString();
        //创建金币保存的元素，并将金币数以字符串形式导入
        root.AppendChild(coinElement);
        //将coinElement这一元素附录至根节点root上

        XmlElement playerPositionXElement = xmlDocument.CreateElement("num");
        playerPositionXElement.InnerText = data.num.ToString();
        root.AppendChild(playerPositionXElement);
        //接下来就是重复步骤：创建节点、储存数据、将节点附录至根节点


        #endregion
        xmlDocument.AppendChild(root);
        //最后要把根节点附录在文件上，以便于文件可以保存    
        xmlDocument.Save("G:/DataT.xmll");
        //把数据保存在文件“DataXML.yj”里
        if (File.Exists("G:/DataT.xmll"))
        {
            Debug.Log("XML FILE SAVED");
        }
    }

    private void ReadByXML()
    {
        if (File.Exists("G:/DataT.xmll"))
        {
            Data data = new Data();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("G:/DataT.xmll");
            //创建并读取保存的XML文件
            XmlNodeList name = xmlDocument.GetElementsByTagName("name");
            //寻找标签名称来找到保存在文件里的金币数
            string name1 = name[0].InnerText;
            //MARKER 为什么是[0]呢？因为如果标签名为Coins的有很多的话，就会重复
            //所以返回的是List集合类型，第一个为[0]，第二个为[1]
            //将String类的金币数通过Parse转化为int并存进变量里
            data.name = name1;
            XmlNodeList num = xmlDocument.GetElementsByTagName("num");
            int num1 = int.Parse(num[0].InnerText);
            data.num = num1;

        }
    }
}
