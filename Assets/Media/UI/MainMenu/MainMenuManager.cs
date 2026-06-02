using System;
using Unity.VectorGraphics;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private AudioClip music;

    private void Start()
    {
        SceneGameManager.Instance.InsertSlot(SceneDatabase.Scenes.MainMenu, SceneDatabase.Scenes.MainMenu);
        AudioManager.Instance.PlayMusic(music, restart:true);
        showMainMenu();
    }

    public void showMainMenu()
    {
        MainMenu.SetActive(true);
    }

    public void OnExit()
    {
        Application.Quit();
    }
    
    public void Play()
    {
        SceneGameManager.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Scenes.Intro, SceneDatabase.Scenes.Intro, true)
            .Unload(SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }
}
