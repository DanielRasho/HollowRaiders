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

        Dictionary<Vector2, List<int>> allPoints = new Dictionary<Vector2, List<int>>();

        int cycleId = 0;

        foreach (var tile in tilemap.Tiles.Values)
        {
            HashSet<Vector2> points = ExpandTetrisTiles(tile.Cells);

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

            // Create rooms
            List<Vector2> loop = ComputeHull(tile.Cells);

            List<Room> rooms = new List<Room>();

            foreach (var p in loop)
            {
                rooms.Add(
                    new Room(RoomType.UNASSIGNED, p, true)
                );
            }

            map.AddCycle(cycleId, rooms, points);

            cycleId++;
        }

        // Place start points and mission points
        DefineLandmarks(map);

        // Create shortcuts
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

            while (true)
            {
                Vector2 start = cycle.Rooms[UnityEngine.Random.Range(0, cycle.Rooms.Count)];
                Vector2 end = GetOpositeRoom(map, id, start, null);
                List<Vector2> path = BFSPath(start, end, cycle.Rooms.ToHashSet());
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

        Vector2 currentCoords = map.Cycles[currentCycle]
                .Rooms[UnityEngine.Random.Range(
                    0,
                    map.Cycles[currentCycle].Rooms.Count)];

        Vector2 initialCoords = currentCoords;

        HashSet<int> visitedCycles = new HashSet<int>();

        HashSet<Vector2> markedRooms =
            new HashSet<Vector2> { currentCoords };

        List<Vector2> roomsOnCycle =
            new List<Vector2> { currentCoords };

        while (true)
        {
            Vector2 opositeRoomCoords = currentCoords;

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

    private Vector2 GetOpositeRoom(
        Map map,
        int cycleId,
        Vector2 point,
        HashSet<Vector2> markedPoints)
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

    private Vector2 GetFurthestPoint(
        List<Vector2> candidates,
        List<Vector2> currentPoints,
        HashSet<Vector2> markedPoints)
    {
        if (candidates.Count == 0)
            throw new Exception("Candidates list is empty");

        if (currentPoints.Count == 0)
            return candidates[0];

        int bestCandidateIdx = -1;
        float bestDistance = -1f;

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2 candidate = candidates[i];

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

    private HashSet<Vector2> ReduceMarkedPoints(
        Map map,
        HashSet<Vector2> markedPoints,
        int maxMarkedPoints)
    {
        if (maxMarkedPoints <= 0)
            return new HashSet<Vector2>();

        HashSet<Vector2> marked = new HashSet<Vector2>(markedPoints);

        HashSet<Vector2> possiblePoints = new HashSet<Vector2>(map.Rooms.Keys);

        while (marked.Count > maxMarkedPoints)
        {
            List<Vector2> markedList =
                marked.ToList();

            (Vector2, Vector2)? bestPair = null;

            float bestDist = float.PositiveInfinity;

            // FIND CLOSEST VALID PAIR

            for (int i = 0; i < markedList.Count; i++)
            {
                for (int j = i + 1; j < markedList.Count; j++)
                {
                    Vector2 a = markedList[i];
                    Vector2 b = markedList[j];

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

            Vector2 pa = bestPair.Value.Item1;
            Vector2 pb = bestPair.Value.Item2;

            Room roomPa = map.Rooms[pa];
            Room roomPb = map.Rooms[pb];

            // PATH MIDPOINT

            List<Vector2> path = BFSPath(pa, pb, possiblePoints);

            if (path.Count == 0)
            {
                Vector2 victim = roomPb.Type != RoomType.START ? pb : pa;

                marked.Remove(victim);

                map.Rooms[victim].Type =
                    RoomType.UNASSIGNED;

                continue;
            }


            // DEGENERATE MIDPOINT FIX
            Vector2 midpoint = path[path.Count / 2];
            if (midpoint == pa || midpoint == pb)
            {
                bool found = false;
                Vector2 replacement = default;

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
                    Vector2 victim = roomPb.Type != RoomType.START ? pb : pa;

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

    private List<Vector2> BFSPath(
        Vector2 start,
        Vector2 goal,
        HashSet<Vector2> possiblePoints)
    {
        Queue<Vector2> queue =
            new Queue<Vector2>();

        queue.Enqueue(start);

        Dictionary<Vector2, Vector2?> cameFrom = new Dictionary<Vector2, Vector2?>
        {
            [start] = null
        };

        while (queue.Count > 0)
        {
            Vector2 current = queue.Dequeue();

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
            return new List<Vector2>();

        // reconstruct path
        List<Vector2> path =
            new List<Vector2>();

        Vector2? currentPath = goal;

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

    private HashSet<Vector2> ExpandTetrisTiles(
        HashSet<Vector2> shape)
    {
        HashSet<Vector2> vertices =
            new HashSet<Vector2>();

        foreach (var c in shape)
        {
            vertices.Add(new Vector2(c.x, c.y));
            vertices.Add(new Vector2(c.x + 1, c.y));
            vertices.Add(new Vector2(c.x, c.y + 1));
            vertices.Add(new Vector2(c.x + 1, c.y + 1));
        }

        return vertices;
    }

    // ========================
    // HULL COMPUTATION
    // ========================

    private List<Vector2> ComputeHull(
        HashSet<Vector2> points)
    {
        HashSet<(Vector2, Vector2)> segments =
            GetBoundarySegments(points);

        List<Vector2> loop =
            TraceLoop(segments);

        return loop;
    }

    private HashSet<(Vector2, Vector2)> GetBoundarySegments(
        HashSet<Vector2> cells)
    {
        HashSet<(Vector2, Vector2)> segments =
            new HashSet<(Vector2, Vector2)>();

        foreach (var c in cells)
        {
            float x = c.x;
            float y = c.y;

            // left
            if (!cells.Contains(new Vector2(x - 1, y)))
            {
                segments.Add((
                    new Vector2(x, y),
                    new Vector2(x, y + 1)
                ));
            }

            // right
            if (!cells.Contains(new Vector2(x + 1, y)))
            {
                segments.Add((
                    new Vector2(x + 1, y),
                    new Vector2(x + 1, y + 1)
                ));
            }

            // top
            if (!cells.Contains(new Vector2(x, y - 1)))
            {
                segments.Add((
                    new Vector2(x, y),
                    new Vector2(x + 1, y)
                ));
            }

            // bottom
            if (!cells.Contains(new Vector2(x, y + 1)))
            {
                segments.Add((
                    new Vector2(x, y + 1),
                    new Vector2(x + 1, y + 1)
                ));
            }
        }

        return segments;
    }

    private List<Vector2> TraceLoop(
        HashSet<(Vector2, Vector2)> segments)
    {
        Dictionary<Vector2, List<Vector2>> graph =
            new Dictionary<Vector2, List<Vector2>>();

        foreach (var seg in segments)
        {
            Vector2 a = seg.Item1;
            Vector2 b = seg.Item2;

            if (!graph.ContainsKey(a)) graph[a] = new List<Vector2>();
            if (!graph.ContainsKey(b)) graph[b] = new List<Vector2>();

            graph[a].Add(b);
            graph[b].Add(a);
        }

        Vector2 start =
            graph.Keys
                .OrderBy(v => v.x)
                .ThenBy(v => v.y)
                .First();

        Vector2 current = start;
        Vector2? prev = null;

        List<Vector2> loop = new List<Vector2> { start };

        while (true)
        {
            List<Vector2> neighbors = graph[current];

            Vector2 next;

            if (prev == null)
            {
                next = neighbors[0];
            }
            else
            {
                next = neighbors[0].Equals(prev)
                        ? neighbors[1]
                        : neighbors[0];
            }

            prev = current;
            current = next;

            loop.Add(current);

            if (current.Equals(start))
                break;
        }

        return loop;
    }
}