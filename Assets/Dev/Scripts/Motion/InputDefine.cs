using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputDefine
{
    [ObservedKey] public readonly static KeyCode Forward =    KeyCode.W;
    [ObservedKey] public readonly static KeyCode Backward =   KeyCode.S;
    [ObservedKey] public readonly static KeyCode Left =       KeyCode.A;
    [ObservedKey] public readonly static KeyCode Right =      KeyCode.D;
    [ObservedKey] public readonly static KeyCode Jump =       KeyCode.Space;
    [ObservedKey] public readonly static KeyCode Shift =      KeyCode.LeftShift;
}
