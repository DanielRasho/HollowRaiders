using System;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public static IntroManager Instance { get; private set; }
    [SerializeField] private AudioClip backgroundMusic;
    
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
        AudioManager.Instance.PlayMusic(backgroundMusic, restart:true);
    }
    
}
