using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions
{
    public static List<Vector2> Neighbors(this Vector2 v)
    {
        return new List<Vector2>()
        {
            new Vector2(v.x + 1, v.y),
            new Vector2(v.x - 1, v.y),
            new Vector2(v.x, v.y + 1),
            new Vector2(v.x, v.y - 1),
        };
    }

    public static int DistSq(this Vector2 a, Vector2 b)
    {
        int dx = (int)(a.x - b.x);
        int dy = (int)(a.y - b.y);

        return dx * dx + dy * dy;
    }

    public static bool LessThan(this Vector2 a, Vector2 b)
    {
        if (a.x != b.x)
            return a.x < b.x;

        return a.y < b.y;
    }

    public static string Id(this Vector2 v)
    {
        return $"{v.x},{v.y}";
    }
}