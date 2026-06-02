using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogSceneChange : DialogActivator, IInteractable
{
    public override void OnDialogueEnd()
    {
        SceneGameManager.Instance
            .NewTransition()
            .Unload(SceneDatabase.Scenes.Intro)
            .Load(SceneDatabase.Scenes.Game, SceneDatabase.Scenes.Game)
            .WithOverlay()
            .Perform();
    }
}
