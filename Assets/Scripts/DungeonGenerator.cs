using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class DungeonGenerator : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Vector2Int DungeonSize = new Vector2Int(100, 100);
    [SerializeField] private int NumMainRooms = 3;
    [SerializeField] private Vector2Int RoomSizeRange = new Vector2Int(3, 7);
    [SerializeField] private Vector2Int SpawnOffsetRange = new Vector2Int(3, 5);
    [SerializeField] private Vector2Int RoomsInBetween = new Vector2Int(-10, 10);
    [SerializeField] private Room RoomPrefab;
    
    [Header("Simulation")]
    [SerializeField] private bool inmediateGeneration = false;
    [SerializeField] private bool showEdges = true;

    // DATA STRUCTURE
    private int RoomCount = 0;
    private Dictionary<int, Room> MapGraph = new Dictionary<int, Room>();
    private List<Edge> Edges = new List<Edge>();
    private List<Room> CritialPath = new List<Room>();

    private void Start()
    {
        gridManager.size = DungeonSize;
        PlaceMainRooms();
        PlaceIntermediateRooms();
    }

    private void Update()
    {
        if (showEdges)  DrawEdges();
    }

    private void PlaceMainRooms()
    {
        Random rnd = new Random();
        float max = SpawnOffsetRange.x;
        float min = SpawnOffsetRange.y;
        Vector3 spawnPosition = new Vector3();
        Room firstRoom = null;
        Room previousRoom = null;
        
        for (int i = 0; i < NumMainRooms; i++)
        {
            // DEFINE ROOM OFFSET
            float offsetX = (float) (rnd.NextDouble() * (max - min)) + min;
            float offsetY = (float) (rnd.NextDouble() * (max - min)) + min;
            
            spawnPosition.Set(offsetX, offsetY, 0f);

            // CREATE ROOM
            Room room = Instantiate(RoomPrefab, transform);
            room.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            
            room.Id = RoomCount;
            RoomCount++;
            
            CritialPath.Add(room);
            MapGraph.Add(room.Id, room);
            
            // SET ROOM SIZE
            int width = rnd.Next(RoomSizeRange.x, RoomSizeRange.y);
            int height = rnd.Next(RoomSizeRange.x, RoomSizeRange.y);
            room.SetSize(width, height);

            // ADD CONNECTIONS
            if (previousRoom != null)
            {
                GameObject edgeObj = new GameObject("Edge");
                Edge edge = edgeObj.AddComponent<Edge>();
                edge.Init(previousRoom, room);

                Edges.Add(edge);
            }
            previousRoom = room;

            if (i == 0) firstRoom = room;
            if (i == NumMainRooms - 1 && NumMainRooms != 1)
            {
                GameObject edgeObj = new GameObject("Edge");
                Edge edge = edgeObj.AddComponent<Edge>();
                edge.Init(room, firstRoom);

                Edges.Add(edge);
            }
            
        }
    }
    
    private void PlaceIntermediateRooms()
    {
        for (int i = 0; i < NumMainRooms; i++)
        {
            
        }
    }

    private void AddRoomInBetweenNodes(Room A, Room B, Room newRoom)
    {
        
    }

    private void DrawEdges()
    {
        foreach (var e in Edges)
        {
            e.DrawEdge();
        }
    }
}
