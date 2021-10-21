using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public MoveState(IControllable controllable,int stateKey = -1) : base(stateKey)
    {
        this.controllable = controllable;
    }
    public override bool OnEnter(int stateKey = -1)
    {
        controllable.SetAnimatorTrigger("Move");

        return true;
    }
    public override bool OnUpdate(float timeStep = 0)
    {
        if (controllable == null)
            return false;

        var anyMove = controllable.OnInputCheck_Move(Time.deltaTime);
        var anyJump = controllable.OnInputCheck_Jump(Time.deltaTime);


        if (!controllable.IsMoving())
        {
            stateMachine.Enter((int)PlayerController.StateType.Stand);
        }
        if (anyJump)
        {
            stateMachine.Enter((int)PlayerController.StateType.Jump);
        }

        return true;
    }
}
