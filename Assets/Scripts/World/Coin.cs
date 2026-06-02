using UnityEngine;

public class Coin : MonoBehaviour, IPickable
{
    [SerializeField] private int value = 30;
    [SerializeField] private AudioClip sfx;
    
    public bool canPick()
    {
        return false;
    }

    public bool canAutoPick()
    {
        return true;
    }

    public void Pick()
    {
        AudioManager.Instance.PlayFX(sfx);
        LevelManager.Instance?.IncreaseMoney(value);
        Destroy(gameObject);
    }
}
