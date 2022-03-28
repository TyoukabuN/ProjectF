using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public MoveState(Controller controllable,int stateKey = -1) : base(stateKey)
    {
        this.controller = controllable;
    }
    public override bool OnEnter(int stateKey = -1)
    {
        controller.SetAnimatorTrigger("Move");

        return true;
    }
    public override bool OnUpdate(float timeStep = 0)
    {
        if (controller == null)
            return false;

        var anyMove = controller.OnInputCheck_Move(Time.deltaTime);
        var anyJump = controller.OnInputCheck_Jump(Time.deltaTime);


        if (!controller.IsMoving())
        {
            stateMachine.Enter((int)PlayerController.StateType.Stand);
            return true;
        }
        if (anyJump && controller.IsGround())
        {
            stateMachine.Enter((int)PlayerController.StateType.Jump);
            return true;
        }

        return true;
    }
}
