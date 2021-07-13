using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : State
{
    public JumpState(IControllable controllable, int stateKey = -1) : base(stateKey)
    {
        this.controllable = controllable;
    }
    public override bool OnEnter()
    {
        controllable.Jump(Time.deltaTime);
        return true;

    }
    public override bool OnUpdate(float timeStep = 0)
    {
        if (controllable == null)
            return false;

        var anyMove = controllable.OnInputCheck_Move(Time.deltaTime);
        var anyJump = controllable.OnInputCheck_Jump(Time.deltaTime);

        controllable.Motion(Time.deltaTime);

        if (controllable.IsGround())
        {
            stateMachine.Enter((int)PlayerController.StateType.Move);
        }

        return true;
    }
}
