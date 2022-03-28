using System.Collections.Generic;
using UnityEngine;


public partial class Controller
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

        Debug.Log(string.Format("<color=red>{0}</color>","跳跃"));
        JumpCounter++;
        if (up.y <= 0)
            up = Vector3.zero;
        up += Vector3.up * JumpSpeed;
        transform.position += Vector3.up * (jumpAddtion + groundedRadius);
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
