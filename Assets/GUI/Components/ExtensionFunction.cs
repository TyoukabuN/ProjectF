using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionFunction 
{
    public static void SetLocalRotation(this Transform t, float x, float y, float z)
    {
        t.localRotation = Quaternion.Euler(x, y, z);
    }
    public static Transform[] FindAllChilds(this Transform t, bool isFindEvery = false)
    {
        Transform[] childs = null;
        if (isFindEvery)
        {
            childs = t.GetComponents<Transform>();
        }
        else
        {
            int count = t.childCount;
            childs = new Transform[count];
            for (int i = 0; i < count; i++)
                childs[i] = t.GetChild(i);
        }
        return childs;
    }
}
