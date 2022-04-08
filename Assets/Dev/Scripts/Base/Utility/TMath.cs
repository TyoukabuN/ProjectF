using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TMath
{ 
    public static class MathUtility
    {
        public const float PI = 3.14159274F;
        public const float Infinity = float.PositiveInfinity;
        public const float NegativeInfinity = float.NegativeInfinity;
        public const float Deg2Rad = 0.0174532924F;
        public const float Rad2Deg = 57.29578F;
        public static Vector3 RodriguesRot(Vector3 vec3, float degree, Vector3 axi, bool doNormalize = false)
        {
            var rad = Deg2Rad * degree;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            var res = cos * vec3 + (1 - cos) * axi * Vector3.Dot(axi, vec3) + sin * Vector3.Cross(axi, vec3);

            if (doNormalize)
                return vec3;

            return vec3.normalized;
        }

        public static float frac(float IN)
        {
            return IN - Mathf.Floor(IN);
        }

        public static Vector3 mul(this Vector3 vec3, Vector3 multiple)
        {
            vec3.x *= multiple.x;
            vec3.y *= multiple.y;
            vec3.z *= multiple.z;

            return vec3;
        }
    }

    public static class Bezier
    {
        public static Vector3 Linear(Vector3 p0, Vector3 p1, float t)
        {
            return (1.0f - t) * p0 + t * p1;
        }

        public static Vector3 Quadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return Linear(Linear(p0, p1, t), Linear(p1, p2, t), t);
        }

        public static Vector3 Cubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return Linear(Quadratic(p0, p1, p2, t), Quadratic(p1, p2, p3, t), t);
        }
        public static Vector3 NOrder(List<Vector3> p, float t)
        {
            if (p.Count < 2)
                return p[0];
            List<Vector3> temp = new List<Vector3>();
            for (int i = 0; i < p.Count - 1; i++)
            {
                Vector3 p0p1 = (1 - t) * p[i] + t * p[i + 1];
                temp.Add(Linear(p[i], p[i + 1], t));
            }
            return NOrder(temp, t);
        }
    }

}
