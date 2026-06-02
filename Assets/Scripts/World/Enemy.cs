using UnityEngine;

public class Enemy : MonoBehaviour, IEnemy
{
    [SerializeField] private Vector2Int value = new Vector2Int(5, 11);
    [SerializeField] private AudioClip sfx;

    public void Hurt()
    {
        AudioManager.Instance.PlayFX(sfx);
        LevelManager.Instance?.DecreaseHealth(Random.Range(value.x, value.y));
    }
}
