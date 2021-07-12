using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandState : State
{
    public override bool OnUpdate(float timeStep = 0)
    {
        bool anyMove = false;

        if (Input.GetKey(InputDefine.Forward) ||
            Input.GetKey(InputDefine.Backward) ||
            Input.GetKey(InputDefine.Left) ||
            Input.GetKey(InputDefine.Right) )
        {
            anyMove = anyMove || true;
            stateMachine.Enter((int)PlayerController.StateType.Move);
        }
        return true;
    }
}
