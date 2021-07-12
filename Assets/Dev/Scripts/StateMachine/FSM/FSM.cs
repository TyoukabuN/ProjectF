using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : StateMachine
{
    public bool OnInit(int stateKey = -1)
    {
        return OnInit(null, stateKey);
    }
    public override bool OnInit(StateMachine stateMachine, int stateKey = -1)
    {
        if (isInit)
            return false;

        base.OnInit(stateMachine, stateKey);

        if (currentState == null)
        {
            //if (stateDict.TryGetValue(defaultStateKey, out currentState))
            //{
            //    return currentState.OnInit(this);
            //}
            return Enter(defaultStateKey);
        }
        return currentState.OnInit(this);
    }
    public bool AddState(int key, State state, bool isCoverExist = false)
    {
        State exist;
        if (stateDict.TryGetValue(key, out exist))
        {
            if (isCoverExist)
            {
                exist.OnDispose();
            }
            else
            { 
                return false;
            }
        }

        stateDict[key] = state;

        return state.OnInit(this, key);
    }

    public bool Enter(int stateKey)
    {
        State state;
        if (!stateDict.TryGetValue(stateKey, out state))
            return false;

        if (currentState != null &&!currentState.OnExit(stateKey))
            return false;

        if (!state.OnCondition(currentState!=null? currentState.stateKey:-1))
            return false;

        lastState = currentState;
        currentState = state;

        state.OnEnter();

        return false;
    }
}
