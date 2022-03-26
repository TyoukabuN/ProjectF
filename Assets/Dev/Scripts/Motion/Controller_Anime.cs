using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Controller
{
    protected Animator animator;

    protected void Init_Animation()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }
    public void SetAnimatorTrigger(string name)
    {
        if (animator == null)
            return;

        animator.SetTrigger(name);
    }
}
