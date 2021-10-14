using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DevPatternUtility
{
    private static string monoSingletonRootName = "MonoSingletonRoot";

    private static Transform s_MonoSingletonRoot;
    public static Transform MonoSingletonRoot
    {
        get
        {
            if (s_MonoSingletonRoot == null)
            { 
                var root = GameObject.Find(monoSingletonRootName);
                if (root == null)
                {
                    root = new GameObject(monoSingletonRootName);
                    GameObject.DontDestroyOnLoad(root);
                }
                s_MonoSingletonRoot = root.transform;
            }
            return s_MonoSingletonRoot;
        }
    }
}
