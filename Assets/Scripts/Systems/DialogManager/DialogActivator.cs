using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogActivator : MonoBehaviour, IInteractable
{
    [Header("Globe")]
    [SerializeField] private Animator bubbleAnimator;
    [SerializeField] private bool showGlobe = true;
    
    [Header("Dialogue")]
    public List<DialogLine> lines = new();

    [SerializeField] private bool shouldIncreaseMissioncount = false;

    private static readonly int IsActiveHash = Animator.StringToHash("isActive");
    public static event Action<DialogActivator> OnStartDialogue;

    private bool missionVisited;

    public bool CanInteract()
    {
        return true;
    }
    
    public void Interact()
    {
        OnStartDialogue?.Invoke(this);

        if (shouldIncreaseMissioncount && !missionVisited)
        {
            LevelManager.Instance?.IncreaseMissionCount();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (showGlobe && other.CompareTag("Player"))
        {
            bubbleAnimator.SetBool(IsActiveHash, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (showGlobe && other.CompareTag("Player"))
        {
            bubbleAnimator.SetBool(IsActiveHash, false);
        }
    }
}
