using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class IceBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;

    [Header("Freeze")]
    public float freezeDuration = 2f; // Thời gian đóng băng

    [Header("Effect")]
    public GameObject hitEffectPrefab;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<Collider2D>().isTrigger = true;

        rb.linearVelocity = transform.right * speed;

        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.isTrigger)
            return;

        // Xử lý khi va chạm Player
        if (col.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(damage);

            // Gọi EffectManager để Freeze Player (Khóa di chuyển + Tấn công)
            if (col.TryGetComponent(out EffectManager effect))
            {
                effect.Freeze(freezeDuration);
            }

            Hit();
            return;
        }

        if (col.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }

        if (col.TryGetComponent<IDamageable>(out var damageableTarget))
        {
            damageableTarget.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }

    private void Hit()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}