using UnityEngine;

public class FireBullet : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 15;

    [Header("Move")]
    public float speed = 8f;

    [Header("Life Time")]
    public float lifeTime = 5f;

    [Header("Hiệu ứng nhiệt")]
    public float heatAmount = 0.1f; 

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Bay thẳng theo hướng Right của FirePoint giống BulletDamage
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player =
                other.GetComponent<PlayerHealth>();

            EffectManager effect =
                other.GetComponent<EffectManager>();

            if (player != null)
            {
                Vector2 hitDirection =
                    rb.linearVelocity.normalized;

                player.TakeDamage(damage, hitDirection);
            }

            if (effect != null)
            {
                effect.AddFireHeat(heatAmount);
            }

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }

        if (other.TryGetComponent<IDamageable>(out var damageableTarget))
        {
            damageableTarget.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }
}