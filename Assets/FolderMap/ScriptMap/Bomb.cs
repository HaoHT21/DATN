using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    [Header("Health")]
    public int hp = 1;

    [Header("Explosion")]
    public float radius = 3f;
    public int damage = 50;

    [Header("Collider sẽ tắt khi nổ")]
    public Collider2D col;

    private Animator animator;
    private bool exploded = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (exploded) return;

        hp -= damageAmount;

        if (hp <= 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (exploded) return;

        exploded = true;

        // Tắt collider được chỉ định trong Inspector
        if (col != null)
            col.isTrigger = false;

        animator.Play("Boom");

        DamageNearby();

        Destroy(gameObject, 0.5f);
    }

    private void DamageNearby()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            // Enemy
            if (hit.TryGetComponent(out EnemyHeath enemy))
            {
                enemy.TakeDamage(damage);

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir =
                        (hit.transform.position - transform.position).normalized;
                }
            }

            // Player
            if (hit.TryGetComponent(out PlayerHealth player))
            {
                player.TakeDamage(damage);

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir =
                        (hit.transform.position - transform.position).normalized;
                }
            }

            // Tile phá hủy
            if (hit.TryGetComponent(out BreakableTile tile))
            {
                tile.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}