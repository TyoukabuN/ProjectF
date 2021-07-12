using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class PlayerController : Controller, IControllable
{
    public void Jump(float timeStep = 0)
    {
        displacement += Vector3.up * Speed * timeStep;
    }
    public bool OnInputCheck_Jump(float timeStep = 0)
    {
        bool anyJump = false;

        if (Input.GetKey(InputDefine.Jump))
        {
            anyJump = anyJump || true;
            this.Jump(Time.deltaTime);
        }
        return anyJump;
    }

}
