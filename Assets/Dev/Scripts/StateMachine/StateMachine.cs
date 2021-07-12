using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : State
{
    public Dictionary<int, State> stateDict = new Dictionary<int, State>();

    public State current;
    public int defaultStateKey = -1;

    public override bool OnInit()
    {
        if (current == null)
        {
            if (stateDict.TryGetValue(defaultStateKey, out current))
            {
                return current.OnInit();
            }
            return base.OnInit();
        }
        return current.OnInit();
    }
    public override bool OnEnter()
    {
        if (current == null)
        {
            return base.OnEnter();
        }
        return current.OnEnter();
    }
    public override bool OnExit()
    {
        if (current == null)
        {
            return base.OnExit();
        }
        return current.OnExit();
    }
    public override bool OnCondition()
    {
        if (current == null)
        {
            return base.OnCondition();
        }
        return current.OnCondition();
    }
    public override bool OnDispose()
    {
        if (current == null)
        {
            return base.OnDispose();
        }
        return current.OnDispose();
    }
}
