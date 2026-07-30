using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletSummonEnemy : MonoBehaviour
{
    public float speed = 12f;

    public float lifeTime = 5f;

    public int damage = 20;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity =
            transform.right * speed;

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHeath enemy =
            other.GetComponent<EnemyHeath>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        BossHeath boss =
            other.GetComponent<BossHeath>();

        if (boss != null)
        {
            boss.TakeDamage(
                damage
                );

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}