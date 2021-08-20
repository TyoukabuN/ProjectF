using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
