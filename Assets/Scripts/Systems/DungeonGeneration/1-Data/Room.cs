using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField] public bool IsActive = true;

    public List<Corridor> Connections = new();
    
    public bool isFromShortcut = false; // Used just for easy of check during map generation.

    private RoomView view;

    public RoomView View
    {
        set => view = value;
    }

    public Room( RoomType type, Vector2Int coords, bool isActive = true)
    {
        Type = type;
        Coords = coords;
        IsActive = isActive;
    }

    public Vector2Int Id()
    {
        return Coords;
    }

    public void Activate(bool newStatus)
    {
        IsActive = newStatus;
        UpdateView();
    }

    public void StartView()
    {
        view.Populate();
        view.FillWithEnemies();
        UpdateView();
    }

    public void UpdateView()
    {
        if (view == null) return;
        
        view.Activate(IsActive);

        if (!IsActive) return;

        bool north = true;
        bool east = true;
        bool south = true;
        bool west = true;

        foreach (var corridor in Connections)
        {
            if (!corridor.IsActive) continue;
            
            var a = corridor.A;
            var b = corridor.B;
            Room other = a.Id() == Id() ? b : a;

            if (corridor.Type == CorridorType.HORIZONTAL)
            {
                if (Coords.x < other.Coords.x)
                    east = false;
                else
                    west = false;
            }
            else
            {
                if (Coords.y < other.Coords.y)
                    north = false;
                else
                    south = false;
            }
        }
        
        view.SetExits(north, east, south, west);

    }
}
