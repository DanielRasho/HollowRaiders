using System;
using Unity.VectorGraphics;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject Levels;
    [SerializeField] private AudioClip music;

    private void Start()
    {
        SceneGameManager.Instance.InsertSlot(SceneDatabase.Scenes.MainMenu, SceneDatabase.Scenes.MainMenu);
        AudioManager.Instance.PlayMusic(music, restart:true);
        showMainMenu();
    }

    public void showLevels()
    {
        MainMenu.SetActive(false);
        Levels.SetActive(true);
    }
    
    public void showMainMenu()
    {
        MainMenu.SetActive(true);
        Levels.SetActive(false);
    }

    public void OnExit()
    {
        Application.Quit();
    }
    
    public void OnPlayLvl(int lvl)
    {
        string level = lvl switch
        {
            1 => SceneDatabase.Scenes.Lvl1,
            2 => SceneDatabase.Scenes.Lvl2,
            3 => SceneDatabase.Scenes.Lvl3,
            _ => null
        };

        if (level == null)
            return;
        
        var uwu = SceneGameManager.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(level, level)
            .Load(SceneDatabase.Scenes.LevelMenu, SceneDatabase.Scenes.LevelMenu, true)
            .Unload(SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }
}
