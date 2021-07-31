using System;
using System.Collections.Generic;
using UnityEngine;
using TMesh;


[ExecuteInEditMode]
public class CollisionDetection
{
    public List<Edge> Edges = new List<Edge>();

    public int stepCount
    {
        get{
            return Edges.Count;
        }
    }

    public bool AddEdge(Edge edge)
    {
        if (Edges.Contains(edge))
            return false;

        Edges.Add(edge);
        return true;
    }

    public bool AddEdge(Point p1, Point p2)
    {
        return AddEdge(new Edge(p1, p2));
    }
    public bool AddEdge(Point p1, Point p2,float length)
    {
        return AddEdge(new Edge(p1, p2, length));
    }


    public bool RemoveEdge(Edge edge)
    {
        if (!Edges.Contains(edge))
            return false;

        Edges.Remove(edge);
        return true;

    }
    public void OnUpdate()
    {
        UpdateEdge();
    }

    private void EdgeVaildCheck()
    {
        for (int i = 0; i < Edges.Count; i++)
        {
            if (!Edges[i].Vaild())
            { 
                Edges.RemoveAt(i);
                i--;
            }
        }
    }

    private void UpdateEdge()
    {
        EdgeVaildCheck();

        foreach (var edge in Edges)
        {
            if (edge == null)
                continue;

            edge.Tick();
        }
    }
}


