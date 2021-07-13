using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class PlayerController : Controller, IControllable
{
    //Physice Properties
    public float Speed = 20;
    [Range(0f,1f)]
    public float StopSmoothParam = 0.3f;
    private Vector3 velocity = Vector3.zero;

    public Vector3 horizontalVelocity = Vector3.zero;
    public Vector3 verticalVelocity = Vector3.zero;

    public float maxHorizontalVelocity = 3;
    public float maxVerticalVelocity = 3;

    public Vector3 gravity = -Vector3.up * 10f;
    public void MoveForward(float timeStep = 0)
    {
        horizontalVelocity += Vector3.forward * Speed;
    }
    public void MoveBackward(float timeStep = 0)
    {
        horizontalVelocity -= Vector3.forward * Speed;
    }

    public void TurnLeft(float timeStep = 0)
    {
        horizontalVelocity -= Vector3.right * Speed;
    }

    public void TurnRight(float timeStep = 0)
    {
        horizontalVelocity += Vector3.right * Speed;
    }

    public void Motion(float timeStep = 0)
    {
        if (horizontalVelocity.magnitude > maxHorizontalVelocity)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxHorizontalVelocity;
        }
        if (verticalVelocity.magnitude > maxVerticalVelocity)
        {
            verticalVelocity = verticalVelocity.normalized * maxVerticalVelocity;
        }


        velocity = horizontalVelocity + verticalVelocity;
        if (rigidbody)
        {
            rigidbody.velocity = velocity;
        }

        velocity = Vector3.zero;

        //drag
        if (!anyMoveInput)
        { 
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, StopSmoothParam);
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

    private bool anyMoveInput = false;
    public bool IsMoving()
    {
        if (!anyMoveInput)
        {
            return (horizontalVelocity - Vector3.zero).magnitude > 0.8f;
        }

        return anyMoveInput;
    }


    public bool OnInputCheck_Move(float timeStep = 0)
    {
        anyMoveInput = false;

        if (IsKeyOn(InputDefine.Forward))
        {
            anyMoveInput = anyMoveInput || true;
            this.MoveForward(Time.deltaTime);
        }
        if (IsKeyOn(InputDefine.Backward))
        {
            anyMoveInput = anyMoveInput || true;
            this.MoveBackward(Time.deltaTime);
        }
        if (IsKeyOn(InputDefine.Left))
        {
            anyMoveInput = anyMoveInput || true;
            this.TurnLeft(Time.deltaTime);
        }
        if (IsKeyOn(InputDefine.Right))
        {
            anyMoveInput = anyMoveInput || true;
            this.TurnRight(Time.deltaTime);
        }
        return anyMoveInput;
    }

}
