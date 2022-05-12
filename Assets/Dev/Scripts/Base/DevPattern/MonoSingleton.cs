using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Util = DevPatternUtility;

public class MonoSingleton<T> : MonoBehaviour where T : Component
{
    static T m_instance;
    public static T current
    {
        get { return instance; }
    }
    public static T instance {
        get {
            if (m_instance == null)
            {
                var temp = new GameObject(string.Format("[{0}]", typeof(T).Name));
                temp.transform.SetParent(Util.MonoSingletonRoot,false);
                m_instance = temp.gameObject.AddComponent<T>();
            }

            return m_instance;
        }
    }
}
