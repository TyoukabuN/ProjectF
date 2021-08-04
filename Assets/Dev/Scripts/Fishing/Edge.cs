using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Edge
{
    public Point[] points = new Point[2];
    public float length = 1;
    public float normalizeLength = 1;
    /// <summary>
    /// length multiple with normalizeLength
    /// </summary>
    public float Length
    {
        get { return length * normalizeLength; }
    }

    private float tinyValue = 0.02f;

    public Edge(Point p1, Point p2)
    {
        points[0] = p1;
        points[1] = p2;
    }

    public Edge(Point p1, Point p2,float originLength):this(p1,p2)
    {
        this.length = originLength;
    }

    public bool Vaild()
    {
        return points[0]!=null && points[1] != null;
    }

    public void Tick()
    {
        if (points[0] == null)
            return;

        if (points[1] == null)
            return;

        var tlength = Mathf.Max(Length, tinyValue);

        var p1 = points[0];
        var p2 = points[1];

        //p1.Tick(Time.deltaTime);
        //p2.Tick(Time.deltaTime);

        var p1p2 = p2.transform.position - p1.transform.position;

        if (p1p2.magnitude <= tinyValue)
        {
            p1p2 = Vector3.down * tinyValue;
        }
        //var diff = Mathf.Abs(p1p2.magnitude - originLength);
        var diff = (p1p2.magnitude - tlength);

        if (!p2.simulate)
        {
            p1.transform.position += p1p2.normalized * diff * 1.0f;
        }
        else if (!p1.simulate)
        {
            p2.transform.position -= p1p2.normalized * diff * 1.0f;
        }
        else
        {
            p1.transform.position += p1p2.normalized * diff * 0.5f;
            p2.transform.position -= p1p2.normalized * diff * 0.5f;
        }
    }

}

