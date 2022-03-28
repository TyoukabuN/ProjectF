using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using System.Linq;

public partial class Controller
{
    //Physice Properties
    public float Speed = 20;
    public float RuningSpeedMul = 1.8f;
    [SerializeField]protected float SpeedMul = 1;
    public float FinalHorizontalSpeed
    {
        get { return Speed * SpeedMul + AdditionSpeed; }
    }
    //addition speed
    public float AdditionSpeed = 0;
    public class AdditionSpeedInfo
    {
        public string key = string.Empty;
        public float value = 0;
        public float duration = 0;
        public float timeStamp = 0;
        public float counter = 0;
        public AdditionSpeedInfo(string key,float value,float duration)
        {
            this.key = key;
            this.value = value;
            this.duration = duration;
            this.timeStamp = Time.time;
        }
        public bool IsVaild()
        {
            return duration > Time.time - timeStamp;
        }
        public void ResetTimeStamp()
        {
            timeStamp = Time.time;
        }
    }
    public Dictionary<string, AdditionSpeedInfo> AdditionSpeedMap = new Dictionary<string, AdditionSpeedInfo>();
    public List<AdditionSpeedInfo> AdditionSpeedList = new List<AdditionSpeedInfo>();
    public void RefreshAdditionSpeed()
    {
        float temp = 0;
        for (int i=0; i < AdditionSpeedList.Count; i++)
        {
            AdditionSpeedInfo info = null;
            try
            {
                info = AdditionSpeedList[i];
            }
            catch(Exception e)
            {
                continue;
            }
            if (info == null)
                continue;

            if (!info.IsVaild())
            {
                AdditionSpeedList.RemoveAt(i);
                AdditionSpeedMap.Remove(info.key);
                continue;
            }
            temp += info.value;
        }
        temp = temp < 0 ? 0 : temp;
        AdditionSpeed = temp;
    }
    public void InsertAdditionSpeed(string key,float value, float duration)
    {
        AdditionSpeedInfo info = null;
        if (AdditionSpeedMap.TryGetValue(key, out info))
        {
            info.value = value;
            info.duration = duration;
            info.ResetTimeStamp();
            return;
        }
        AdditionSpeed += value;
        AdditionSpeed = AdditionSpeed < 0 ? 0 : AdditionSpeed;
        info = new AdditionSpeedInfo(key,value, duration);
        AdditionSpeedMap.Add(key, info);
        AdditionSpeedList.Add(info);
    }
    public void RemoveAdditionSpeed(string key)
    {
        if (!AdditionSpeedMap.ContainsKey(key))
            return;
        AdditionSpeed -= AdditionSpeedMap[key].value;
        AdditionSpeed = AdditionSpeed < 0 ? 0 : AdditionSpeed;
        AdditionSpeedInfo info = null;
        if (AdditionSpeedMap.TryGetValue(key, out info))
        {
            AdditionSpeedMap.Remove(key);
            AdditionSpeedList.Remove(info);
        }
    }
    public void UpdateAdditonSpeedTimeStamp()
    {
        foreach (var pair in AdditionSpeedMap)
        {
            if (pair.Value == null) continue;
            pair.Value.counter += Time.deltaTime;
        }
        RefreshAdditionSpeed();
    }
    //
    public float FinalVerticalSpeed
    {
        get { return JumpSpeed * SpeedMul; }
    }    
    [Range(0f,1f)] public float Damping = 0.3f;
    [Range(0f, 1f)] public float RotationDamping = 0.3f;
    protected Vector3 velocity = Vector3.zero;

    public Vector3 forward = Vector3.zero;
    public Vector3 right = Vector3.zero;
    public Vector3 up = Vector3.zero;
    public float threshold = 0.01f;
    public Vector3 gravity = -Vector3.up * 10f;

    protected Vector3 tempForward = Vector3.zero;
    protected Vector3 tempRight = Vector3.zero;

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

    protected CinemachineBrain cinemachineBrain;
    [SerializeField] protected Camera followingCamera;

    protected new CapsuleCollider collider;
    protected new Rigidbody rigidbody;

    protected void Init_Move()
    {
        if (rigidbody == null) rigidbody = GetComponent<Rigidbody>();
        if (collider == null) collider = GetComponent<CapsuleCollider>();

        if (cinemachineBrain == null && Camera.main)
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain)
        {
            cinemachineBrain.m_CameraCutEvent.RemoveListener(OnCameraCutEvent);
            cinemachineBrain.m_CameraCutEvent.AddListener(OnCameraCutEvent);
            cinemachineBrain.m_CameraActivatedEvent.RemoveListener(OnCameraActivatedEvent);
            cinemachineBrain.m_CameraActivatedEvent.AddListener(OnCameraActivatedEvent);
        }

        m_WhatIsGround = LayerMask.GetMask("Ground");
        InitObservedKey();
    }

    void OnCameraCutEvent(CinemachineBrain brain)
    {

    }
    [SerializeField]protected CinemachineVirtualCamera continuousVC;
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

    protected float forwardStep = 1.0f;
    protected float rightStep = 1.0f;
    protected float upStep = 1.0f;
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

        if (!IsGround() || lastGroundDistance > 0)
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

    [SerializeField] protected LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] protected float groundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    [SerializeField] protected Vector3 groundedOffset = Vector3.zero;
    [SerializeField] protected bool grounded;
    [SerializeField] protected float lastGroundDistance = 0.0f;
    [SerializeField] protected Vector3 lastGroundHitPoint;

    [SerializeField] public float lastJumpGroundDistance = 0.5f;

    public bool CanRecoverJumpCounter()
    {
        return lastGroundDistance <= lastJumpGroundDistance;
    }
    public bool IsGround()
    {
        return grounded;
    }

    public void GroundCheck()
    {
        grounded = Physics.CheckSphere(transform.position + groundedOffset, groundedRadius, m_WhatIsGround, QueryTriggerInteraction.Ignore);

        RaycastHit hit;
        if (Physics.Raycast(transform.position + groundedOffset, Vector3.down, out hit, 100))
        { 
            if(transform.position.y - hit.point.y < 0)
                lastGroundDistance = 0;
            else
                lastGroundDistance = (transform.position - hit.point).magnitude;

            lastGroundHitPoint = hit.point;
        }

        lastGroundDistance = lastGroundDistance > 0 ? lastGroundDistance : 0;
    }

    protected bool anyMoveInput = false;
    public bool IsMoving()
    {
        return anyMoveInput;
    }


    public float GetMoveSpeedMul()
    {
        return SpeedMul;
    }
    public float dashDuration = 1.0f;
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

        UpdateAdditonSpeedTimeStamp();

        if (GetKeyDown(InputDefine.Shift))
        {
            //SpeedMul = RuningSpeedMul;
            InsertAdditionSpeed("dash", 20, dashDuration);
        }

        return anyMoveInput;
    }

}
