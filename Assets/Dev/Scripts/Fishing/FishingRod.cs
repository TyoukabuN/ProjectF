using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float m_FishLineStep = 10f;

    public float fishLineStep
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

    }

}
