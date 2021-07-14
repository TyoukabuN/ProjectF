﻿using System.Collections;
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
        Fall,
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
    private CapsuleCollider collider;


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
            m_FSM.AddState((int)StateType.Fall, new FallState(this));
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
        if (collider == null)
        {
            collider = GetComponent<CapsuleCollider>();
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
    private GUIStyle GizmosGUIStyle = null;
    void OnDrawGizmos()
    {
        if (m_FSM == null)
            return;

        if (GizmosGUIStyle == null)
        { 
            GUIStyle style = new GUIStyle();
            style.richText = true;
            GizmosGUIStyle = style;
        }
        var state = string.Format("<color=red>状态:{0}</color>", ((StateType)m_FSM.currentState.stateKey).ToString());
        var jump = string.Format("<color=red>跳跃:{0}/{1}</color>", JumpCounter.ToString(), CanJumpTime.ToString());
        var speed = string.Format("<color=red>速度:{0}</color>", FinalHorizontalSpeed.ToString());
        var ground = string.Format("<color=red>着地:{0}</color>", grounded.ToString());
        string str = state + "\n" + jump + "\n" + speed + "\n" + ground;
        Handles.Label(transform.position, str, GizmosGUIStyle);
        Handles.color = Color.red;

        Gizmos.color = Color.red; 
        Gizmos.DrawWireSphere(transform.position + k_GroundedOffset, k_GroundedRadius);
    }
#endif



}
