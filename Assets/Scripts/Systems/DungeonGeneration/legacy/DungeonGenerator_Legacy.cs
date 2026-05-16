using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class DungeonGenerator_Legacy : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Vector2Int DungeonSize = new Vector2Int(100, 100);
    [SerializeField] private int NumMainRooms = 3;
    [SerializeField] private Vector2Int RoomSizeRange = new Vector2Int(3, 7);
    [SerializeField] private Vector2Int SpawnOffsetRange = new Vector2Int(3, 5);
    [SerializeField] private Vector2Int RoomsInBetween = new Vector2Int(-10, 10);
    [FormerlySerializedAs("RoomPrefab")] [SerializeField] private Room_Legacy roomLegacyPrefab;
    
    [Header("Simulation")]
    [SerializeField] private bool inmediateGeneration = false;
    [SerializeField] private bool showEdges = true;

    // DATA STRUCTURE
    private int RoomCount = 0;
    private Dictionary<int, Room_Legacy> MapGraph = new Dictionary<int, Room_Legacy>();
    private List<Edge_Legacy> Edges = new List<Edge_Legacy>();
    private List<Room_Legacy> CritialPath = new List<Room_Legacy>();

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
        Room_Legacy firstRoomLegacy = null;
        Room_Legacy previousRoomLegacy = null;
        
        for (int i = 0; i < NumMainRooms; i++)
        {
            // DEFINE ROOM OFFSET
            float offsetX = (float) (rnd.NextDouble() * (max - min)) + min;
            float offsetY = (float) (rnd.NextDouble() * (max - min)) + min;
            
            spawnPosition.Set(offsetX, offsetY, 0f);

            // CREATE ROOM
            Room_Legacy roomLegacy = Instantiate(roomLegacyPrefab, transform);
            roomLegacy.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            
            roomLegacy.Id = RoomCount;
            RoomCount++;
            
            CritialPath.Add(roomLegacy);
            MapGraph.Add(roomLegacy.Id, roomLegacy);
            
            // SET ROOM SIZE
            int width = rnd.Next(RoomSizeRange.x, RoomSizeRange.y);
            int height = rnd.Next(RoomSizeRange.x, RoomSizeRange.y);
            roomLegacy.SetSize(width, height);

            // ADD CONNECTIONS
            if (previousRoomLegacy != null)
            {
                GameObject edgeObj = new GameObject("Edge");
                Edge_Legacy edgeLegacy = edgeObj.AddComponent<Edge_Legacy>();
                edgeLegacy.Init(previousRoomLegacy, roomLegacy);

                Edges.Add(edgeLegacy);
            }
            previousRoomLegacy = roomLegacy;

            if (i == 0) firstRoomLegacy = roomLegacy;
            if (i == NumMainRooms - 1 && NumMainRooms != 1)
            {
                GameObject edgeObj = new GameObject("Edge");
                Edge_Legacy edgeLegacy = edgeObj.AddComponent<Edge_Legacy>();
                edgeLegacy.Init(roomLegacy, firstRoomLegacy);

                Edges.Add(edgeLegacy);
            }
            
        }
    }
    
    private void PlaceIntermediateRooms()
    {
        for (int i = 0; i < NumMainRooms; i++)
        {
            
        }
    }

    private void AddRoomInBetweenNodes(Room_Legacy A, Room_Legacy B, Room_Legacy newRoomLegacy)
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
