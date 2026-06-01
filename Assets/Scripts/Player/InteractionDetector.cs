using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;

    private void Start()
    {
        Input_Manager.Instance.Actions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        Input_Manager.Instance.Actions.Player.Interact.performed -= OnInteract;
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        interactableInRange?.Interact();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
            interactableInRange = interactable;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
            interactableInRange = null;
    }
}
