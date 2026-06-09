using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossEnemyMeleeHitbox : MonoBehaviour
{
    public int damage = 25;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent(out PlayerHealth player))
            player.TakeDamage(damage);
    }
}
