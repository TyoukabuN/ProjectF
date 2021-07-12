using System;
using System.Collections.Generic;
using UnityEngine;
using TMesh;


[ExecuteInEditMode]
public class CollisionDetection : MonoBehaviour
{
    public List<Edge> Edges = new List<Edge>();

    public static CollisionDetection current;
    private void Awake()
    {
        if (current == null)
            current = this;
    }

    public static bool AddEdge(Edge edge)
    {
        if (current == null)
            current = GameObject.FindObjectOfType<CollisionDetection>();

        if (current == null)
            return false;

        if (current.Edges.Contains(edge))
            return false;


        current.Edges.Add(edge);
        return true;
        
    }

    public static bool RemoveEdge(Edge edge)
    {
        if (current == null)
            current = GameObject.FindObjectOfType<CollisionDetection>();

        if (current == null)
            return false;

        if (!current.Edges.Contains(edge))
            return false;


        current.Edges.Remove(edge);
        return true;

    }
    void Update()
    {
        UpdateEdge();
        UpdateEdge();
        UpdateEdge();
    }

    public void UpdateEdge()
    {
        foreach (var edge in Edges)
        {
            if (!edge)
                continue;

            edge.Tick();
        }
    }
}


