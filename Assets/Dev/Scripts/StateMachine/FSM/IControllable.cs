using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable
{
    void MoveForward(State state);
    void MoveBackward(State state);
    void TureLeft(State state);
    void TureRight(State state);
    void OnMotion(State state);
}
