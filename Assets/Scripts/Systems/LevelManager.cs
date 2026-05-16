using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private AudioClip BattleMusic;

    private void Awake()
    {
        // Singleton — survive scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Input_Manager.Instance.SwitchToMap("Player");
        AudioManager.Instance.PlayMusic(BattleMusic, true);
    }
}
