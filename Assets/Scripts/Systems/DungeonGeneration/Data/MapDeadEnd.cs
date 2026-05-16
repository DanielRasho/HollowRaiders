using System.Collections.Generic;
using UnityEngine;

public class MapDeadEnd
{
    public int Id;

    public List<Vector2> Rooms = new();

    public MapDeadEnd(int id)
    {
        Id = id;
    }
}
