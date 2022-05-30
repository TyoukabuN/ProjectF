using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarMapInit
{
    public int rangeX;
    public int rangeY;

    public MapNode[,] mapNodes;

    public AStarMapInit(int rangeX,int rangeY)
    {
        this.rangeX = rangeX;
        this.rangeY = rangeY;
        mapNodes = new MapNode[this.rangeX, this.rangeY];
        
    }
}
