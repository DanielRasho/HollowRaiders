using System.Collections.Generic;
using UnityEngine;

public class MapCycle
{
    public int Id;

    public List<Vector2> Rooms = new();

    public HashSet<Vector2> AllPoints = new();

    public MapCycle(int id, HashSet<Vector2> allPoints)
    {
        Id = id;
        AllPoints = allPoints;
    }
}
