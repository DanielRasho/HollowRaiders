    using System.Collections;
    using UnityEngine;

public class Enemy : MonoBehaviour, IEnemy
{
    [Header("Damage")]
    [SerializeField] private Vector2Int value = new(5, 11);
    [SerializeField] private AudioClip sfx;

    [Header("Movement")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Min and max seconds before changing direction")]
    [SerializeField] private Vector2 moveDurationRange = new(1f, 3f);

    private void Start()
    {
        StartCoroutine(RandomMovementRoutine());
    }

    public void Hurt()
    {
        AudioManager.Instance.PlayFX(sfx);

        LevelManager.Instance?.DecreaseHealth(
            Random.Range(value.x, value.y)
        );
    }

    private IEnumerator RandomMovementRoutine()
    {
        while (true)
        {
            Vector2 direction = GetRandomCardinalDirection();

            float duration = Random.Range(
                moveDurationRange.x,
                moveDurationRange.y);

            rb.linearVelocity = direction * moveSpeed;

            yield return new WaitForSeconds(duration);
        }
    }

    private Vector2 GetRandomCardinalDirection()
    {
        switch (Random.Range(0, 4))
        {
            case 0:
                return Vector2.up;

            case 1:
                return Vector2.down;

            case 2:
                return Vector2.left;

            default:
                return Vector2.right;
        }
    }

    private void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}