using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Singleton<T> where T : Singleton<T>,new()
{
    protected static T m_instance;
    public static T instance {
        get {
            if (m_instance == null)
                m_instance = new T();

            return m_instance;
        }
    }
}
