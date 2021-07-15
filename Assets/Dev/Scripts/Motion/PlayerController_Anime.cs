using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : Controller, IControllable
{
    public void SetAnimatorTrigger(string name)
    {
        if (animator == null)
            return;

        animator.SetTrigger(name);
    }
}
