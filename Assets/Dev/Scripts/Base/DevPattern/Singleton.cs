using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Singleton<T> where T : Singleton<T>,new()
{
    protected static T m_instance;
    protected StingletonInitEvent OnInit;

    public static T instance {
        get {
            if (m_instance == null)
                m_instance = new T();

            m_instance.OnInit.Invoke();
            return new T();
        }
    }

    public class StingletonInitEvent : UnityEvent {  }
}
