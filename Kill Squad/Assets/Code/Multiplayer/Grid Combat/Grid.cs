using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Grid<TGridObject>
{
    private int width;
    private int length;
    private int cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public Grid(int width, int length, int cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.length = length;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, length];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < gridArray.GetLength(1); z++)
            {

            }
        }
    }

    public int GetWidth() { return width; }
    public int GetLength() { return length; }
    public float GetCellSize() { return cellSize; }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }
    private void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    public void SetValue(int x, int z, TGridObject value)
    {
        if (x >= 0 && z >= 0 && x < width && z < length)
        {
            gridArray[x, z] = value;
            
        }
    }
    public void SetValue(Vector3 worldPosition, TGridObject value)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        SetValue(x, z, value);
    }

    public TGridObject GetValue(int x, int z)
    {
        if (x >= 0 && z >= 0 && x < width && z < length)
            return gridArray[x, z];
        else
            return default(TGridObject);
    }
    public TGridObject GetValue(Vector3 worldPosition)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        return (GetValue(x, z));
    }
}
