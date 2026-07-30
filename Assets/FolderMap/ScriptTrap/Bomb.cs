using UnityEngine;

public class Bomb : MonoBehaviour, IDamageable
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

        if (col != null)
            col.enabled = false;
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

        if (col != null)
            col.enabled = true;

        animator.Play("Boom");

        DamageNearby();

        Destroy(gameObject, 0.5f);
    }

    private void DamageNearby()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            // Các vật thể gần đó cũng nhận damage nếu có IDamageable
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}