using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateType = PlayerController.StateType;

public class FallState : State
{
    public FallState(IControllable controllable, int stateKey = -1) : base(stateKey)
    {
        this.controllable = controllable;
    }

    public override bool OnUpdate(float timeStep = 0)
    {
        if (controllable == null)
            return false;

        var anyMove = controllable.OnInputCheck_Move(Time.deltaTime);
        var anyJump = controllable.OnInputCheck_Jump(Time.deltaTime);

        controllable.Motion(Time.deltaTime);


        if (anyJump && controllable.CanJump())
        {
            stateMachine.Enter((int)PlayerController.StateType.Jump);
        }
        if (controllable.IsGround())
        {
            stateMachine.Enter((int)PlayerController.StateType.Move);
        }

        return true;
    }

    public override bool OnExit(int stateKey = -1)
    {
        if ((StateType)stateKey != StateType.Jump &&
            (StateType)stateKey != StateType.Fall)
        {
            controllable.RecoverJumpTime();
        }
        return base.OnExit(stateKey);
    }
}