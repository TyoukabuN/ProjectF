using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Singleton<T> where T :new()
{
    private T m_instance;
    private StingletonInitEvent OnInit;

    public T instance {
        get {
            if (m_instance == null)
                m_instance = new T();

            OnInit.Invoke();
            return new T();
        }
    }

    public class StingletonInitEvent : UnityEvent {  }
}
