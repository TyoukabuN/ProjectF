using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
public class GalMono : MonoSingleton<GalMono>
{
    string scenes;  //实验用
    bool switchb;   //试验用
    public List<MonoBehaviour> monoList;
    //private static GalMono _instance;
    //public static GalMono Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            GameObject obj = new GameObject("GalMono");
    //            DontDestroyOnLoad(obj);
    //            _instance =  obj.AddComponent<GalMono>();
    //        }
    //        return _instance;
    //    }
    //}

    public UnityAction OnInitComplete;
    public void Init()
    {
        monoList = new List<MonoBehaviour>();

        if (OnInitComplete != null)
        {
            OnInitComplete.Invoke();
        }
    }
    /// <summary>
    /// 游戏运行执行
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    public static void OnGameStarRun()
    {
        
        GalMono.instance.Init();
    }
    void Start()
    {
        GalSceneManager.instance.AddCompleteEvent(() => { Debug.LogError("坚挺加载scene的事件");});
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadScene()
    {

    }
    #region 测试的部分
    //private void OnGUI()
    //{
    //    if (GUILayout.Button("点击load图片场景"))
    //    {
    //        GalSceneManager.instance.LoadScene("scene1");
    //    }
    //    if (GUILayout.Button("查看当前载入场景"))
    //    {
    //        scenes = "";
    //        GalScene node = GalSceneManager.instance.galScenePool.firstScene;
    //        while (node != null)
    //        {
    //            scenes = scenes +"\n" + node.name;
    //            node = node.nextScene;
    //        }
    //    }

    //    if (GUILayout.Button("切换场景"))
    //    {
    //        //string name = switchb ?"scene1":"scene2";
    //        //GalSceneManager.instance.SwitchScene(name);
    //        //switchb = !switchb;
    //        GalSceneManager.instance.SwitchScene();

    //    }
    //    GUILayout.Label("<color=red><size=30>"+scenes+"</size></color>");

    //    //if (GUILayout.Button("charu"))
    //    //{
    //    //    int[] arr = { 5, 8, 6, 4, 2, 1, 9, 7 };
    //    //    for (int i = 1; i < arr.Length; i++)
    //    //    {
    //    //        int nextValue = arr[i];
    //    //        int index = i - 1;
    //    //        while (index >= 0 && arr[index] > nextValue)
    //    //        {
    //    //            arr[index + 1] = arr[index];
    //    //            arr[index] = nextValue;
    //    //            index--;
    //    //        }
    //    //    }
    //    //    foreach (var item in arr)
    //    //    {
    //    //        Debug.Log(item);
    //    //    }
    //    //}
    //}
    #endregion
}
