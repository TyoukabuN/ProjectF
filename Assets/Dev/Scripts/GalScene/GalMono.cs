using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GalMono : MonoBehaviour
{
    public List<MonoBehaviour> monoList;
    private static GalMono _instance;
    public static GalMono Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("GalMono");
                DontDestroyOnLoad(obj);
                _instance =  obj.AddComponent<GalMono>();
            }
            return _instance;
        }
    }

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
        
        GalMono.Instance.Init();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadScene()
    {

    }
}
