using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CollisionDetection))]
public class FishingRod : MonoBehaviour
{
    /// <summary>
    /// 鱼线有多长
    /// </summary>
    public float fishLineLength = 10.0f;
    /// <summary>
    /// 绳子有多少节
    /// </summary>
    public int m_FishLineStep = 10;

    public GameObject lineRoot;

    public int fishLineStep
    {
        get
        {
            return m_FishLineStep;
        }

        set
        {
            if (value == m_FishLineStep)
                return;

            UpdateLineStep();
        }
    }

    private CollisionDetection collisiionDetection;

    private void Awake()
    {
        SelfCheck();
    }

    public bool SelfCheck()
    {
        if (!collisiionDetection)
            collisiionDetection = GetComponent<CollisionDetection>();

        if (!collisiionDetection)
            return false;
        return true;
    }

    public void UpdateLineStep()
    {
        if (!SelfCheck())
            return;

        if (collisiionDetection.stepCount == m_FishLineStep)
            return;

        float diff = Mathf.Abs(collisiionDetection.stepCount - m_FishLineStep);
        Func<bool> func = diff < 0 ? RemoveStep : AddStep;

        for (int i = 0; i < diff; i++)
        {
            func();
        }
    }

    public bool AddStep()
    {
        return true;
    }

    public bool RemoveStep()
    {
        return true;
    }

}
