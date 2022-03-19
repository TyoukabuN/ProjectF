using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class PlayerController 
{
    public float JumpSpeed = 10;
    public int CanJumpTime = 2;
    public int JumpCounter = 0;

    public bool CanJump()
    {
        return JumpCounter < CanJumpTime;
    }
    public int LeftJumpTime()
    {
        return CanJumpTime - JumpCounter;
    }
    public bool RecoverJumpTime()
    {
        JumpCounter = 0;
        return true;
    }
    public float jumpAddtion = 0.05f;
    public void Jump(float timeStep = 0)
    {
        if (JumpCounter >= CanJumpTime)
            return;

        JumpCounter++;
        if(up.y <= 0)
            up = Vector3.zero;
        up += Vector3.up * JumpSpeed;
        Debug.Log("Jump");
        transform.position += Vector3.up * jumpAddtion;
    }
    public bool OnInputCheck_Jump(float timeStep = 0)
    {
        bool anyJump = false;

        if (GetKeyDown(InputDefine.Jump))
        {
            anyJump = anyJump || true;
        }
        if (GetKey(InputDefine.Jump))
        {
            //Jumping();
        }
        return anyJump;
    }

}
