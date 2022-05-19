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

    public GalScene curScene;
    public void Add(GalScene galScene)
    {
        if (firstScene == null)
        {
            firstScene = galScene;
            lastScene = galScene;
            return;
        }
        else
        {
            lastScene.nextScene = galScene;
            galScene.beforScene = lastScene;
            lastScene = galScene;
        }
    }

    public void Delete(GalScene galScene)
    {
        if (firstScene == null)
        {
            return;
        }
        GalScene node = firstScene;
        while (node.nextScene != null)
        {
            if (node.value == galScene)
            {
                GalScene before = node.beforScene;
                GalScene next = node.nextScene;
                node.beforScene.nextScene = next;
                node.nextScene.beforScene = before;
                return;
            }
            node = node.nextScene;
        }
    }

    public GalScene Find(string name)
    {
        if (firstScene == null)
        {
            return null;
        }
        GalScene node = firstScene;
        while (node != null)
        {
            if (node.name == name)
            {
                return node;
            }
            node = node.nextScene;
        }
        return null;
    }

    public void Clear()
    {
        firstScene = null;
        lastScene = null;
    }
}
public class GalSceneManager:Singleton<GalSceneManager>
{
    public GalScene currentScene;

    private event UnityAction OnComplete;

    public GameObject canvasObj;

    public GalScenePool galScenePool;
    public GalSceneManager()
    {
       
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

    public void SwitchScene(string name)
    {
        GalScene galScene = galScenePool.Find(name);
       
        if (galScene != null)
        {
            galScene.gameObject.transform.SetAsLastSibling();
            currentScene = galScene;
        }
        else
        {
            LoadScene(name);
        }
        
    }
    public void SwitchScene()
    {

        if (currentScene != null)
        {
            currentScene = currentScene.nextScene;
            if (currentScene != null)
            {
                currentScene.gameObject.transform.SetAsLastSibling();
            }
        }

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
        GalScene galScene = new GalScene(sceneObj, name);
        GalSceneManager.instance.galScenePool.Add(galScene);
        currentScene = galScene;
    }

    public GameObject LoadScenePrefab(string name)
    {
        string path =  "Assets/GUI/GalTalk/Prefabs/" + name +".prefab";
#if UNITY_EDITOR
        GameObject obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
#endif
        obj = GameObject.Instantiate(obj);
        return obj;
    }
    private GameObject InitCanvas()
    {
        galScenePool = new GalScenePool();

        GameObject canvas = new GameObject("GalCanvas");
        GameObject.DontDestroyOnLoad(canvas);
        Canvas canvasComponent =  canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        return canvas;
    }
}
