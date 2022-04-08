using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMath;
using Bezier = TMath.Bezier;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BezierCurve : MonoBehaviour
{
    public List<Transform> points = new List<Transform>();
    public int step = 50;
    public void OnEnable()
    {

    }

    private List<Vector3> temp = new List<Vector3>();

    public List<Vector3> DrawvCurve()
    {
        temp.Clear();
        if (points.Count == 3)
        {
            for (float i = 0; i <= step; i++)
            {
                float t = i / step;
                temp.Add(Bezier.Quadratic(points[0].position, points[1].position, points[2].position, t));
            }
        }
        else if (points.Count == 4)
        {
            for (float i = 0; i <= step; i++)
            {
                float t = i / step;
                temp.Add(Bezier.Cubic(points[0].position, points[1].position, points[2].position, points[3].position, t));
            }
        }

        for (int i = 1; i < temp.Count; i++)
        {
            var p0 = temp[i - 1];
            var p1 = temp[i];
            Debug.DrawLine(p0, p1);
            Debug.DrawLine(p0, p1, Color.yellow, Time.deltaTime);
        }
        return temp;
    }

    public Transform Target;
    Vector3 lastPoint = Vector3.zero;
    public MotionInfo info;
    public void Update()
    {
        DrawvCurve();

        Motion_ApplyMotion(Target.transform, info);
    }


    public class MotionInfo
    {
        public float duration;
        public AnimationCurve curveX;
        public AnimationCurve curveY;
        public AnimationCurve curveZ;
        //
        float counter = 0;
        Vector3 lastPoint = Vector3.zero;

        public void Restart()
        {
            counter = 0;
        }
        public Vector3 Evaluate(float deltaTime)
        {
            counter += deltaTime;
            counter = counter >= duration ? duration : counter;
            return new Vector3(curveX.Evaluate(counter), curveY.Evaluate(counter), curveZ.Evaluate(counter));
        }

        public void ApplyCurve(Transform transform, float deltaTime)
        {
            if (!transform)
                return;
            var point = Evaluate(deltaTime);
            transform.position = Evaluate(deltaTime);
            var forward = point - lastPoint;
            transform.forward = forward.normalized;
            lastPoint = point;
        }

        public bool IsDone()
        {
            return counter >= duration;
        }
    }

    public List<Vector3> Motion_CubicBezierSamples(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,int step = 50)
    {
        List<Vector3> samples = new List<Vector3>();
        for (float i = 0; i <= step; i++)
        {
            float t = i / step;
            samples.Add(Bezier.Cubic(p0, p1, p2, p3, t));
        }
        for (int i = 1; i < samples.Count; i++)
        {
            var _p0 = samples[i - 1];
            var _p1 = samples[i];
            Debug.DrawLine(_p0, _p1, Color.yellow, 2.0f);
        }
        return samples;
    }

    public MotionInfo Motion_BezierSamplesToAnimaCurves(List<Vector3> samples, float duration)
    {
        MotionInfo info = new MotionInfo();
        info.duration = duration;
        info.curveX = new AnimationCurve();
        info.curveY = new AnimationCurve();
        info.curveZ = new AnimationCurve();

        for (int i = 0; i < samples.Count; i++)
        {
            var time = duration * i / samples.Count;
            var value = samples[i];
            info.curveX.AddKey(time, value.x);
            info.curveY.AddKey(time, value.y);
            info.curveZ.AddKey(time, value.z);
        }
        return info;
    }

    public Vector4 Vec3ToVec4(Vector3 vec3, float w = 0)
    {
        return new Vector4(vec3.x, vec3.y, vec3.z, w);
    }
    public MotionInfo Motion_CubicCurveMovement(Vector3 origin, Vector3 target, Vector3 cOriginControlLocalPos, Vector3 cTargetControlLocalPos,float duration, int step = 50)
    {
        var forward = (target - origin).normalized;
        var right = Vector3.Cross(forward,Vector3.up).normalized;
        var up = Vector3.Cross(right,forward).normalized;

        var mat = new Matrix4x4();
        mat.SetColumn(0, right);
        mat.SetColumn(1, up);
        mat.SetColumn(2, forward);
        mat.SetColumn(3, Vec3ToVec4(origin,1));

        Vector3 cOriginControl = mat * Vec3ToVec4(cOriginControlLocalPos,1);

        mat.SetColumn(3, Vec3ToVec4(target, 1));

        Vector3 cTargetControl = mat * Vec3ToVec4(cTargetControlLocalPos, 1);
#if UNITY_EDITOR
        Debug.Log(cOriginControlLocalPos);
        Debug.Log(cTargetControlLocalPos);

        Debug.DrawLine(cOriginControl, cOriginControl + Vector3.up, Color.yellow, 2.0f);
        Debug.DrawLine(cTargetControl, cTargetControl + Vector3.up, Color.yellow, 2.0f);
#endif
        var sampler = Motion_CubicBezierSamples(origin, cOriginControl, cTargetControl, target, step);
        var motionInfo = Motion_BezierSamplesToAnimaCurves(sampler, duration);
        return motionInfo;
    }
    public void Motion_ApplyMotion(Transform transform, MotionInfo info)
    {
        if (info == null || info.IsDone())
            return;

        info.ApplyCurve(transform, Time.deltaTime);
    }

    public Transform originP;
    public Transform targetP;
    public float duration = 2.0f;
}


#if UNITY_EDITOR
[CustomEditor(typeof(BezierCurve))]
class BezierCurveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var handle = target as BezierCurve;
        if (handle == null)
            return;

        GUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            if (GUILayout.Button("绘制曲线", GUILayout.Width(80)))
            {
                handle.DrawvCurve();
                Debug.DrawLine(Vector3.zero, Vector3.one, Color.yellow,2.0f);
            }

            if (GUILayout.Button("测试", GUILayout.Width(80)))
            {
                handle.info = handle.Motion_CubicCurveMovement(
                    handle.originP.position,
                    handle.targetP.position,
                    handle.points[2].position - handle.points[3].position,
                    handle.points[1].position - handle.points[0].position, 
                    handle.duration
                );
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
