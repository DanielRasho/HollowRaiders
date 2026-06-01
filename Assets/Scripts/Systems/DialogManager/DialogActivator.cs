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

    private static readonly int IsActiveHash = Animator.StringToHash("isActive");
    public static event Action<DialogActivator> OnStartDialogue;

    public bool CanInteract()
    {
        return true;
    }
    
    public void Interact()
    {
        OnStartDialogue?.Invoke(this);
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
