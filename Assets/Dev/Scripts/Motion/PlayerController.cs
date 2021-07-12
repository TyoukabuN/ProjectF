﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : Controller, IControllable
{
    public enum StateType:int
    { 
        Stand,
        Move,
    }

    protected FSM m_FSM;
    public FSM FSM
    {
        get {
            return m_FSM;
        }
        protected set {
            m_FSM = value;
        }
    }

    public static PlayerController current;
    // Start is called before the first frame update
    void Awake()
    {
        if (current == null)
        {
            current = this;
        }

        if (m_FSM == null)
        {
            m_FSM = new FSM();
            m_FSM.AddState((int)StateType.Move, new MoveState());
            m_FSM.AddState((int)StateType.Stand, new StandState());
            m_FSM.OnInit();
            m_FSM.Enter((int)StateType.Stand);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_FSM != null)
        {
            m_FSM.OnUpdate(Time.deltaTime);
        }
    }
}
