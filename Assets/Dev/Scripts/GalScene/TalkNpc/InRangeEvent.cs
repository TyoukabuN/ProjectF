using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InRangeEvent : MonoBehaviour
{
    public UnityAction onStart;
    public UnityAction onUpdate;
    public UnityAction onExit;

    public bool isIn;
    public bool isDo;
    public bool isExit;
    // Update is called once per frame
    void Update()
    {
        if (isIn)
        {
            if (onStart != null)
            {
                onStart.Invoke();
            }
            isIn = false;
        }
        if (isDo)
        {
            if (onUpdate != null)
            {
                onUpdate.Invoke();
            }
        }
        if (isExit)
        {
            if (onExit != null)
            {
                onExit.Invoke();
            }
            isExit = false;
        }

    }
}
