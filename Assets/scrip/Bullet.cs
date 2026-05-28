using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    [HideInInspector] public int damage = 20; // Đặt giá trị mặc định
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // Đảm bảo là Trigger
        GetComponent<Collider2D>().isTrigger = true;

        // Di chuyển
        _rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Kiểm tra Layer để tránh va chạm với chính Player hoặc các vật không cần thiết
        if (col.CompareTag("Player")) return;

        // Xử lý sát thương Enemy
        if (col.TryGetComponent<EnemyHealth>(out var e))
        {
            e.TakeDamage(damage);
        }

        // Hủy đạn khi va chạm với bất kỳ thứ gì có Collider (Enemy, Wall, v.v.)
        Destroy(gameObject);

        Debug.Log("Đạn chạm vào: " + col.name);
    }
}