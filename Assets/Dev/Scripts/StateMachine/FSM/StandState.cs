using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandState : State
{
    public StandState(Controller controllable, int stateKey = -1) : base(stateKey)
    {
        this.controller = controllable;
    }
    public override bool OnEnter(int stateKey = -1)
    {
        controller.SetAnimatorTrigger("Stand");

        return true;
    }
    public override bool OnUpdate(float timeStep = 0)
    {
        var anyMove = controller.OnInputCheck_Move(Time.deltaTime);
        var anyJump = controller.OnInputCheck_Jump(Time.deltaTime);

        if (anyMove)
        {
            stateMachine.Enter((int)PlayerController.StateType.Move);
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
