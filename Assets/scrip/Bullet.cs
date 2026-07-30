using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damageAmount);
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    [HideInInspector] public int damage = 20;
    private Rigidbody2D _rb;

    [Header("Hiệu ứng")]
    public GameObject hitEffectPrefab; // Kéo Prefab hiệu ứng hạt vào đây

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // Đảm bảo đạn là Trigger để xuyên qua Player nhưng chạm được tường
        GetComponent<Collider2D>().isTrigger = true;

        // Di chuyển đạn (Dùng linearVelocity cho Unity 6)
        _rb.linearVelocity = transform.right * speed;

        // Tự hủy sau 3 giây để tránh rác bộ nhớ
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") || col.isTrigger)
            return;

        if (col.TryGetComponent<BreakableTile>(out var breakable))
        {
            breakable.TakeDamage(damage);
            HitEffect();
            return;
        }

        if (col.TryGetComponent<IDamageable>(out var damageableTarget))
        {
            damageableTarget.TakeDamage(damage);
            HitEffect();
            return;
        }

        if (col.TryGetComponent<EnemyHeath>(out var enemy))
        {
            EnemyController controller =
                col.GetComponent<
                EnemyController>();

            if (
                controller != null
            )
            {
                controller
                .SetHitDirection(
                    _rb.linearVelocity
                    .normalized
                );
            }

            enemy.TakeDamage(
                damage
            );

            HitEffect();

            return;
        }

        // Boss
        if (col.TryGetComponent<BossHeath>(out var boss))
        {
            boss.TakeDamage(
                damage
            );

            HitEffect();
            return;
        }

        HitEffect();
    }



    private void HitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(
                hitEffectPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}