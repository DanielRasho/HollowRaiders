using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private int seed; // TODO: To control randomness
    [SerializeField] private MapConfig cfg;
    [SerializeField] private ContentAutomata automata;
    [SerializeField] private RoomsDB roomDatabase;
    [SerializeField] private CorridorsDB corridorDB;

    private DungeonGenerator generator;
    private Map map;
    private Transform spawnPoint;
    public Transform SpawnPoint { get => spawnPoint; }
    
    public int mapWidth = 1;
    public int mapHeight = 1;
    public Vector2 mapOrigin;
    public Vector2 MapOrigin => mapOrigin;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Bounds dungeonBounds = new Bounds(
            transform.position,
            new Vector3(mapWidth, mapHeight, 0)
        );

        Gizmos.DrawWireCube(dungeonBounds.center, dungeonBounds.size);
    }

    public int ShortcutCount()
    {
        return map != null ? map.Shortcuts.Count : 0;
    } 
    
    private void EnsureInitialized()
    {
        generator ??= new DungeonGenerator(cfg);
    }
    void Start()
    {
    }

    public void Generate()
    {
        EnsureInitialized();
        // Generate Tetris texture
        TetrisTilemap tilemap = new TetrisTilemap(this.cfg);
        Debug.Log("Generate Tetris texture");
        tilemap.Generate();
        Debug.Log("Build Chain");
        List<int> chain = tilemap.BuildChain();
        Debug.Log("Filter Chain");
        tilemap = tilemap.FilterToChain(chain);

        Debug.Log("GENERATING MAP");
        // Generate Map
        map = generator.Generate(tilemap,automata);
    }

    public void ResetMap()
    {
        map = null;
        if (transform != null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                #if UNITY_EDITOR
                DestroyImmediate(transform.GetChild(i).gameObject);
                #else
                Destroy(transform.GetChild(i).gameObject);
                #endif
            }
        }
    }
    
    public void ModifyMap()
    {
        // Select random shortcuts
        List<int> selectedShorcuts = Randomness.PickRandomIndexes(
            map.Shortcuts.Count, 
            Math.Min(map.Shortcuts.Count, cfg.max_active_shorcuts));

        // Iterate over each shortcut
        for (int i = 0; i < map.Shortcuts.Count; i++)
        {
            MapShortcut s = map.Shortcuts[i];
            bool shouldActivate = selectedShorcuts.Contains(i);
            map.ActivateShortcut(s, shouldActivate);
        }
    }

    public void RenderMap()
    {
        CalculateMapBounds(
            out int minX,
            out int maxX,
            out int minY,
            out int maxY
        );

        // PLACE ROOMS
        foreach (Room room in map.Rooms.Values)
        {
            Vector2Int p = GetRoomPositionInWorld(room, minX, minY);

            RoomView roomView = roomDatabase.FindMatch(
                    cfg.RoomSize.x,
                    cfg.RoomSize.y,
                    room.Type);

            if (roomView != null)
            {
                RoomView instance =
                    Instantiate(
                        roomView,
                        new Vector3(p.x, p.y, 0),
                        Quaternion.identity,
                        transform
                    );

                room.View = instance;
                room.StartView();

                if (room.Type == RoomType.START)
                {
                    spawnPoint = room.View.Spawnpoint;
                }
            }
        }
        
        foreach (Corridor corridor in map.Corridors.Values)
        {
            Room a = corridor.A;
            Room b = corridor.B;
            
            Vector2Int ap = a.Coords;
            Vector2Int bp = b.Coords;

            int x = 0;
            int y = 0;
            CorridorView prefab = null;
            
            if (corridor.Type == CorridorType.HORIZONTAL)
            {
                Room room = corridor.GetLeftMost();
                Vector2Int originPos = GetRoomPositionInWorld(room, minX, minY);

                x = originPos.x + cfg.RoomSize.x;
                y = originPos.y + (cfg.RoomSize.y / 2) - (cfg.HorizontalCorridorSize.y / 2);
                
                prefab = corridorDB.FindMatch(cfg.HorizontalCorridorSize.x, 
                    cfg.HorizontalCorridorSize.y, CorridorType.HORIZONTAL);
            }
            else
            {
                Room room = corridor.GetBottomMost();
                Vector2Int originPos = GetRoomPositionInWorld(room, minX, minY);

                x = originPos.x + (cfg.RoomSize.x / 2) - (cfg.HorizontalCorridorSize.x / 2) + 2;
                y = originPos.y + cfg.RoomSize.y;
                
                prefab = corridorDB.FindMatch(cfg.VerticalCorridorSize.y, 
                    cfg.VerticalCorridorSize.x, CorridorType.VERTICAL);
            }

            Vector3 corridorPosition = new Vector3(x, y, 0);
            if (prefab != null)
            {
                CorridorView view = Instantiate(
                    prefab,
                    corridorPosition,
                    Quaternion.identity,
                    transform
                );
                corridor.View = view;
                corridor.UpdateView();
            }
        }
    }

    private Vector2Int GetRoomPositionInWorld(
        Room room,
        int minX,
        int minY)
    {
        int cellWidth =
            cfg.RoomSize.x +
            cfg.HorizontalCorridorSize.x;

        int cellHeight =
            cfg.RoomSize.y +
            cfg.VerticalCorridorSize.x;

        float x =
            mapOrigin.x +
            (room.Coords.x - minX) * cellWidth;

        float y =
            mapOrigin.y +
            (room.Coords.y - minY) * cellHeight;

        return new Vector2Int(
            Mathf.RoundToInt(x),
            Mathf.RoundToInt(y)
        );
    }
    
    private void CalculateMapBounds(
        out int minX,
        out int maxX,
        out int minY,
        out int maxY)
    {
        minX = int.MaxValue;
        maxX = int.MinValue;

        minY = int.MaxValue;
        maxY = int.MinValue;

        foreach (Room room in map.Rooms.Values)
        {
            Vector2Int p = room.Coords;

            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);

            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        int logicalWidth = maxX - minX + 1;
        int logicalHeight = maxY - minY + 1;

        int cellWidth =
            cfg.RoomSize.x +
            cfg.HorizontalCorridorSize.x;

        int cellHeight =
            cfg.RoomSize.y +
            cfg.VerticalCorridorSize.x;

        mapWidth =
            (logicalWidth - 1) * cellWidth +
            cfg.RoomSize.x;

        mapHeight =
            (logicalHeight - 1) * cellHeight +
            cfg.RoomSize.y;

        mapOrigin = (Vector2)transform.position -
                    new Vector2(mapWidth, mapHeight) * 0.5f;
    }
    
    public void ExportAsciiMap(string fileName = "dungeon_map.txt")
    {
        // =====================================
        // FIND MAP BOUNDS
        // =====================================

        int minX = int.MaxValue;
        int maxX = int.MinValue;

        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (Room room in map.Rooms.Values)
        {
            Vector2Int p = room.Coords;

            minX = Mathf.Min(minX, (int)p.x);
            maxX = Mathf.Max(maxX, (int)p.x);

            minY = Mathf.Min(minY, (int)p.y);
            maxY = Mathf.Max(maxY, (int)p.y);
        }

        // =====================================
        // EXPANDED ASCII GRID
        // each logical node occupies:
        //
        // node - corridor - node
        //
        // so final size becomes:
        // width  = logicalWidth  * 2 - 1
        // height = logicalHeight * 2 - 1
        // =====================================

        int width = (maxX - minX + 1) * 2 - 1;

        int height = (maxY - minY + 1) * 2 - 1;

        char[,] grid = new char[height, width];

        // fill empty
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[y, x] = ' ';
            }
        }

        // =====================================
        // PLACE ROOMS
        // =====================================

        foreach (Room room in map.Rooms.Values)
        {
            Vector2Int p = room.Coords;

            int gx = ((int)p.x - minX) * 2;
            int gy = ((int)p.y - minY) * 2;

            // invert y for text output
            gy = height - 1 - gy;

            grid[gy, gx] = '■';
        }

        // =====================================
        // PLACE CONNECTIONS
        // =====================================

        HashSet<string> drawn = new HashSet<string>();

        foreach (Room room in map.Rooms.Values)
        {
            Vector2Int a = room.Coords;

            foreach (Corridor corridor in room.Connections)
            {
                Room other =
                    corridor.A == room
                        ? corridor.B
                        : corridor.A;

                Vector2Int b = other.Coords;

                // prevent drawing twice
                string edgeId =
                    a.x < b.x || (a.x == b.x && a.y < b.y)
                        ? $"{a}-{b}"
                        : $"{b}-{a}";

                if (drawn.Contains(edgeId))
                    continue;

                drawn.Add(edgeId);

                int ax = (a.x - minX) * 2;
                int ay = (a.y - minY) * 2;

                int bx = (b.x - minX) * 2;
                int by = (b.y - minY) * 2;

                ay = height - 1 - ay;
                by = height - 1 - by;

                int mx = (ax + bx) / 2;
                int my = (ay + by) / 2;

                // horizontal corridor
                if (ay == by)
                {
                    grid[my, mx] = '─';
                }
                // vertical corridor
                else if (ax == bx)
                {
                    grid[my, mx] = '│';
                }
            }
        }

        // =====================================
        // BUILD STRING
        // =====================================

        StringBuilder sb = new StringBuilder();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(grid[y, x]);
            }

            sb.AppendLine();
        }

        // =====================================
        // WRITE FILE
        // =====================================

        string path =
            Path.Combine(
                Application.dataPath,
                fileName
            );

        File.WriteAllText(path, sb.ToString());

        Debug.Log($"ASCII map exported to:\n{path}");
    }
}
