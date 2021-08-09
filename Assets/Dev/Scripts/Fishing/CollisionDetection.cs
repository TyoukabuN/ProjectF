using System;
using System.Collections.Generic;
using UnityEngine;
using TMesh;


[ExecuteInEditMode]
public class CollisionDetection
{
    public List<Edge> Edges = new List<Edge>();

    public int edgeCount
    {
        get{
            return Edges.Count;
        }
    }

    public Edge AddEdge(Edge edge)
    {
        if (Edges.Contains(edge))
            return null;

        Edges.Add(edge);
        return edge;
    }

    public Edge AddEdge(Point p1, Point p2)
    {
        return AddEdge(new Edge(p1, p2));
    }
    public Edge AddEdge(Point p1, Point p2,float length)
    {
        return AddEdge(new Edge(p1, p2, length));
    }

    public void SetEdgesLength(float length)
    {
        for (int i = 0; i < Edges.Count; i++)
        {
            Edges[i].m_length = length;
        }
    }

    public bool RemoveEdge(Edge edge,bool destroyPoint = true)
    {
        if (edge == null)
            return false;

        if (!Edges.Contains(edge))
            return false;

        if (Edges.Remove(edge) && destroyPoint)
        {
            GameObject.Destroy(edge.points[0].gameObject);
            GameObject.Destroy(edge.points[1].gameObject);
        }
        return true;
    }

    public void OnUpdate()
    {
        UpdateEdge();
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


