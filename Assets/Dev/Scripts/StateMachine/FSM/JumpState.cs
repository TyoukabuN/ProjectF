using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateType = PlayerController.StateType;
public class JumpState : State
{
    public JumpState(Controller controllable, int stateKey = -1) : base(stateKey)
    {
        this.controller = controllable;
    }
    public override bool OnEnter(int stateKey = -1)
    {
         controller.Jump(Time.deltaTime);
        return true;
    }
    public override bool OnUpdate(float timeStep = 0)
    {
        if (controller == null)
            return false;

        var anyMove = controller.OnInputCheck_Move(Time.deltaTime);
        var anyJump = controller.OnInputCheck_Jump(Time.deltaTime);

        controller.SetAnimatorTrigger(anyMove?"Move":"Stand");

        if (anyJump && controller.CanJump())
        {
            stateMachine.Enter((int)PlayerController.StateType.Jump);
        }
        if (controller.IsGround())
        {
            stateMachine.Enter((int)PlayerController.StateType.Move);
        }
        //else(controllable.GetForwardVector)
        //{
        //    stateMachine.Enter((int)PlayerController.StateType.Fall);
        //}

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
