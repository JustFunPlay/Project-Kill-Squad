using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Grid<GridObject>
{
    private int width;
    private int length;
    private int cellSize;
    private GridObject[,] gridArray;

    public Grid(int width, int length, int cellSize)
    {
        this.width = width;
        this.length = length;
        this.cellSize = cellSize;

        gridArray = new GridObject[width, length];

        CreateGrid();
    }

    [Server] public void CreateGrid()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {

            }
        }
    }
}
