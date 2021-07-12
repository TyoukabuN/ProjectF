using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public IControllable controllable;

    public MoveState(IControllable controllable,int stateKey = -1) : base(stateKey)
    {
        this.controllable = controllable;
    }
    public override bool OnUpdate(float timeStep = 0)
    {
        if (controllable == null)
            return false;

        bool anyMove = false;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            anyMove = anyMove || true;
            controllable.MoveForward(Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            anyMove = anyMove || true;
            controllable.MoveBackward(Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            anyMove = anyMove || true;
            controllable.TureLeft(Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            anyMove = anyMove || true;
            controllable.TureRight(Time.deltaTime);
        }

        controllable.OnMotion(Time.deltaTime);

        if (!anyMove)
        {
            stateMachine.Enter((int)PlayerController.StateType.Stand);
        }

        return true;
    }
}
