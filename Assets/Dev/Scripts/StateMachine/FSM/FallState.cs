using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateType = PlayerController.StateType;

public class FallState : State
{
    public FallState(Controller controllable, int stateKey = -1) : base(stateKey)
    {
        this.controller = controllable;
    }

    public override bool OnUpdate(float timeStep = 0)
    {
        if (controller == null)
            return false;

        var anyMove = controller.OnInputCheck_Move(Time.deltaTime);
        var anyJump = controller.OnInputCheck_Jump(Time.deltaTime);

        if (anyJump)
        {
            if(controller.CanRecoverJumpCounter())
                controller.RecoverJumpTime();

            if (controller.CanJump())
            {
                stateMachine.Enter((int)PlayerController.StateType.Jump);
                return true;
            }
        }
        if (controller.IsGround())
        {
            stateMachine.Enter((int)PlayerController.StateType.Stand);
            return true;
        }

        return true;
    }

    public override bool OnExit(int stateKey = -1)
    {
        if ((StateType)stateKey == StateType.Move ||
            (StateType)stateKey == StateType.Stand)
        {
            controller.RecoverJumpTime();
        }
        return base.OnExit(stateKey);
    }
}