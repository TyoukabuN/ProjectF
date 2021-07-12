using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class PlayerController : Controller, IControllable
{
    //Physice Properties
    public float Speed = 20;
    private Vector3 displacement = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 gravity = -Vector3.up * 10f;
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
        displacement += velocity * timeStep + gravity * timeStep * (transform.position.y > 0 ? 1 : 0);
        transform.position += displacement;
        displacement = Vector3.zero;
        velocity = Vector3.zero;
    }

    public bool OnInputCheck_Move(float timeStep = 0)
    {
        bool anyMove = false;

        if (IsKeyOn(InputDefine.Forward))
        {
            anyMove = anyMove || true;
            this.MoveForward(Time.deltaTime);
        }
        if (IsKeyOn(InputDefine.Backward))
        {
            anyMove = anyMove || true;
            this.MoveBackward(Time.deltaTime);
        }
        if (IsKeyOn(InputDefine.Left))
        {
            anyMove = anyMove || true;
            this.TurnLeft(Time.deltaTime);
        }
        if (IsKeyOn(InputDefine.Right))
        {
            anyMove = anyMove || true;
            this.TurnRight(Time.deltaTime);
        }

        return anyMove;
    }

}
