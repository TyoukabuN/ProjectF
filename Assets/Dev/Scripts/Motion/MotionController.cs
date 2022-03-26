using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Controller : MonoBehaviour
{
    protected virtual void Awake()
    {
        Init_Move();
        Init_Animation();
    }
}
