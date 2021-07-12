using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : State
{
    public Dictionary<int, State> stateDict = new Dictionary<int, State>();

    public State lastState;
    public State currentState;
    public int defaultStateKey = -1;

    public bool Enter(int stateKey)
    {
        State state;
        if (!stateDict.TryGetValue(stateKey, out state))
            return false;

        if (currentState != null && !currentState.OnExit(stateKey))
            return false;

        if (!state.OnCondition(currentState != null ? currentState.stateKey : -1))
            return false;

        lastState = currentState;
        currentState = state;
        //Debug.Log(((PlayerController.StateType)currentState.stateKey).ToString());
        state.OnEnter();

        return false;
    }
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
