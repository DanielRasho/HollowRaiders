using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    
    [SerializeField] private MapConfig cfg;
    [SerializeField] private ContentAutomata automata;
    [SerializeField] private Room_Legacy roomPrefab;

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
        
        RenderMap();
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
    public void RenderMap()
    {
        if (map == null)
        {
            Debug.LogWarning("Map is null");
            return;
        }

        if (roomPrefab == null)
        {
            Debug.LogWarning("Room prefab missing");
            return;
        }

        // =========================
        // CLEAR PREVIOUS ROOMS
        // =========================

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

        // =========================
        // EXPANDED GRID SETTINGS
        // =========================

        const int ROOM_WIDTH = 10;
        const int ROOM_HEIGHT = 7;

        const int SEPARATION = 4;

        // final spacing in world units
        float stepX = ROOM_WIDTH + SEPARATION;
        float stepY = ROOM_HEIGHT + SEPARATION;

        // =========================
        // CREATE ROOMS
        // =========================

        foreach (Room room in map.Rooms.Values)
        {
            Vector2 gridPos = room.Coords;

            // expanded grid coordinates
            Vector3 worldPos = new Vector3(
                gridPos.x * stepX,
                gridPos.y * stepY,
                0
            );

            Room_Legacy roomObj =
                Instantiate(
                    roomPrefab,
                    worldPos,
                    Quaternion.identity,
                    transform
                );

            roomObj.name =
                $"Room_{gridPos.x}_{gridPos.y}";

            roomObj.SetSize(
                ROOM_WIDTH,
                ROOM_HEIGHT
            );
        }

        Debug.Log($"Rendered {map.Rooms.Count} rooms");
    }
}
