using System;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Render")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] public Vector2 size = new Vector2(1000, 1000);
    [SerializeField] private int cellSize = 1;
    [SerializeField] private float lineWidth = 0.2f;
    
    void Start()
    {
        if (showGrid)
        {
            DrawGrid();
        }
    }

    Vector2 GetGridPosition(Vector2 coords)
    {
        Vector2 gridPosition = new Vector2();
        gridPosition.x = Mathf.Round(coords.x / cellSize) * cellSize;
        gridPosition.y = Mathf.Round(coords.y / cellSize) * cellSize;
        return gridPosition;
    }

    void DrawGrid()
    {
        var a = new Vector3();
        var b = new Vector3();
        for (int x = 0; x <= size.y; x++)
        {
            a.Set(x * cellSize - 0.5f, 0 - 0.5f, 0);
            b.Set(x * cellSize - 0.5f,size.y * cellSize -0.5f, 0);
            CreateLine(a, b);
        }
        for (int y = 0; y <= size.y; y++)
        {
            a.Set(0 - 0.5f, y * cellSize - 0.5f, 0);
            b.Set( size.x * cellSize - 0.5f, y * cellSize -0.5f, 0);
            CreateLine(a, b);
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = transform;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.widthMultiplier = lineWidth;
    }

}
