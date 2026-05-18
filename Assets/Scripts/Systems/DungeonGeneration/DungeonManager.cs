using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    
    [SerializeField] private MapConfig cfg;
    [SerializeField] private ContentAutomata automata;
    [SerializeField] private Room_Legacy roomPrefab;
    [SerializeField] private Room_Legacy corridorPrefab;

    private DungeonGenerator generator;
    private Map map;
    
    private void EnsureInitialized()
    {
        generator ??= new DungeonGenerator(cfg);
    }
    void Start()
    {
        generator = new DungeonGenerator(cfg);
    }

    public void Generate()
    {
        EnsureInitialized();
        // Generate Tetris texture
        TetrisTilemap tilemap = new TetrisTilemap(this.cfg);
        tilemap.Generate();
        List<int> chain = tilemap.BuildChain();
        tilemap = tilemap.FilterToChain(chain);

        // Generate Map
        map = generator.Generate(tilemap);
        
        // RenderMap();
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
            Destroy(roomContainer.GetChild(i).gameObject);
#endif
            }
        }
    }
    
    public void ModifyMap()
    {
        
    }
    
    public void ExportAsciiMap(string fileName = "dungeon_map.txt")
    {
        if (map == null)
        {
            Debug.LogWarning("Map is null");
            return;
        }

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

        int width =
            (maxX - minX + 1) * 2 - 1;

        int height =
            (maxY - minY + 1) * 2 - 1;

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
