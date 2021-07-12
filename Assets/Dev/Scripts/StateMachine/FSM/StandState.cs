using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandState : State
{
    public IControllable controllable;
    public override bool OnUpdate(float timeStep = 0)
    {
        if (controllable != null)
        {
        }
        return true;
    }
}
