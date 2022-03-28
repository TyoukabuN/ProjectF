using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class PlayerController : Controller//, IControllable
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

    private Vector3 originPosition = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();

        if (current == null)
            current = this;

        if (m_FSM == null)
        {
            m_FSM = new FSM();
            m_FSM.AddState((int)StateType.Stand, new StandState(this));
            m_FSM.AddState((int)StateType.Move, new MoveState(this));
            m_FSM.AddState((int)StateType.Jump, new JumpState(this));
            m_FSM.AddState((int)StateType.Fall, new FallState(this));
            m_FSM.OnInit((int)StateType.Stand);
        }

        originPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    //void FixedUpdate()
    {
        Update_CircleScanLine();
        Update_InputDetection();

        Move_Update();

        if (m_FSM != null)
            m_FSM.OnUpdate(Time.deltaTime);
    }

    public void ResetPosition()
    {
        transform.position = originPosition;
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
        var velocity = string.Format("<color=red>速度:{0}</color>", rigidbody.velocity.ToString());
        string str = state + "\n" + jump + "\n" + speed + "\n" + ground + "\n" + velocity;
        Handles.Label(transform.position, str, GizmosGUIStyle);
        Handles.color = Color.red;

        Gizmos.color = Color.red; 
        Gizmos.DrawWireSphere(transform.position + groundedOffset, groundedRadius);
    }
#endif



}
