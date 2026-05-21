using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator
{

    private MapConfig cfg;
    private ContentAutomata automata;
    
    public DungeonGenerator(MapConfig config)
    {
        cfg = config;
    }

    public Map Generate(TetrisTilemap tilemap)
    {
        /*
        // CREATE TETRIS TEXTURE
        tilemap = new TetrisTilemap(cfg);
        tilemap.Generate();

        // SELECT CONNECTED TETRIS TILES
        var chain = tilemap.BuildChain();
        tilemap = tilemap.FilterToChain(chain);
        */

        // EXPAND PIECES TO ITS VERTEX
        Map map = new Map();

        Dictionary<Vector2Int, List<int>> allPoints = new Dictionary<Vector2Int, List<int>>();

        int cycleId = 0;

        foreach (var tile in tilemap.Tiles.Values)
        {
            Debug.Log("Expand Tetris Tiles");
            HashSet<Vector2Int> points = ExpandTetrisTiles(tile.Cells);

            // Keep track of points
            foreach (var p in points)
            {
                if (!allPoints.ContainsKey(p))
                {
                    allPoints[p] = new List<int> { cycleId };
                }
                else
                {
                    allPoints[p].Add(cycleId);
                }
            }

            Debug.Log("Compute Hull");
            // Create rooms
            List<Vector2Int> loop = ComputeHull(tile.Cells);
            
            Debug.Log(loop.Count);
            Debug.Log(points.Count);

            List<Room> rooms = new List<Room>();

            foreach (var p in loop)
            {
                rooms.Add(
                    new Room(RoomType.UNASSIGNED, p, true)
                );
            }

            Debug.Log("Insert Cycle");
            map.AddCycle(cycleId, rooms, points);

            cycleId++;
        }
        
        Debug.Log("CORRIDORS: " + map.Corridors.Count);

        // Place start points and mission points
        Debug.Log("Define Landmarks");
        DefineLandmarks(map);

        // Create shortcuts
        Debug.Log("Add shortcuts");
        AddShortcuts(map);
        return map;
    }

    // ======================
    // CREATE SHORTCUTS
    // ======================

    private void AddShortcuts(Map map)
    {
        foreach (var kv in map.Cycles)
        {
            int id = kv.Key;
            var cycle = kv.Value;

            for (int i = 0; i < 1000; i++)
            {
                Vector2Int start = cycle.Rooms[UnityEngine.Random.Range(0, cycle.Rooms.Count)];
                Vector2Int end = GetOpositeRoom(map, id, start, null);
                List<Vector2Int> path = BFSPath(start, end, cycle.Rooms.ToHashSet());
                if (path.Count == 0) continue;
                bool isValid = map.AddShortcut(path);
                if (isValid)
                    break;
            }
        }
    }

    // ======================
    // LANDMARKS DEFINITION
    // ======================

    private void DefineLandmarks(Map map)
    {
        List<int> cycleKeys = map.Cycles.Keys.ToList();

        int currentCycle = cycleKeys[UnityEngine.Random.Range(0, cycleKeys.Count)];

        Vector2Int currentCoords = map.Cycles[currentCycle]
                .Rooms[UnityEngine.Random.Range(
                    0,
                    map.Cycles[currentCycle].Rooms.Count)];

        Vector2Int initialCoords = currentCoords;

        HashSet<int> visitedCycles = new HashSet<int>();

        HashSet<Vector2Int> markedRooms =
            new HashSet<Vector2Int> { currentCoords };

        List<Vector2Int> roomsOnCycle =
            new List<Vector2Int> { currentCoords };

        while (true)
        {
            Vector2Int opositeRoomCoords = currentCoords;

            while (roomsOnCycle.Count < 2)
            {
                opositeRoomCoords =
                    GetOpositeRoom(
                        map,
                        currentCycle,
                        currentCoords,
                        markedRooms);

                Room opositeRoom =
                    map.Rooms[opositeRoomCoords];

                opositeRoom.Type = RoomType.MISSION;

                // Update room
                currentCoords = opositeRoomCoords;

                // Update counters
                markedRooms.Add(currentCoords);
                roomsOnCycle.Add(currentCoords);
            }

            roomsOnCycle.Clear();

            visitedCycles.Add(currentCycle);

            // Should stop room marking
            if (visitedCycles.Count == map.Cycles.Count)
                break;

            // Choose next cycle
            HashSet<int> candidateCycles =
                map.GetRoomCycles(opositeRoomCoords);

            candidateCycles.ExceptWith(visitedCycles);

            if (candidateCycles.Count > 0)
            {
                List<int> list = candidateCycles.ToList();

                currentCycle =
                    list[UnityEngine.Random.Range(0, list.Count)];

                markedRooms.Add(currentCoords);
                roomsOnCycle.Add(currentCoords);
            }
            else
            {
                candidateCycles =
                    new HashSet<int>(map.Cycles.Keys);

                candidateCycles.ExceptWith(visitedCycles);

                List<int> list = candidateCycles.ToList();

                currentCycle =
                    list[UnityEngine.Random.Range(0, list.Count)];

                currentCoords =
                    GetFurthestPoint(
                        map.Cycles[currentCycle].Rooms,
                        roomsOnCycle,
                        markedRooms);

                markedRooms.Add(currentCoords);
                roomsOnCycle.Add(currentCoords);
            }

            // Mark start room
            Room currentRoom = map.Rooms[currentCoords];
            currentRoom.Type = RoomType.MISSION;
        }

        Room start = map.Rooms[initialCoords];
        start.Type = RoomType.START;

        // Simplify points
        ReduceMarkedPoints(
            map,
            markedRooms,
            cfg.maxLandmarks);
    }

    private Vector2Int GetOpositeRoom(
        Map map,
        int cycleId,
        Vector2Int point,
        HashSet<Vector2Int> markedPoints)
    {
        var cycle = map.Cycles[cycleId];

        int distance = cycle.Rooms.Count / 2;

        int idx = cycle.Rooms.IndexOf(point);

        int opositeRoomIdx =
            (idx + distance) % cycle.Rooms.Count;

        if (markedPoints != null)
        {
            while (markedPoints.Contains(
                       cycle.Rooms[opositeRoomIdx]))
            {
                opositeRoomIdx++;

                opositeRoomIdx %= cycle.Rooms.Count;
            }
        }

        return cycle.Rooms[opositeRoomIdx];
    }

    private Vector2Int GetFurthestPoint(
        List<Vector2Int> candidates,
        List<Vector2Int> currentPoints,
        HashSet<Vector2Int> markedPoints)
    {
        if (candidates.Count == 0)
            throw new Exception("Candidates list is empty");

        if (currentPoints.Count == 0)
            return candidates[0];

        int bestCandidateIdx = -1;
        float bestDistance = -1f;

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int candidate = candidates[i];

            float minDist = float.PositiveInfinity;

            foreach (var p in currentPoints)
            {
                float dx = candidate.x - p.x;
                float dy = candidate.y - p.y;

                float distSq = dx * dx + dy * dy;

                if (distSq < minDist)
                    minDist = distSq;
            }

            if (minDist > bestDistance)
            {
                bestDistance = minDist;
                bestCandidateIdx = i;
            }
        }

        while (markedPoints.Contains(
                   candidates[bestCandidateIdx]))
        {
            bestCandidateIdx++;

            bestCandidateIdx %= candidates.Count;
        }

        return candidates[bestCandidateIdx];
    }

    private HashSet<Vector2Int> ReduceMarkedPoints(
        Map map,
        HashSet<Vector2Int> markedPoints,
        int maxMarkedPoints)
    {
        if (maxMarkedPoints <= 0)
            return new HashSet<Vector2Int>();

        HashSet<Vector2Int> marked = new HashSet<Vector2Int>(markedPoints);

        HashSet<Vector2Int> possiblePoints = new HashSet<Vector2Int>(map.Rooms.Keys);

        while (marked.Count > maxMarkedPoints)
        {
            List<Vector2Int> markedList =
                marked.ToList();

            (Vector2Int, Vector2Int)? bestPair = null;

            float bestDist = float.PositiveInfinity;

            // FIND CLOSEST VALID PAIR

            for (int i = 0; i < markedList.Count; i++)
            {
                for (int j = i + 1; j < markedList.Count; j++)
                {
                    Vector2Int a = markedList[i];
                    Vector2Int b = markedList[j];

                    Room roomA = map.Rooms[a];
                    Room roomB = map.Rooms[b];

                    // never merge two START rooms
                    if (
                        roomA.Type == RoomType.START &&
                        roomB.Type == RoomType.START
                    )
                    {
                        continue;
                    }

                    float d = a.DistSq(b);

                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestPair = (a, b);
                    }
                }
            }

            if (bestPair == null) break;

            Vector2Int pa = bestPair.Value.Item1;
            Vector2Int pb = bestPair.Value.Item2;

            Room roomPa = map.Rooms[pa];
            Room roomPb = map.Rooms[pb];

            // PATH MIDPOINT

            List<Vector2Int> path = BFSPath(pa, pb, possiblePoints);

            if (path.Count == 0)
            {
                Vector2Int victim = roomPb.Type != RoomType.START ? pb : pa;

                marked.Remove(victim);

                map.Rooms[victim].Type =
                    RoomType.UNASSIGNED;

                continue;
            }


            // DEGENERATE MIDPOINT FIX
            Vector2Int midpoint = path[path.Count / 2];
            if (midpoint == pa || midpoint == pb)
            {
                bool found = false;
                Vector2Int replacement = default;

                foreach (var n in midpoint.Neighbors())
                {
                    if (
                        possiblePoints.Contains(n) &&
                        !marked.Contains(n)
                    )
                    {
                        replacement = n;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Vector2Int victim = roomPb.Type != RoomType.START ? pb : pa;

                    marked.Remove(victim);

                    map.Rooms[victim].Type =
                        RoomType.UNASSIGNED;

                    continue;
                }

                midpoint = replacement;
            }

            // MERGE

            marked.Remove(pa);
            marked.Remove(pb);

            marked.Add(midpoint);

            if (
                roomPa.Type == RoomType.START ||
                roomPb.Type == RoomType.START
            )
            {
                map.Rooms[midpoint].Type =
                    RoomType.START;
            }
            else
            {
                map.Rooms[midpoint].Type = RoomType.MISSION;
            }

            map.Rooms[pa].Type = RoomType.UNASSIGNED;

            map.Rooms[pb].Type = RoomType.UNASSIGNED;
        }

        return marked;
    }

    private List<Vector2Int> BFSPath(
        Vector2Int start,
        Vector2Int goal,
        HashSet<Vector2Int> possiblePoints)
    {
        Queue<Vector2Int> queue =
            new Queue<Vector2Int>();

        queue.Enqueue(start);

        Dictionary<Vector2Int, Vector2Int?> cameFrom = new Dictionary<Vector2Int, Vector2Int?>
        {
            [start] = null
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current.Equals(goal))
                break;

            foreach (var n in current.Neighbors())
            {
                if (!possiblePoints.Contains(n))
                    continue;

                if (!cameFrom.TryAdd(n, current))
                    continue;

                queue.Enqueue(n);
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return new List<Vector2Int>();

        // reconstruct path
        List<Vector2Int> path =
            new List<Vector2Int>();

        Vector2Int? currentPath = goal;

        while (currentPath != null)
        {
            path.Add(currentPath.Value);

            currentPath = cameFrom[currentPath.Value];
        }

        path.Reverse();

        return path;
    }

    // ================
    // TILE EXPANSION
    // ================

    private HashSet<Vector2Int> ExpandTetrisTiles(
        HashSet<Vector2Int> shape)
    {
        HashSet<Vector2Int> vertices =
            new HashSet<Vector2Int>();

        foreach (var c in shape)
        {
            vertices.Add(new Vector2Int(c.x, c.y));
            vertices.Add(new Vector2Int(c.x + 1, c.y));
            vertices.Add(new Vector2Int(c.x, c.y + 1));
            vertices.Add(new Vector2Int(c.x + 1, c.y + 1));
        }

        return vertices;
    }

    // ========================
    // HULL COMPUTATION
    // ========================

        public static List<Vector2Int> ComputeHull(HashSet<Vector2Int> points)
    {
        var segments = GetBoundarySegments(points);
        return TraceLoop(segments);
    }

    private static HashSet<(Vector2Int, Vector2Int)> GetBoundarySegments(HashSet<Vector2Int> cells)
    {
        var segments = new HashSet<(Vector2Int, Vector2Int)>();

        foreach (var c in cells)
        {
            int x = c.x, y = c.y;

            // left
            if (!cells.Contains(new Vector2Int(x - 1, y)))
                segments.Add((new Vector2Int(x, y), new Vector2Int(x, y + 1)));

            // right
            if (!cells.Contains(new Vector2Int(x + 1, y)))
                segments.Add((new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1)));

            // top
            if (!cells.Contains(new Vector2Int(x, y - 1)))
                segments.Add((new Vector2Int(x, y), new Vector2Int(x + 1, y)));

            // bottom
            if (!cells.Contains(new Vector2Int(x, y + 1)))
                segments.Add((new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1)));
        }

        return segments;
    }

    private static List<Vector2Int> TraceLoop(HashSet<(Vector2Int, Vector2Int)> segments)
    {
        var graph = new Dictionary<Vector2Int, List<Vector2Int>>();

        foreach (var (a, b) in segments)
        {
            if (!graph.ContainsKey(a)) graph[a] = new List<Vector2Int>();
            if (!graph.ContainsKey(b)) graph[b] = new List<Vector2Int>();
            graph[a].Add(b);
            graph[b].Add(a);
        }

        var start = graph.Keys.OrderBy(v => v.x).ThenBy(v => v.y).First();
        var current = start;
        Vector2Int? prev = null;

        var loop = new List<Vector2Int> { start };

        while (true)
        {
            var neighbors = graph[current];
            Vector2Int next;

            if (prev is null)
                next = neighbors[0];
            else
                next = neighbors[1] == prev.Value ? neighbors[0] : neighbors[1];

            prev = current;
            current = next;
            loop.Add(current);

            if (current == start)
                break;
        }

        return loop;
    }
}