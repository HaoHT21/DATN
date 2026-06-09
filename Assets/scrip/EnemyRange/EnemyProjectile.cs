using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Mặc định (có thể ghi đè khi bắn)")]
    public float speed = 8f;
    public int damage = 10;
    public float lifeTime = 5f;

    [Header("Hiệu ứng")]
    public GameObject hitEffectPrefab;

    private Rigidbody2D _rb;
    private bool _initialized;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        if (!_initialized)
            _rb.linearVelocity = transform.right * speed;

        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Gọi ngay sau Instantiate để thiết lập hướng, tốc độ và sát thương.
    /// </summary>
    public void Initialize(Vector2 direction, float projectileSpeed, int projectileDamage)
    {
        speed = projectileSpeed;
        damage = projectileDamage;
        _initialized = true;

        Vector2 normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        _rb.linearVelocity = normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger && !other.CompareTag("Player"))
            return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerHealth>(out var playerHealth))
                playerHealth.TakeDamage(damage);

            SpawnHitEffect();
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
    }
}
