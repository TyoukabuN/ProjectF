using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public virtual bool OnInit()
    {
        return false;
    }
    public virtual bool OnEnter()
    {
        return false;
    }
    public virtual bool OnExit()
    {
        return false;
    }
    public virtual bool OnCondition()
    {
        return true;
    }
    public virtual bool OnDispose()
    {
        return false;
    }
}
