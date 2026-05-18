using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExtensions
{
    public static List<Vector2Int> Neighbors(this Vector2Int v)
    {
        return new List<Vector2Int>()
        {
            new Vector2Int(v.x + 1, v.y),
            new Vector2Int(v.x - 1, v.y),
            new Vector2Int(v.x, v.y + 1),
            new Vector2Int(v.x, v.y - 1),
        };
    }

    public static int DistSq(this Vector2Int a, Vector2Int b)
    {
        int dx = (int)(a.x - b.x);
        int dy = (int)(a.y - b.y);

        return dx * dx + dy * dy;
    }

    public static bool LessThan(this Vector2Int a, Vector2Int b)
    {
        if (a.x != b.x)
            return a.x < b.x;

        return a.y < b.y;
    }

    public static string Id(this Vector2Int v)
    {
        return $"{v.x},{v.y}";
    }
}