using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GalScenePool
{
    public GalScene firstScene;
    public GalScene lastScene;

    public void Add(GalScene galScene)
    {

    }

    public void Delete(GalScene galScene)
    {

    }

    public void Clear()
    {
        firstScene = null;
        lastScene = null;
    }
}
public class GalSceneManager:Singleton<GalSceneManager>
{
    public List<GalScene> galScenes;

    private event UnityAction OnComplete;

    private GameObject canvasObj;

    public GalSceneManager()
    {
        if (galScenes == null)
        {
            galScenes = new List<GalScene>();
        }
        if (canvasObj == null)
        {
            canvasObj = InitCanvas();
        }
    }
    public void AddCompleteEvent(UnityAction unityAction)
    {
        OnComplete += unityAction;
    }
    public void RemoveCompleteEvent(UnityAction unityAction)
    {
        OnComplete -= unityAction;
    }
    public void RemoveAllEvent()
    {
        OnComplete = null;
    }

    public void LoadScene(string name)
    {
        GameObject sceneObj = LoadScenePrefab(name);
        sceneObj.transform.SetParent(canvasObj.transform);
        RectTransform sceneRect =  sceneObj.GetComponent<RectTransform>();
        sceneRect.anchoredPosition = Vector2.zero;
        sceneRect.sizeDelta = Vector2.zero;
        if (OnComplete != null)
        {
            OnComplete.Invoke();
        }
    }

    public GameObject LoadScenePrefab(string name)
    {
        string path =  "Assets/GUI/GalTalk/Prefabs/" + name +".prefab";
#if UNITY_EDITOR
        GameObject obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        obj = GameObject.Instantiate(obj);
#endif

        return obj;
    }
    private GameObject InitCanvas()
    {
        GameObject canvas = new GameObject("GalCanvas");
        GameObject.DontDestroyOnLoad(canvas);
        Canvas canvasComponent =  canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        return canvas;
    }
}
