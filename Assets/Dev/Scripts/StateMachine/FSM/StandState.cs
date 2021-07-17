using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandState : State
{
    public StandState(IControllable controllable, int stateKey = -1) : base(stateKey)
    {
        this.controllable = controllable;
    }
    public override bool OnEnter(int stateKey = -1)
    {
        controllable.SetAnimatorTrigger("Stand");

        return true;
    }
    public override bool OnUpdate(float timeStep = 0)
    {
        var anyMove = controllable.OnInputCheck_Move(Time.deltaTime);
        var anyJump = controllable.OnInputCheck_Jump(Time.deltaTime);

       controllable.Motion(Time.deltaTime);

        if (anyMove)
        {
            stateMachine.Enter((int)PlayerController.StateType.Move);
        }
        if (anyJump)
        {
            stateMachine.Enter((int)PlayerController.StateType.Jump);
        }
        return true;
    }
}
