using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable
{
    void MoveForward(float timeStep = 0);
    void MoveBackward(float timeStep = 0);
    void TurnLeft(float timeStep = 0);
    void TurnRight(float timeStep = 0);
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
}
