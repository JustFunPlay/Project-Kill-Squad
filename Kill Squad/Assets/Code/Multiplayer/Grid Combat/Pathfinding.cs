using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Pathfinding : NetworkBehaviour
{
    public GridSystem<GridNode> grid = new GridSystem<GridNode>();
    private List<GridNode> openList;
    private List<GridNode> closedList;

    [Server] public void InitializeGrid(int width, int height, Vector3 origin)
    {
        grid = new GridSystem<GridNode>(width, height, 2f, origin, (GridSystem<GridNode> grid, int x, int z) => new GridNode(grid, x, z));
    }
    
    [Server]public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos, bool trueMovement)
    {
        grid.GetXZ(startPos, out int startX, out int startZ);
        grid.GetXZ(endPos, out int endX, out int endZ);
        List<GridNode> path = FindPath(startX, startZ, endX, endZ, trueMovement);
        if (path == null)
            return null;
        List<Vector3> vectorPath = new List<Vector3>();
        foreach (GridNode gridNode in path)
        {
            vectorPath.Add(grid.GetWorldPosition(gridNode.X, gridNode.Z));
        }
        return vectorPath;
    }

    [Server]public List<GridNode> FindPath(int startX, int startZ, int endX, int endZ, bool trueMovement)
    {
        GridNode startNode = grid.GetGridObject(startX, startZ);
        GridNode endNode = grid.GetGridObject(endX, endZ);

        openList = new List<GridNode> { startNode };
        closedList = new List<GridNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetLength(); z++)
            {
                GridNode pathnode = grid.GetGridObject(x, z);
                pathnode.gCost = int.MaxValue;
                pathnode.CalculateFCost();
                pathnode.cameFromNode = null;
            }
        }
        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();
        while (openList.Count > 0)
        {
            GridNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (GridNode neighborNode in GetneighborList(currentNode, trueMovement))
            {
                if (closedList.Contains(neighborNode))
                    continue;
                if (!neighborNode.isWalkable)
                {
                    closedList.Add(neighborNode);
                    continue;
                }
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);
                //Debug.Log($"tentative cost: {tentativeGCost}, actual cost: {neighborNode.gCost}");
                if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);
                    neighborNode.CalculateFCost();
                    if (!openList.Contains(neighborNode))
                        openList.Add(neighborNode);
                }
            }
        }
        //Debug.Log($"Failed to create path\n {closedList.Count} options checked");
        return null;
    }
    [Server]private List<GridNode> CalculatePath(GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();

        path.Add(endNode);
        GridNode currentnode = endNode;
        while (currentnode.cameFromNode != null)
        {
            path.Add(currentnode.cameFromNode);
            currentnode = currentnode.cameFromNode;
        }
        path.Reverse();

        return path;
    }

    protected List<GridNode> GetneighborList(GridNode currentnode, bool requiresMovePossibility)
    {
        List<GridNode> neighbors = new List<GridNode>();

        if ((currentnode.canMoveWest || !requiresMovePossibility) && currentnode.X - 1 >= 0)
        {
            neighbors.Add(GetNode(currentnode.X - 1, currentnode.Z));
        }
        if ((currentnode.canMoveEast || !requiresMovePossibility) && currentnode.X + 1 < grid.GetWidth())
        {
            neighbors.Add(GetNode(currentnode.X + 1, currentnode.Z));
        }
        if ((currentnode.canMoveSouth || !requiresMovePossibility) && currentnode.Z - 1 >= 0)
        {
            neighbors.Add(GetNode(currentnode.X, currentnode.Z - 1));
        }
        if ((currentnode.canMoveNorth || !requiresMovePossibility) && currentnode.Z + 1 < grid.GetLength())
        {
            neighbors.Add(GetNode(currentnode.X, currentnode.Z + 1));
        }
        //Debug.Log($"{neighbors.Count} neighbors");
        return neighbors;
    }
    [Server]private GridNode GetNode(int x, int z)
    {
        return grid.GetGridObject(x, z);
    }

    [Server]private int CalculateDistanceCost(GridNode a, GridNode b)
    {
        int xDistance = Mathf.Abs(a.X - b.X);
        int zDistance = Mathf.Abs(a.Z - b.Z);
        //int remaining = Mathf.Abs(xDistance - zDistance);
        return xDistance + zDistance;
    }
    [Server]private GridNode GetLowestFCostNode(List<GridNode> gridNodeList)
    {
        GridNode lowestFCostNode = gridNodeList[0];
        for (int i = 0; i < gridNodeList.Count; i++)
        {
            if (gridNodeList[i].fCost < lowestFCostNode.fCost)
                lowestFCostNode = gridNodeList[i];
        }
        return lowestFCostNode;
    }
}
