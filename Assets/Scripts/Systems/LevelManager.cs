using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private List<AudioClip> BattleSountracks;
    [SerializeField] private DungeonManager _dungeonManager;
    public static event Action<Transform> OnSpawnPlayer;

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
        Input_Manager.Instance.SwitchToMap("Player");
        AudioClip music = BattleSountracks[Random.Range(0, BattleSountracks.Count)];
        AudioManager.Instance.PlayMusic(music, true);
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Pointer);
        
        // Map Generation
        _dungeonManager.Generate();
        _dungeonManager.RenderMap();
        
        // Place Player
        Transform spawnPoint = _dungeonManager.SpawnPoint;
        OnSpawnPlayer?.Invoke(spawnPoint);
    }
}
