using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

[Serializable]
public class GridSystem<TGridObject> : SyncObject
{
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int z;
    }
    [SerializeField]private int width;
    [SerializeField]private int length;
    [SerializeField]private float cellSize;
    [SerializeField]private Vector3 originPosition;
    [SerializeField]public TGridObject[,] gridArray;

    public GridSystem(int width, int length, float cellSize, Vector3 originPosition, Func<GridSystem<TGridObject>, int, int, TGridObject> createGridObject)
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
                gridArray[x, z] = createGridObject(this, x, z);
            }
        }
    }
    public GridSystem()
    {
        gridArray = null;
    }


    public int GetWidth() { return width; }
    public int GetLength() { return length; }
    public float GetCellSize() { return cellSize; }
    //public int GetGridsize() { return gridArray.Length; }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }
    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    public void TriggerGridObjectChanged(int x, int z)
    {
            if (OnGridValueChanged != null)
                OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, z = z });
    }
    public void SetGridObject(int x, int z, TGridObject value)
    {
        if (x >= 0 && z >= 0 && x < width && z < length)
        {
            gridArray[x, z] = value;
            TriggerGridObjectChanged(x, z);
        }
    }
    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        SetGridObject(x, z, value);
    }

    public TGridObject GetGridObject(int x, int z)
    {
        if (x >= 0 && z >= 0 && x < width && z < length)
            return gridArray[x, z];
        else
            return default(TGridObject);
    }
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        return (GetGridObject(x, z));
    }

    public override void ClearChanges()
    {

    }

    public override void OnSerializeAll(NetworkWriter writer)
    {
        writer.WriteInt(GetWidth());
        writer.WriteInt(GetLength());
        writer.WriteFloat(GetCellSize());
        writer.WriteVector3(originPosition);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                writer.Write(gridArray[x, z]);
            }
        }
    }

    public override void OnSerializeDelta(NetworkWriter writer)
    {
        writer.WriteInt(GetWidth());
        writer.WriteInt(GetLength());
        writer.WriteFloat(GetCellSize());
        writer.WriteVector3(originPosition);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                writer.Write<TGridObject>(gridArray[x, z]);
            }
        }
    }

    public override void OnDeserializeAll(NetworkReader reader)
    {
        width = reader.ReadInt();
        length = reader.ReadInt();
        cellSize = reader.ReadFloat();
        originPosition = reader.ReadVector3();
        gridArray = new TGridObject[width, length];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                reader.Read<TGridObject>();
            }
        }
    }

    public override void OnDeserializeDelta(NetworkReader reader)
    {
        width = reader.ReadInt();
        length = reader.ReadInt();
        cellSize = reader.ReadFloat();
        originPosition = reader.ReadVector3();
        gridArray = new TGridObject[width, length];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                reader.Read<TGridObject>();
            }
        }
    }

    public override void Reset()
    {
        width = 0;
        length = 0;
        cellSize = 0;
        originPosition = Vector3.zero;
        gridArray = null;
    }
}
