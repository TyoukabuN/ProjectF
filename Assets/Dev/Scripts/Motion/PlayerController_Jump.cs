using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class PlayerController : Controller, IControllable
{
    public float JumpSpeed = 10;
    public void Jump(float timeStep = 0)
    {
        verticalVelocity = Vector3.zero;
        verticalVelocity += Vector3.up * JumpSpeed;
    }
    public bool OnInputCheck_Jump(float timeStep = 0)
    {
        bool anyJump = false;

        if (IsKeyOn(InputDefine.Jump))
        {
            anyJump = anyJump || true;
        }
        return anyJump;
    }



}
