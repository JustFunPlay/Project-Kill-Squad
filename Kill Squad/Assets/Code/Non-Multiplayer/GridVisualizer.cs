using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public static GridVisualizer instance;
    [Header("Visualisation")]
    [SerializeField] private GameObject gridCube;
    private GameObject[,] gridVisualizer;
    [SerializeField] private int gridSizeX, gridSizeZ;
    [SerializeField] private Vector3 gridOrigin;
    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    public void SetupGridVisualizer(Vector3 gridOrigin, int sizeX, int sizeZ)
    {
        this.gridOrigin = gridOrigin;
        gridSizeX = sizeX;
        gridSizeZ = sizeZ;
        gridVisualizer = new GameObject[gridSizeX, gridSizeZ];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 origin = gridOrigin + new Vector3(1 + x * 2, 0.01f, 1 + z * 2);
                GameObject newVisualizer = Instantiate(gridCube, origin, Quaternion.identity, transform);
                gridVisualizer[x, z] = newVisualizer;
                gridVisualizer[x, z].SetActive(false);
            }
        }
    }

    public void VisualizeRange(List<Vector3> positions)
    {
        ResetVisualRange();
        for (int i = 0; i < positions.Count; i++)
        {
            if (GetVisualizer(positions[i]) != null)
                GetVisualizer(positions[i]).SetActive(true);
        }
    }
    public void ResetVisualRange()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                gridVisualizer[x, z].SetActive(false);
            }
        }
    }
    private GameObject GetVisualizer(Vector3 worldPosition)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        //Debug.Log($"Position: {worldPosition}; {x} {z}");
        if (x < gridSizeX && z < gridSizeZ)
            return (gridVisualizer[x, z]);
        return null;
    }
    private void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - gridOrigin).x / 2f);
        z = Mathf.FloorToInt((worldPosition - gridOrigin).z / 2f);
    }
}
