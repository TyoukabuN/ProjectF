using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BezierCurveSimulater : MonoBehaviour
{
    public List<MotionInfo> motions = new List<MotionInfo>();

    // Update is called once per frame
    void Update()
    {
        foreach (var motion in motions)
        {
            if (motion == null || motion.IsDone() || !motion.IsVaild())
                continue;
            motion.ApplyCurve(Time.deltaTime);
        }
        //clear
        for (int i = 0; i < motions.Count; i++)
        {
            var motion = motions[i];
            if (motion == null || motion.IsDone() || !motion.IsVaild())
            { 
                motions.RemoveAt(i);
                i--;
            }
        }
    }
    public void Clear()
    {
        if (motions == null)
            return;

        foreach (var motion in motions)
        {
            if (motion == null || motion.IsDone() || !motion.IsVaild())
                continue;
            motion.OnDone();
        }

        motions.Clear();
    }
    public bool IsExitMotion(string key)
    {
        return motions.Any((item) => item!=null && item.key == key);
    }    
    public bool AddExitMotion(MotionInfo info)
    {
        if (info == null || IsExitMotion(info.key))
            return false;

        motions.Add(info);
        return true;
    }
}
public interface IMotionable {
    Transform transform { get;}
    void OnMotionBegin();
    void OnMotionDone();
}
public class MotionInfo
{
    public string key;
    public float duration;
    public AnimationCurve curveX;
    public AnimationCurve curveY;
    public AnimationCurve curveZ;
    public IMotionable motionable;
    public bool ignoreDoneCall = false;
    //
    float counter = 0;
    Vector3 lastPoint = Vector3.zero;
    bool began = false;
    bool done = false;

    public void Restart()
    {
        began = false;
        counter = 0;
    }
    public Vector3 Evaluate(float deltaTime)
    {
        OnBegin();
        counter += deltaTime;
        counter = counter >= duration ? duration : counter;
        return new Vector3(curveX.Evaluate(counter), curveY.Evaluate(counter), curveZ.Evaluate(counter));
    }

    public void ApplyCurve(float deltaTime)
    {
        ApplyCurve(motionable.transform, deltaTime);
    }

    public void ApplyCurve(Transform transform, float deltaTime)
    {
        if (transform == null)
            return;
        var point = Evaluate(deltaTime);
        transform.position = point;
        if (point != lastPoint)
        { 
            var forward = point - lastPoint;
            transform.forward = forward.normalized;
        }
        lastPoint = point;
        if (IsDone()) OnDone();
    }
    public bool IsVaild()
    {
        return motionable != null && 
            motionable.transform != null &&
            curveX != null && 
            curveY != null && 
            curveZ != null;
    }
    public bool IsDone()
    {
        return done || counter >= duration;
    }

    public void OnBegin()
    {
        if (began && done)
            return;
        if (motionable != null)
            motionable.OnMotionBegin();
        began = true;
    }
    public void OnDone()
    {
        if (done)
            return;
        if (motionable != null && !ignoreDoneCall)
            motionable.OnMotionDone();
        done = true;
    }
}
