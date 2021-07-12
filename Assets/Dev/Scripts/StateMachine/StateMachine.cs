using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : State
{
    public Dictionary<int, State> stateDict = new Dictionary<int, State>();

    public State lastState;
    public State currentState;
    public int defaultStateKey = -1;


    public override bool OnEnter()
    {
        if (currentState == null)
        {
            return base.OnEnter();
        }
        return currentState.OnEnter();
    }
    public override bool OnUpdate(float timeStep = 0)
    {
        if (currentState == null)
        {
            return base.OnUpdate(timeStep);
        }
        return currentState.OnUpdate(timeStep);
    }

    public override bool OnDispose()
    {
        if (currentState == null)
        {
            return base.OnDispose();
        }
        return currentState.OnDispose();
    }
}
