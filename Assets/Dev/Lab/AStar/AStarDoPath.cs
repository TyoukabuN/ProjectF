using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  static class AStarDoPath
{
    public static MapNode curNode;
    public static int GetTwoNodeDistance(MapNode a,MapNode b)
    {
        int cntX = Mathf.Abs(a.x - b.x);
        int cntZ = Mathf.Abs(a.z - b.z);
        if (cntX > cntZ)
        {
            return 14 * cntZ + 10 * (cntX - cntZ);
        }
        else
        {
            return 14 * cntX + 10 * (cntZ - cntX);
        }
    }
    public static void DoPath(MapNode star,MapNode end)
    {
        List<MapNode> openlist = new List<MapNode>();
        List<MapNode> closelist = new List<MapNode>();

        openlist.Add(star);
        while (openlist.Count > 0)
        {
            curNode = openlist[0];
            for (int i = 0; i < openlist.Count; i++)
            {
                if (openlist[i].F <= curNode.F )
                {
                    curNode = openlist[i];
                }
            }
            openlist.Remove(curNode);
            closelist.Add(curNode);
            if (curNode == end)
            {
                return;
            }
            foreach (var item in curNode.GetNeibourNodes())
            {
                if (item.isNotPass||closelist.Contains(item))
                {
                    continue;
                }
                int newCost = curNode.g + GetTwoNodeDistance(curNode, item);
                if (newCost < item.g ||!openlist.Contains(item))
                {
                    item.g = newCost;
                    item.h = GetTwoNodeDistance(item, end);
                    item.parent = curNode;
                    if (!openlist.Contains(item))
                    {
                        openlist.Add(item);
                    }
                }
            }
        }
    }
    public static void DoAStar(Vector3 starPos,Vector3 endPos)
    {
        //不考虑y轴的
        float distance = Vector2.Distance(new Vector2(starPos.x, starPos.z), new Vector2(endPos.x, endPos.z));
        //根据距离创建多大的地图范围

        DoPath(PosToMapNode(starPos),PosToMapNode(endPos));
    }

    public static MapNode PosToMapNode(Vector3 pos)
    {
        MapNode node = new MapNode(Mathf.FloorToInt( pos.x), Mathf.FloorToInt(pos.z));

        return node;
    }
}
