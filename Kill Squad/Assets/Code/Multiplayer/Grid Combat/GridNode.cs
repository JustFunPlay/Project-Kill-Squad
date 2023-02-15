using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    private Grid<GridNode> grid;
    private int x;
    private int z;
    public int X { get { return x; } }
    public int Z { get { return z; } }

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable = true;

    public GridNode cameFromNode;

    public GridNode(Grid<GridNode> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}
