using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : Controller, IControllable
{
    public enum StateType:int
    { 
        Stand,
        Move,
        Jump,
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

    //Physice Properties
    public float Speed = 20;
    private Vector3 displacement = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 gravity = -Vector3.up * 10f;


    void Awake()
    {
        if (current == null)
        {
            current = this;
        }

        if (m_FSM == null)
        {
            m_FSM = new FSM();
            m_FSM.AddState((int)StateType.Stand, new StandState());
            m_FSM.AddState((int)StateType.Move, new MoveState(this));
            m_FSM.AddState((int)StateType.Jump, new JumpState(this));
            m_FSM.OnInit();
            m_FSM.Enter((int)StateType.Stand);
        }

        System.Reflection.MemberInfo info = typeof(InputDefine);
        object[] attributes = info.GetCustomAttributes(typeof(ObservedKey), false);
        for (int i = 0; i < attributes.Length; i++)
        {
            Debug.Log((KeyCode)attributes[i]);
        }

        foreach (var field in typeof(InputDefine).GetFields())
        {
            var attrs = field.GetCustomAttributes(typeof(ObservedKey), false);
            if (attrs.Length <= 0)
            {
                continue;
            }

            ObservedKey attr = attrs[0] as ObservedKey;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_FSM != null)
        {
            m_FSM.OnUpdate(Time.deltaTime);
        }

        Update_InputDetection();
    }
}
