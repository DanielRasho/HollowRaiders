using System.Collections;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private FreeMovement Player;

    private bool isInvulnerable;


    private void OnTriggerStay2D(Collider2D other)
    {
        if (isInvulnerable)
            return;

        if (other.TryGetComponent(out IEnemy enemy))
        {
            enemy.Hurt();

            ApplyKnockback(enemy.transform);

            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    private void ApplyKnockback(Transform enemy)
    {
        Vector2 direction =
            (transform.position - enemy.transform.position).normalized;

        Player.ApplyKnockback(
            direction,
            knockbackForce,
            0.2f
        );
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        yield return new WaitForSeconds(invulnerabilityTime);

        isInvulnerable = false;
    }
}