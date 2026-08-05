using UnityEngine;

public class KnockbackBullet : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 15;

    [Header("Movement Settings")]
    public float speed = 8f;
    public float lifeTime = 5f;

    [Header("Knockback Custom Settings")]
    [Tooltip("Lực hất văng bổ sung riêng cho loại đạn này (Ví dụ: 20f đến 35f để văng cực xa)")]
    public float extraKnockbackForce = 25f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Bay thẳng theo hướng quay của nòng súng/FirePoint
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Kiểm tra nếu trúng Player
        if (other.CompareTag("Player"))
        {
            Vector2 hitDirection = rb.linearVelocity.normalized;

            // --- BƯỚC 1: Gọi gây sát thương bình thường (Không sửa PlayerHealth) ---
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage, hitDirection);
            }

            // --- BƯỚC 2: TỰ ĐỘNG ĐẨY PLAYER VĂNG XA BẰNG RIGIDBODY2D ---
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Reset lực di chuyển cũ của Player để lực đẩy mới tác động chuẩn xác nhất
                playerRb.linearVelocity = Vector2.zero;

                // Áp thêm lực đẩy cực mạnh (ForceMode2D.Impulse cho tác động tức thì)
                playerRb.AddForce(hitDirection * extraKnockbackForce, ForceMode2D.Impulse);
            }

            // Hủy viên đạn sau khi va chạm
            Destroy(gameObject);
            return;
        }

        // 2. Trúng tường
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        // 3. Trúng các mục tiêu khác có IDamageable
        if (other.TryGetComponent<IDamageable>(out var damageableTarget))
        {
            damageableTarget.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }
}