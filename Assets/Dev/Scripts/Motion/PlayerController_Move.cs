using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class PlayerController : Controller, IControllable
{
    public float Speed = 20;
    private Vector3 displacement = Vector3.zero;
    public void MoveForward(float timeStep = 0)
    {
        displacement += Vector3.forward * Speed * timeStep;
    }
    public void MoveBackward(float timeStep = 0)
    {
        displacement -= Vector3.forward * Speed * timeStep;
    }

    public void TureLeft(float timeStep = 0)
    {
        displacement -= Vector3.right * Speed * timeStep;
    }

    public void TureRight(float timeStep = 0)
    {
        displacement += Vector3.right * Speed * timeStep;
    }

    public void OnMotion(float timeStep = 0)
    {
        transform.position += displacement;
        displacement = Vector3.zero;
    }


}
