using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListPool<T>
{
    private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, Clear);

    static void Clear(List<T> list)
    {
        list.Clear();
    }

    public static List<T> Get()
    {
        return s_ListPool.Get();
    }

    public static bool Release(List<T> list)
    {
        return s_ListPool.Release(list);
    }
}
    
