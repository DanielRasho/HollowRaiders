using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Room_Legacy : MonoBehaviour
{
    [SerializeField] public int Id = 0;
    
    [SerializeField] private bool isStableNode = false;
    [SerializeField] private Vector2 size = new Vector2Int(1, 1);
    [SerializeField] private List<Vector2> entrances = new List<Vector2>(); 
    [SerializeField] private float padding = 1;
    
    [Header("Controllers")]
    [SerializeField] private GameObject MoveHandler;
    
    [Header("Room Assets")]
    [SerializeField] private GameObject TilesContainer;
    [SerializeField] private GameObject TilePrefab;

    private BoxCollider2D coll;
    private List<Room_Legacy> Neightbors;

    private void Start()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    public Vector3 GetRoomCenter()
    {
        return transform.position + new Vector3( size.x / 2 - 0.5f, size.y / 2 - 0.5f, 0);
    }

    public void SetSize(int width, int height)
    {
        size.x = width;
        size.y = height;
        Generate();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (PrefabUtility.IsPartOfPrefabAsset(this))
            return;

        size.x = Mathf.Max(3, size.x);
        size.y = Mathf.Max(3, size.y);

        EditorApplication.delayCall += () =>
        {
            if (this != null)
                Generate();
        };
    }
#endif
    
    void Generate()
    {
        if (TilesContainer == null || TilePrefab == null) return;

        // Ensure collider exists
        if (coll == null)
            coll = GetComponent<BoxCollider2D>();

        if (coll == null)
            coll = gameObject.AddComponent<BoxCollider2D>();

        // Clear old tiles
        for (int i = TilesContainer.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(TilesContainer.transform.GetChild(i).gameObject);
        }
        
        
        var tile = Instantiate(TilePrefab, TilesContainer.transform);
        tile.transform.localScale = new Vector3(size.x, size.y, 1);
        tile.transform.localPosition = new Vector3(
            size.x / 2f - 0.5f, 
            size.y / 2f - 0.5f, 
            0);
        
        // Create tiles
        /*
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var tile = Instantiate(TilePrefab, TilesContainer.transform);
                tile.transform.localPosition = new Vector3(x, y, 0);
            }
        }*/

        // Update collider
        coll.size = size + new Vector2(padding * 2, padding * 2);
        coll.offset = new Vector2(
            (size.x ) / 2f - 0.5f, 
            (size.y ) / 2f - 0.5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.pink;
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetRoomCenter(), 0.1f);
    }
}
