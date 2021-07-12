using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandState : State
{
    public IControllable controllable;
    public override bool OnUpdate(float timeStep = 0)
    {
        bool anyMove = false;

        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.D) )
        {
            anyMove = anyMove || true;
            stateMachine.Enter((int)PlayerController.StateType.Move);
        }
        return true;
    }
}
