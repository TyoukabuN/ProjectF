using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable
{
    void MoveForward(float timeStep = 0);
    void MoveBackward(float timeStep = 0);
    void TureLeft(float timeStep = 0);
    void TureRight(float timeStep = 0);
    void OnMotion(float timeStep = 0);
}
