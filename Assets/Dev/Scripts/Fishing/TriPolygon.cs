using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMesh;

public class TriPolygon : TTrangle
{
    //public Edge[] edges;
    //public Edge GetEdge(int index)
    //{
    //    if (edges == null)
    //    {
    //        edges = new Edge[3] {
    //            new Edge(this[0].transform.position,this[1].transform.position),
    //            new Edge(this[0].transform.position,this[2].transform.position),
    //            new Edge(this[1].transform.position,this[2].transform.position),
    //        };
    //    }

    //    return edges[index];
    //}

}

//public class Edge
//{
//    public Vector3[] points = new Vector3[2];

//    public Edge(Vector3 p1, Vector3 p2)
//    {
//        points[0] = p1;
//        points[1] = p2;
//    }

//    public Vector3 this[int index]
//    {
//        get
//        {
//            return points[index];
//        }
//    }
//    public bool IsSameEdge(Edge y)
//    {
//        var x = this;

//        if (x[0] == y[0] && x[1] == y[1])
//        {
//            return true;
//        }
//        else if (x[0] == y[1] && x[1] == y[0])
//        {
//            return true;
//        }

//        return false;
//    }


//}
