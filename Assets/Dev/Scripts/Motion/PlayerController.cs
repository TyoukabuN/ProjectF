using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    private CharacterController characterController;
    private new Rigidbody rigidbody;


    void Awake()
    {
        if (current == null)
        {
            current = this;
        }

        if (m_FSM == null)
        {
            m_FSM = new FSM();
            m_FSM.AddState((int)StateType.Stand, new StandState(this));
            m_FSM.AddState((int)StateType.Move, new MoveState(this));
            m_FSM.AddState((int)StateType.Jump, new JumpState(this));
            m_FSM.OnInit();
            m_FSM.Enter((int)StateType.Stand);
        }

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        InitObservedKey();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Update_InputDetection();

        if (m_FSM != null)
        {
            m_FSM.OnUpdate(Time.deltaTime);
        }


    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (m_FSM == null)
            return;

        GUIStyle style = new GUIStyle();
        style.richText = true;
        var state = string.Format("<color=red>{0}</color>", ((StateType)m_FSM.currentState.stateKey).ToString());
        Handles.Label(transform.position, state, style);
        Handles.color = Color.red;

        Gizmos.DrawWireSphere(transform.position + k_GroundedOffset, k_GroundedRadius);
    }
#endif



}
