using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public IControllable controllable;

    public MoveState(int stateKey = -1) : base(stateKey)
    { 
    }
    public override bool OnUpdate(float timeStep = 0)
    {
        if (controllable == null)
            return false;

        if (Input.GetKey(KeyCode.W))
        {
            controllable.MoveForward(this);
        }
        if (Input.GetKey(KeyCode.S))
        {
            controllable.MoveBackward(this);
        }
        if (Input.GetKey(KeyCode.A))
        {
            controllable.TureLeft(this);
        }
        if (Input.GetKey(KeyCode.D))
        {
            controllable.TureRight(this);
        }

        controllable.OnMotion(this);
        return true;
    }
}
