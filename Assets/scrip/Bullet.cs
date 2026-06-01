using UnityEngine;

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
        // 1. Bỏ qua nếu chạm Player hoặc các vùng Trigger (như vùng đổi nhạc)
        if (col.CompareTag("Player") || col.isTrigger)
        {
            return;
        }

        // 2. TẠO HIỆU ỨNG NỔ (Cái này Trung đang thiếu nè)
        if (hitEffectPrefab != null)
        {
            // Tạo hiệu ứng tại đúng vị trí viên đạn đang đứng
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // 3. Xử lý gây sát thương cho Enemy
        if (col.TryGetComponent<EnemyHealth>(out var e))
        {
            e.TakeDamage(damage);
            Debug.Log($"<color=red>[Bullet]</color> Gây {damage} sát thương cho {col.name}");
        }

        // 4. Hủy viên đạn sau khi nổ
        Destroy(gameObject);
    }
}