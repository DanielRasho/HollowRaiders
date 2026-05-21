using UnityEngine;
using System;
using System.Collections.Generic;

public class Map
{
    public Dictionary<Vector2Int, Room> Rooms = new();

    public Dictionary<string, Corridor> Corridors = new();

    public Dictionary<int, MapCycle> Cycles = new();

    public Dictionary<int, MapDeadEnd> DeadEnds = new();

    public List<MapShortcut> Shortcuts = new();
    
    public void AddCycle(
        int cycleId,
        List<Room> rooms,
        HashSet<Vector2Int> points
    )
    {
        MapCycle newCycle = new MapCycle(cycleId, points);

        Cycles[cycleId] = newCycle;

        foreach (Room r in rooms)
        {
            Vector2Int id = r.Id();

            Rooms.TryAdd(id, r);

            if (!newCycle.Rooms.Contains(id))
            {
                newCycle.Rooms.Add(id);
            }
        }

        if (rooms.Count <= 1)
            return;

        for (int i = 0; i < rooms.Count; i++)
        {
            int nextIdx = (i + 1) % rooms.Count;

            Corridor corridor = new Corridor(rooms[i], rooms[nextIdx]);

            if (Corridors.TryAdd(corridor.Id, corridor))
            {
                JoinRooms(
                    rooms[i],
                    rooms[nextIdx],
                    corridor
                );
            }
        }
    }

    public bool AddShortcut(List<Vector2Int> points)
    {
        List<Vector2Int> rooms = new();

        List<string> corridors = new();

        foreach (Vector2Int p in points)
        {
            if (!Rooms.ContainsKey(p))
            {
                Room newRoom = new Room(RoomType.UNASSIGNED, p, true);
                newRoom.isFromShortcut = true;
                Rooms[p] = newRoom;
                rooms.Add(p);
            }
        }

        if (points.Count <= 1)
            return false;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Room room = Rooms[points[i]];
            Room nextRoom = Rooms[points[i + 1]];
            Corridor corridor = new Corridor(room, nextRoom);
            corridor.isFromShortcut = true;

            if (!Corridors.ContainsKey(corridor.Id))
            {
                Corridors[corridor.Id] = corridor;

                corridors.Add(corridor.Id);

                JoinRooms(
                    room,
                    nextRoom,
                    corridor
                );
            }
        }

        if (rooms.Count > 0 || corridors.Count > 0)
        {
            Shortcuts.Add(
                new MapShortcut(rooms, corridors)
            );

            return true;
        }

        return false;
    }

    void JoinRooms( Room a, Room b, Corridor c)
    {
        a.Connections.Add(c);
        b.Connections.Add(c);
    }

    public HashSet<int> GetRoomCycles(Vector2Int roomId)
    {
        HashSet<int> cycles = new();

        foreach (MapCycle cycle in Cycles.Values)
        {
            if (cycle.Rooms.Contains(roomId))
            {
                cycles.Add(cycle.Id);
            }
        }

        return cycles;
    }
    
    public void ActivateShortcut( MapShortcut shortcut, bool newStatus)
    {
        foreach (Vector2Int r in shortcut.Rooms)
        {
            Rooms[r].Activate(newStatus);
        }

        foreach (string c in shortcut.Corridors)
        {
            Corridors[c].Activate(newStatus);
        }
    }
}
