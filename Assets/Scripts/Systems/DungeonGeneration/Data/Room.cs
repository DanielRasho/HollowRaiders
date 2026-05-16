using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    UNASSIGNED = -1,
    START = 0,
    BATTLE = 1,
    ENCOUNTER = 2,
    RESOURCE = 3,
    MISSION = 4
}
public class Room
{
    [SerializeField] public RoomType Type;

    public Vector2 Coords;

    [SerializeField] public bool Active = true;

    public List<Corridor> Connections = new();

    public Room( RoomType type, Vector2 coords, bool active = true)
    {
        Type = type;
        Coords = coords;
        Active = active;
    }

    public Vector2 Id()
    {
        return Coords;
    }

    public void Activate(bool newStatus)
    {
        Active = newStatus;
    }
}
