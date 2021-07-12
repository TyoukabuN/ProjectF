using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class PlayerController : Controller, IControllable
{
    public void MoveForward(float timeStep = 0)
    {
        velocity += Vector3.forward * Speed;
    }
    public void MoveBackward(float timeStep = 0)
    {
        velocity -= Vector3.forward * Speed;
    }

    public void TurnLeft(float timeStep = 0)
    {
        velocity -= Vector3.right * Speed;
    }

    public void TurnRight(float timeStep = 0)
    {
        velocity += Vector3.right * Speed;
    }

    public void OnMotion(float timeStep = 0)
    {
        displacement += velocity * timeStep;
        transform.position += displacement;
        displacement = Vector3.zero;
    }

    public bool OnInputCheck_Move(float timeStep = 0)
    {
        bool anyMove = false;

        if (Input.GetKey(InputDefine.Forward))
        {
            anyMove = anyMove || true;
            this.MoveForward(Time.deltaTime);
        }
        if (Input.GetKey(InputDefine.Backward))
        {
            anyMove = anyMove || true;
            this.MoveBackward(Time.deltaTime);
        }
        if (Input.GetKey(InputDefine.Left))
        {
            anyMove = anyMove || true;
            this.TurnLeft(Time.deltaTime);
        }
        if (Input.GetKey(InputDefine.Right))
        {
            anyMove = anyMove || true;
            this.TurnRight(Time.deltaTime);
        }

        return anyMove;
    }

}
