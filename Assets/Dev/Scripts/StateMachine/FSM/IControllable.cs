using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable
{
    void OnMoveForward(float timeStep = 0);
    void OnMoveBackward(float timeStep = 0);
    void OnMoveLeft(float timeStep = 0);
    void OnMoveRight(float timeStep = 0);
    void Jump(float timeStep = 0);
    bool CanJump();
    int LeftJumpTime();
    bool RecoverJumpTime();
    void Motion(float timeStep = 0);
    bool OnInputCheck_Move(float timeStep = 0);
    bool OnInputCheck_Jump(float timeStep = 0);
    bool IsMoving();
    float GetMoveSpeedMul();
    bool IsGround();
    void SetAnimatorTrigger(string name);
    Vector3 GetForwardVector();
    Vector3 GetRightVector();
}
