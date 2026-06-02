using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private List<AudioClip> BattleSountracks;
    [SerializeField] private DungeonManager _dungeonManager;
    
    [Header("Debugging")]
    [SerializeField] private bool positionPlayer = true;

    [Header("Health & Money")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Image healthbarTexture;
    [SerializeField] private Image DelayHealthbarTexture;
    [SerializeField] private float HealthDecreaseDelayAnimation = 0.4f;
    [SerializeField] private TextMeshProUGUI MoneyText;

    [Header("Missions")] 
    [SerializeField] private TextMeshProUGUI MissionText;
    
    private int totalMissions = 0;
    private int missionsCompleted = 0;
    
    private float _health;
    private int _money;
    
    // STATE
    
    // EVENTS
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
        _health = maxHealth;
        healthbarTexture.fillAmount = 1f;
        DelayHealthbarTexture.fillAmount = 1f;
        Input_Manager.Instance.SwitchToMap(InputMap.PLAYER);
        AudioClip music = BattleSountracks[Random.Range(0, BattleSountracks.Count)];
        AudioManager.Instance.PlayMusic(music, true);
        CursorManager.Instance.SetCursor(CursorManager.CursorType.Pointer);
        
        // Map Generation
        _dungeonManager.Generate();
        _dungeonManager.RenderMap();
        totalMissions = _dungeonManager.MissionCount(); // get total missions to complete for this run
        UpdateMissionCountUI();
        
        // Place Player
        if (positionPlayer)
            OnSpawnPlayer?.Invoke(_dungeonManager.SpawnPoint);
        
        // Generate MiniMap
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

    public void IncreaseMoney(int amount)
    {
        _money += amount;
        MoneyText.text = _money.ToString();
    }

    public void DecreaseHealth(int amount)
    {
        _health = Math.Max(_health - amount, 0);
        
        float ratio = _health / maxHealth;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(healthbarTexture
            .DOFillAmount(ratio, 0.25f))
            .SetEase(Ease.InOutSine);
        sequence.AppendInterval(HealthDecreaseDelayAnimation);
        sequence.Append(DelayHealthbarTexture
                .DOFillAmount(ratio, 0.3f))
            .SetEase(Ease.InOutSine);
        
        sequence.OnComplete(() =>
        {
            if (_health == 0)
            {
                SceneGameManager.Instance
                    .NewTransition()
                    .Load(SceneDatabase.Scenes.MainMenu, SceneDatabase.Scenes.MainMenu)
                    .Unload(SceneDatabase.Scenes.Game)
                    .WithOverlay()
                    .Perform();
            }
        });

        sequence.Play();
    }

    public void IncreaseMissionCount()
    {
        missionsCompleted++;
        UpdateMissionCountUI();
        
        if (missionsCompleted == totalMissions)
            SceneGameManager.Instance
                .NewTransition()
                .Load(SceneDatabase.Scenes.Credits, SceneDatabase.Scenes.Credits)
                .Unload(SceneDatabase.Scenes.Game)
                .WithOverlay()
                .Perform();
        
        _dungeonManager.ModifyMap();
    }

    private void UpdateMissionCountUI()
    {
        string text = missionsCompleted + "/" + totalMissions;
        MissionText.text = text;
    }
    
}
