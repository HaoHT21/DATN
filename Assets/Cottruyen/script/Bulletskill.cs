using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bulletskill : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Đảm bảo đạn không bị rơi xuống dưới đất
    }

    // Hàm này sẽ được NPCCombat gọi ngay khi bắn để truyền hướng cho đạn bay
    public void Launch(Vector2 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;

            // Xoay mũi viên đạn hướng theo hướng bay (nếu đạn có chiều mũi tên)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // Tự hủy đạn sau 4 giây nếu bắn trượt để tránh rác bộ nhớ
    void Start()
    {
        Destroy(gameObject, 4f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Gây sát thương cho Player ở đây (nếu có)
            Debug.Log("Đạn trúng Player!");
            Destroy(gameObject); // Trúng player thì biến mất
        }
    }
}