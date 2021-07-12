using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public StateMachine stateMachine;
    public int stateKey = -1;
    public bool isInit = false;
    public State(int stateKey = -1)
    {
        this.stateKey = stateKey;
    }
    public virtual bool OnInit(StateMachine stateMachine, int stateKey = -1)
    {
        isInit = true;
        this.stateMachine = stateMachine;
        this.stateKey = stateKey;

        return true;
    }
    public virtual bool OnEnter()
    {
        return true;
    }
    public virtual bool OnUpdate(float timeStep = 0)
    {
        return true;
    }
    public virtual bool OnExit(int stateKey = -1)
    {
        return true;
    }
    public virtual bool OnCondition(int stateKey = -1)
    {
        return true;
    }
    public virtual bool OnDispose()
    {
        return true;
    }

}
