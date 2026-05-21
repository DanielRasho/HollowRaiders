using System.Collections.Generic;
using UnityEngine;

public class MapCycle
{
    public int Id;

    public List<Vector2Int> Rooms = new();

    public HashSet<Vector2Int> AllPoints = new();

    public MapCycle(int id, HashSet<Vector2Int> allPoints)
    {
        Id = id;
        AllPoints = allPoints;
    }
}
