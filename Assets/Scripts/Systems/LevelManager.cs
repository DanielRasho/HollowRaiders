using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private List<AudioClip> BattleSountracks;
    [SerializeField] private DungeonManager _dungeonManager;
    public static event Action<Transform> OnSpawnPlayer;
    public static event Action<Vector2, int, int> OnStartMinimapGeneration;
    public static event Action OnShowMap;

    private void Awake()
    {
        // Singleton — survive scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() 
    {
        Input_Manager.Instance.SwitchToMap(InputMap.PLAYER);
        AudioClip music = BattleSountracks[Random.Range(0, BattleSountracks.Count)];
        AudioManager.Instance.PlayMusic(music, true);
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Pointer);
        
        // Map Generation
        _dungeonManager.Generate();
        _dungeonManager.RenderMap();
        
        // Place Player
        Transform spawnPoint = _dungeonManager.SpawnPoint;
        OnSpawnPlayer?.Invoke(spawnPoint);
        
        // Generate Map
        Vector2 mapCenter = new Vector2(
            _dungeonManager.MapOrigin.x + _dungeonManager.mapWidth * 0.5f,
            _dungeonManager.MapOrigin.y + _dungeonManager.mapHeight * 0.5f
            );
        OnStartMinimapGeneration?.Invoke(mapCenter, 
            _dungeonManager.mapWidth, 
            _dungeonManager.mapHeight);
        
        // Set controls
        Input_Manager.Instance.Actions.Player.Map.performed += ShowMap;
    }

    private void OnDestroy()
    {
        Input_Manager.Instance.Actions.Player.Map.performed -= ShowMap;
    }

    public void ShowMap(InputAction.CallbackContext ctx)
    {
        Input_Manager.Instance.SwitchToMap(InputMap.MAP);
        OnShowMap?.Invoke();
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Default);
    }
}
