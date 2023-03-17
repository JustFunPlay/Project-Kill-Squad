using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridNode
{
    private GridSystem<GridNode> grid;
    private int x;
    private int z;
    public int X { get { return x; } }
    public int Z { get { return z; } }

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable = true;
    public bool isOccupied;

    public GridNode cameFromNode;

    public GridNode(GridSystem<GridNode> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }
    public GridNode()
    {
        this.grid = null;
        this.x = -5;
        this.z = -5;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}
