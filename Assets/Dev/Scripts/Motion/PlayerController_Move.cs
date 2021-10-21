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
    [Range(0f,1f)]
    public float Damping = 0.3f;
    [Range(0f, 1f)]
    public float RotationDamping = 0.3f;

    private Vector3 velocity = Vector3.zero;

    public Vector3 horizontalVelocity = Vector3.zero;
    public Vector3 verticalVelocity = Vector3.zero;
    public Vector3 upVelocity = Vector3.zero;

    public Vector3 gravity = -Vector3.up * 10f;

    private Vector3 tempForward = Vector3.zero;
    private Vector3 tempRight = Vector3.zero;

    public bool useForce = true;

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
    public void MoveForward(float timeStep = 0)
    {
        horizontalVelocity += GetForwardVector() * FinalHorizontalSpeed;
    }
    public void MoveBackward(float timeStep = 0)
    {
        horizontalVelocity -= GetForwardVector() * FinalHorizontalSpeed;
    }

    public void TurnLeft(float timeStep = 0)
    {
        horizontalVelocity -= GetRightVector() * FinalHorizontalSpeed;
    }

    public void TurnRight(float timeStep = 0)
    {
        horizontalVelocity += GetRightVector() * FinalHorizontalSpeed;
    }
    public void Jumping()
    {
        upVelocity += GetUpVector() * FinalVerticalSpeed;
    }

    Vector3 rdHorizontalVelocity = Vector3.zero;
    Vector3 rdVerticalVelocity = Vector3.zero;
    public void Motion(float timeStep = 0)
    {
        //Cinemachine.CinemachineBrain.
        //limit
        //if (!useForce)
        //{ 
        float forwardStep = 1.0f;
        float rightStep = 1.0f;

        rdHorizontalVelocity.Set(rigidbody.velocity.x, 0, rigidbody.velocity.z);
        if (rdHorizontalVelocity.magnitude > FinalHorizontalSpeed)
        {
            rdHorizontalVelocity = rdHorizontalVelocity.normalized * FinalHorizontalSpeed;
            forwardStep = 0;
        }

        rdVerticalVelocity.Set(0, rigidbody.velocity.y, 0);
        if (rdVerticalVelocity.magnitude > FinalVerticalSpeed)
        {
            rdVerticalVelocity = rdVerticalVelocity.normalized * FinalVerticalSpeed;
            rightStep = 0;
        }
        //}

        //displacement
        velocity = rdHorizontalVelocity + rdVerticalVelocity;
        if (rigidbody)
        {
            rigidbody.velocity = velocity;
            if (useForce)
            {
                rigidbody.AddForce(GetForwardVector() * FinalHorizontalSpeed * timeStep * forwardStep);
                rigidbody.AddForce(GetRightVector() * FinalHorizontalSpeed * timeStep * rightStep);
            }
            else
            { 
                rigidbody.MovePosition(rigidbody.position + velocity * timeStep);
            }

        }

        //velocity = Vector3.zero;


        //damp
        if (!anyMoveInput)
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, 1.0f - Damping);
            continuousVC = null;
        }
        //rotation
        if (anyMoveInput)
        {
            //var forward = Vector3.Lerp(transform.forward, GetForwardVector(), RotationDragParam);
            var forward = Vector3.Lerp(transform.forward, horizontalVelocity.normalized, 1.0f - RotationDamping);
            forward.y = 0;
            transform.forward = forward.normalized;
        }
        if (!IsGround())
        {
            verticalVelocity += gravity * timeStep;
        }
        else
        {
            verticalVelocity = Vector3.zero;
        }
    }

    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    [SerializeField] private Vector3 k_GroundedOffset = Vector3.zero;
    [SerializeField] private bool grounded;
    public bool IsGround()
    {
        if (true)
        { 
            return grounded;
        }
        bool wasGrounded = grounded;
        grounded = false;

        Collider[] colliders = Physics.OverlapSphere(transform.position + k_GroundedOffset, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                grounded = true;
                if (!wasGrounded)
                {
                    //OnLandEvent.Invoke();
                }
            }
        }

        return grounded;
    }

    bool cancellingGrounded;
    float maxSlopeAngle = 35f;
    Vector3 normalVector = Vector3.zero;
    private bool IsFloor(Vector3 colContactNormal)
    {
        float angle = Vector3.Angle(Vector3.up, colContactNormal);
        return angle < maxSlopeAngle;
    }

    void OnCollisionStay(Collision other)
    {
        //if (theCollision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //{
        //    grounded = true;
        //}
        int layer = other.gameObject.layer;

        if (m_WhatIsGround != (m_WhatIsGround | (1 << layer))) return;

        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            if (IsFloor(normal))
            {
                if (!grounded)
                    OnGrounding();

                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
                break;
            }
        }

        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void OnGrounding()
    {
        FSM.Enter((int)PlayerController.StateType.Move);
    }

    private void StopGrounded()
    {
        grounded = false;
    }


    private bool anyMoveInput = false;
    public bool IsMoving()
    {
        if (!anyMoveInput)
        {
            return (horizontalVelocity - Vector3.zero).magnitude > 0.8f;
        }

        return anyMoveInput;
    }


    public float GetMoveSpeedMul()
    {
        return SpeedMul;
    }

    public bool OnInputCheck_Move(float timeStep = 0)
    {
        anyMoveInput = false;


        if (GetKey(InputDefine.Forward))
        {
            anyMoveInput = anyMoveInput || true;
            this.MoveForward(Time.deltaTime);
        }
        if (GetKey(InputDefine.Backward))
        {
            anyMoveInput = anyMoveInput || true;
            this.MoveBackward(Time.deltaTime);
        }
        if (GetKey(InputDefine.Left))
        {
            anyMoveInput = anyMoveInput || true;
            this.TurnLeft(Time.deltaTime);
        }
        if (GetKey(InputDefine.Right))
        {
            anyMoveInput = anyMoveInput || true;
            this.TurnRight(Time.deltaTime);
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
