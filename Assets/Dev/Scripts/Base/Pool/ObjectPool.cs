using System.Collections.Generic;
using UnityEngine.Events;

public class ObjectPool<T> where T : new()
{
    private int count = 0;
    private Stack<T> m_Stack = new Stack<T>();

    private UnityAction<T> OnGet;
    private UnityAction<T> OnRelease;

    public ObjectPool(UnityAction<T> OnGet, UnityAction<T> OnRelease)
    {
        this.OnGet = OnGet;
        this.OnRelease = OnRelease;
    }    

    public T Get()
    {
        T obj;

        if (m_Stack.Count <= 0)
        {
            count++;
            obj = new T();
        }
        else
        {
            obj = m_Stack.Pop();
        }

        if (OnGet != null)
        {
            OnGet(obj);
        }

        return obj;
    }

    public bool Release(T obj)
    {
        if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), obj))
            return false;

        if (OnRelease != null)
            OnRelease(obj);

        m_Stack.Push(obj);
        return true;
    }
}
