using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Edge
{
    public Point[] points = new Point[2];
    public float length = 1;
    private float m_normalizeLength = 1f;
    public float normalizeLength = 1f;
    public float dumping = 1f;
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

        var tlength = Mathf.Max(Length, 0.001f);

        var p1 = points[0];
        var p2 = points[1];

        //p1.Tick(Time.deltaTime);
        //p2.Tick(Time.deltaTime);
        bool normalizeChange = m_normalizeLength != normalizeLength;

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
            //p2.transform.position -= p1p2.normalized * diff * 1.0f;
            p1.transform.position += p1p2.normalized * diff * 0.5f * Mathf.Max(0.05f, dumping);
            p2.transform.position -= p1p2.normalized * diff * 0.5f * Mathf.Max(0.05f, dumping);
        }

        if (normalizeChange)
        {
            p1.ApplyCurrentPosition();
            p2.ApplyCurrentPosition();
            m_normalizeLength = normalizeLength;
        }
    }

}

