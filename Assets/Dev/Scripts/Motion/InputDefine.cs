using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ObservedKey]
public static class InputDefine
{
    [ObservedKey]
    public readonly static KeyCode Forward =    KeyCode.UpArrow;
    [ObservedKey]
    public readonly static KeyCode Backward =   KeyCode.DownArrow;
    [ObservedKey]
    public readonly static KeyCode Left =       KeyCode.LeftArrow;
    [ObservedKey]
    public readonly static KeyCode Right =      KeyCode.RightArrow;
    [ObservedKey]
    public readonly static KeyCode Jump =       KeyCode.Z;
}
