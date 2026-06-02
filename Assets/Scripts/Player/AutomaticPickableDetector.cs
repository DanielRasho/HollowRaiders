using UnityEngine;
using UnityEngine.InputSystem;

public class AutomaticPickableDetector : MonoBehaviour
{
    private IPickable pickableInRange = null;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        pickableInRange?.Pick();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IPickable interactable) && interactable.canAutoPick())
        {
            Debug.Log("Something picked");
            if (interactable.canAutoPick())
            {
                interactable.Pick();
            }
            else
            {
                pickableInRange = interactable;
            }
        }
            
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
            pickableInRange = null;
    }
}
