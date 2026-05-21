using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    ANY = -2,
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

    public Vector2Int Coords;

    [SerializeField] public bool Active = true;

    public List<Corridor> Connections = new();
    
    public bool isFromShortcut = false; // Used just for easy of check during map generation.

    public Room( RoomType type, Vector2Int coords, bool active = true)
    {
        Type = type;
        Coords = coords;
        Active = active;
    }

    public Vector2Int Id()
    {
        return Coords;
    }

    public void Activate(bool newStatus)
    {
        Active = newStatus;
    }
}
