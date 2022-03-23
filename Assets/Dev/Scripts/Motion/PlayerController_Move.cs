using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public partial class PlayerController
{
    //Physice Properties
    public float Speed = 20;
    public float RuningSpeedMul = 1.8f;
    [SerializeField]private float SpeedMul = 1;
    public float FinalHorizontalSpeed
    {
        get { return Speed * SpeedMul; }
    }
    public float FinalVerticalSpeed
    {
        get { return JumpSpeed * SpeedMul; }
    }    
    [Range(0f,1f)] public float Damping = 0.3f;
    [Range(0f, 1f)] public float RotationDamping = 0.3f;
    private Vector3 velocity = Vector3.zero;

    public Vector3 forward = Vector3.zero;
    public Vector3 right = Vector3.zero;
    public Vector3 up = Vector3.zero;
    public float threshold = 0.01f;
    public Vector3 gravity = -Vector3.up * 10f;

    private Vector3 tempForward = Vector3.zero;
    private Vector3 tempRight = Vector3.zero;

    public bool useForce = true;
    public bool Log_Move = true;

    public float Mass
    { 
        set
        {
            rigidbody.mass = value;
        }
        get
        {
            if (!rigidbody)
                return 0f;
            return rigidbody.mass;
        }
    }

    void OnCameraCutEvent(CinemachineBrain brain)
    {

    }
    [SerializeField]private CinemachineVirtualCamera continuousVC;
    void OnCameraActivatedEvent(ICinemachineCamera now, ICinemachineCamera pre)
    {
        continuousVC = pre as CinemachineVirtualCamera;
    }

    public Vector3 GetForwardVector()
    {
        if (continuousVC != null)
        {
            tempForward.Set(continuousVC.VirtualCameraGameObject.transform.forward.x, 0, continuousVC.VirtualCameraGameObject.transform.forward.z);
            return tempForward.normalized;
        }
        if (followingCamera)
        {
            tempForward.Set(followingCamera.transform.forward.x, 0, followingCamera.transform.forward.z);
            return tempForward.normalized;
        }
        return Vector3.forward;
    }
    public Vector3 GetRightVector()
    {
        if (continuousVC != null)
        {
            tempForward.Set(continuousVC.VirtualCameraGameObject.transform.right.x, 0, continuousVC.VirtualCameraGameObject.transform.right.z);
            return tempForward.normalized;
        }
        if (followingCamera)
        {
            tempRight.Set(followingCamera.transform.right.x, 0, followingCamera.transform.right.z);
            return tempRight.normalized;
        }
        return Vector3.right;
    }
    public Vector3 GetUpVector()
    {
        return Vector3.up;
    }
    public void OnMoveForward(float timeStep = 0)
    {
        forward = GetForwardVector();
    }
    public void OnMoveBackward(float timeStep = 0)
    {
        forward = -1 * GetForwardVector();
    }
    public void OnMoveRight(float timeStep = 0)
    {
        right = GetRightVector();
    }
    public void OnMoveLeft(float timeStep = 0)
    {
        right = -1 * GetRightVector();
    }

    public void Move_Update()
    {
        Motion(Time.deltaTime);
    }

    Vector3 rdHorizontalVelocity = Vector3.zero;
    Vector3 rdVerticalVelocity = Vector3.zero;

    private float forwardStep = 1.0f;
    private float rightStep = 1.0f;
    private float upStep = 1.0f;
    public void Motion(float timeStep = 0)
    {
        GroundCheck();

        timeStep *= Damping;
        forwardStep = 1.0f;
        rightStep = 1.0f;
        upStep = 1.0f;

        rdHorizontalVelocity.Set(rigidbody.velocity.x, 0, rigidbody.velocity.z);
        if (rdHorizontalVelocity.magnitude > FinalHorizontalSpeed)
        {
            rdHorizontalVelocity = rdHorizontalVelocity.normalized * FinalHorizontalSpeed;
            //forwardStep = 0;
        }

        rdVerticalVelocity.Set(0, rigidbody.velocity.y, 0);
        if (rdVerticalVelocity.magnitude > FinalVerticalSpeed)
        {
            rdVerticalVelocity = rdVerticalVelocity.normalized * FinalVerticalSpeed;
            rightStep = 0;
        }

        //var direction = forward.normalized * forwardStep + right.normalized * rightStep + up;

        if (!IsGround())
        { 
            up += gravity * timeStep;
            if(up.y < 0)
                up = Vector3.ClampMagnitude(up, Math.Abs(gravity.y));
        }
        else
            up = Vector3.zero;

        //displacement
        //rigidbody.MovePosition(rigidbody.position + direction * FinalHorizontalSpeed * timeStep + direction * FinalVerticalSpeed * timeStep);
        //rigidbody.MovePosition(rigidbody.position + direction * Speed * timeStep);
        Vector3 displacement = (forward + right) * FinalHorizontalSpeed + up;
        rigidbody.MovePosition(rigidbody.position + displacement * timeStep);

        //rotation
        if (anyMoveInput)
        {
            //var forward = Vector3.Lerp(transform.forward, GetForwardVector(), RotationDragParam);
            var forward = Vector3.Lerp(transform.forward, GetForwardVector(), 1.0f - RotationDamping);
            forward.y = 0;
            transform.forward = forward.normalized;
        }
    }
    private void OnGrounding()
    {
        FSM.Enter((int)PlayerController.StateType.Stand);
    }

    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private float groundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    [SerializeField] private Vector3 groundedOffset = Vector3.zero;
    [SerializeField] private bool grounded;

    public bool IsGround()
    {
        return grounded;
    }

    public void GroundCheck()
    {
        grounded = Physics.CheckSphere(transform.position + groundedOffset, groundedRadius, m_WhatIsGround, QueryTriggerInteraction.Ignore);
    }

    private bool anyMoveInput = false;
    public bool IsMoving()
    {
        return anyMoveInput;
    }


    public float GetMoveSpeedMul()
    {
        return SpeedMul;
    }

    public bool OnInputCheck_Move(float timeStep = 0)
    {
        anyMoveInput = false;

        forward = Vector3.zero;
        right = Vector3.zero;
        if (GetKey(InputDefine.Forward))
        {
            anyMoveInput = anyMoveInput || true;
            this.OnMoveForward(Time.deltaTime);
        }
        if (GetKey(InputDefine.Backward))
        {
            anyMoveInput = anyMoveInput || true;
            this.OnMoveBackward(Time.deltaTime);
        }
        if (GetKey(InputDefine.Left))
        {
            anyMoveInput = anyMoveInput || true;
            this.OnMoveLeft(Time.deltaTime);
        }
        if (GetKey(InputDefine.Right))
        {
            anyMoveInput = anyMoveInput || true;
            this.OnMoveRight(Time.deltaTime);
        }


        if (SpeedMul <= 1.00001)
        {
            if (IsDoubleClick(InputDefine.Forward) ||
                IsDoubleClick(InputDefine.Backward) ||
                IsDoubleClick(InputDefine.Left) ||
                IsDoubleClick(InputDefine.Right))
            {
                SpeedMul = RuningSpeedMul;
            }
        }
        else if(!anyMoveInput)
        {
            SpeedMul = 1f;
        }

        return anyMoveInput;
    }

}
