using System.Collections.Generic;
using UnityEngine;

public class MapShortcut
{
    public List<Vector2> Rooms = new();

    public List<string> Corridors = new();

    public MapShortcut( List<Vector2> rooms, List<string> corridors )
    {
        Rooms = rooms;
        Corridors = corridors;
    }
}
