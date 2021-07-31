using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Point : MonoBehaviour
{
    public Vector3 Acceleration = new Vector3(0,-9.8f,0);
    public Vector3 OldPosition;
    public bool simulate = true;

    public void Tick(float timeStep)
    {
        var point = this;

        var temp = point.transform.position;

        point.transform.position += point.transform.position - point.OldPosition + point.Acceleration* timeStep * timeStep;

        point.OldPosition = temp;
    }
    public void Update()
    {
        if (!simulate)
            return;

        Tick(Time.deltaTime);
    }
}

