using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Stat")]
    public int damage = 25;
    public float knockbackForce = 5f;

    public abstract void Attack();

    // Khi Player nhặt vũ khí sẽ gọi hàm này
    public virtual void OnEquip()
    {
    }

    protected void DamageEnemy(Collider2D other)
    {
        // Enemy
        if (other.TryGetComponent<EnemyHeath>(out var enemy))
        {
            enemy.TakeDamage(damage);

            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir =
                    (other.bounds.center - transform.position).normalized;

                rb.linearVelocity = Vector2.zero;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }

            return;
        }

        // Boss
        if (other.TryGetComponent<BossHeath>(out var boss))
        {
            boss.TakeDamage(damage);
            return;
        }

        // Gạch phá được
        if (other.TryGetComponent<BreakableTile>(out var tile))
        {
            tile.TakeDamage(damage);
            return;
        }

        // Bom
        if (other.TryGetComponent<Bomb>(out var bomb))
        {
            bomb.TakeDamage(damage);
            return;
        }

        // Poison
        if (other.TryGetComponent<PoisonSpawner>(out var poison))
        {
            poison.TakeDamage(damage);
            return;
        }

        // Ice Spawner
        if (other.TryGetComponent<BulletSpawnerIce>(out var ice))
        {
            ice.TakeDamage(damage);
            return;
        }
    }
}