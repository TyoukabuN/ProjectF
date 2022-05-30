using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode 
{
    public Vector3 position;
    public int x;
    public int z;
    public bool isNotPass;

    public MapNode parent;
    public int g;
    public int h;
    public int F
    {
        get
        {
            return g + h;
        }
    }
    public MapNode(int x, int z)
    {
        this.x = x;
        this.z = z;
        this.position = new Vector3(x, 0, z);
    }

    public MapNode(int x,int z, bool isNotPass):this(x,z)
    {
        this.isNotPass = isNotPass;
    }

    public List<MapNode> GetNeibourNodes()
    {
        List<MapNode> list = new List<MapNode>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                int x = this.x + i;
                int z = this.z + j;
               
                list.Add(new MapNode(x,z));
            }
        }
        return list;
    }
}
