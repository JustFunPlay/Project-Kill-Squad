using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    //[Header("Visualisation")]
    //[SerializeField] private GameObject gridCube;
    //public GameObject[,] gridVisualizer;
    //[SerializeField] private int gridSizeX, gridSizeZ;
    //[SerializeField] private Vector3 gridOrigin;

    //private void SetupGridVisualizer()
    //{
    //    GridCombatSystem grid = GridCombatSystem.instance;
    //    gridSizeX = grid.grid.GetWidth();
    //    gridSizeZ = grid.grid.GetLength();
    //    gridVisualizer = new GameObject[gridSizeX, gridSizeZ];
    //    for (int x = 0; x < gridSizeX; x++)
    //    {
    //        for (int z = 0; z < gridSizeZ; z++)
    //        {
    //            GameObject newVisualizer = Instantiate(gridCube, grid.grid.GetWorldPosition(x, z) + new Vector3(0, 0.1f, 0), Quaternion.identity, transform);
    //            gridVisualizer[x, z] = newVisualizer;
    //            gridVisualizer[x, z].SetActive(false);
    //        }
    //    }
    //}

    //public void VisualizeRange(Vector3 origin, int range, bool requiresLos)
    //{
    //    ResetVisualRange();
    //    List<GameObject> validPositions = new List<GameObject> { GetVisualizer(origin) };
    //    for (int i = 0; i < range; i++)
    //    {
    //        int currentPositions = validPositions.Count;
    //        for (int ii = 0; ii < currentPositions; ii++)
    //        {
    //            foreach (GameObject neighborNode in GetneighborList(validPositions[ii]))
    //            {
    //                if (validPositions.Contains(neighborNode) || !neighborNode.isWalkable)
    //                    continue;
    //                if (!requiresLos)
    //                {
    //                    validPositions.Add(neighborNode);
    //                    continue;
    //                }
    //                bool hasLos = false;
    //                for (int l = 0; l < 4; l++)
    //                {
    //                    Vector3 startpos = origin + Vector3.up;
    //                    if (l == 1)
    //                        startpos += Vector3.forward * 0.95f;
    //                    else if (l == 2)
    //                        startpos += Vector3.back * 0.95f;
    //                    else if (l == 3)
    //                        startpos += Vector3.left * 0.95f;
    //                    else
    //                        startpos += Vector3.right * 0.95f;

    //                    if (Physics.Raycast(startpos, (grid.GetWorldPosition(neighborNode.X, neighborNode.Z) - startpos).normalized, Vector3.Distance(startpos, grid.GetWorldPosition(neighborNode.X, neighborNode.Z)), obstacleLayer) == false)
    //                    {
    //                        hasLos = true;
    //                        break;
    //                    }
    //                }
    //                if (hasLos)
    //                    validPositions.Add(neighborNode);
    //            }
    //        }
    //    }
    //    foreach (GridNode validPos in validPositions)
    //    {
    //        gridVisualizer[validPos.X, validPos.Z].SetActive(true);
    //    }
    //}
    //public void ResetVisualRange()
    //{
    //    for (int x = 0; x < grid.GetWidth(); x++)
    //    {
    //        for (int z = 0; z < grid.GetLength(); z++)
    //        {
    //            gridVisualizer[x, z].SetActive(false);
    //        }
    //    }
    //}

    //protected List<GridNode> GetneighborList(GridNode currentnode)
    //{
    //    List<GridNode> neighbors = new List<GridNode>();

    //    if (currentnode.X - 1 >= 0)
    //    {
    //        neighbors.Add(GetVisualizer(currentnode.X - 1, currentnode.Z));
    //    }
    //    if (currentnode.X + 1 < grid.GetWidth())
    //    {
    //        neighbors.Add(GetVisualizer(currentnode.X + 1, currentnode.Z));
    //    }
    //    if (currentnode.Z - 1 >= 0)
    //    {
    //        neighbors.Add(GetVisualizer(currentnode.X, currentnode.Z - 1));
    //    }
    //    if (currentnode.Z + 1 < grid.GetLength())
    //    {
    //        neighbors.Add(GetVisualizer(currentnode.X, currentnode.Z + 1));
    //    }
    //    //Debug.Log($"{neighbors.Count} neighbors");
    //    return neighbors;
    //}

    //private GameObject GetVisualizer(int x, int z)
    //{

    //}
}
