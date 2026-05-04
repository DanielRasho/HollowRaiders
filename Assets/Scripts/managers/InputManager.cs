using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Input_Manager : MonoBehaviour
{
    public static Input_Manager Instance { get; private set; }

    private GameInputActions _actions;
    
    public GameInputActions Actions => _actions;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        _actions = new GameInputActions();
    }

    private void OnEnable()
    {
        _actions.Enable();
    }
    
    private void OnDisable()
    {
        _actions.Disable();
    }

    // Replace the context switch section
    public void SwitchToMap(string mapName)
    {
        foreach (var map in _actions.asset.actionMaps)
        {
            if (map.name == mapName)
                map.Enable();
            else
                map.Disable();
        }
    }
}
