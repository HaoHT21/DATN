using UnityEngine;

public class IceLance : MonoBehaviour
{
    [Header("--- Chỉ số Đạn Băng (Bay Thẳng) ---")]
    public float speed = 12f;
    public int damage = 150;

    private Rigidbody2D _rb;
    private float maxRange;
    private Vector2 startPosition;
    private Vector2 moveDirection = Vector2.right; // Hướng bay mặc định

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Lưu lại vị trí xuất phát để tính toán khoảng cách giới hạn tầm bay
        startPosition = transform.position;

        // Ép Rigidbody truyền lực bay thẳng theo hướng được thiết lập ngay từ đầu
        if (_rb != null)
        {
            _rb.linearVelocity = moveDirection * speed;

            // Xoay đầu mũi giáo băng hướng chuẩn theo vector di chuyển
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Phòng hờ tự hủy sau 3 giây để dọn rác bộ nhớ nếu bay ra ngoài map
        Destroy(gameObject, 3f);
    }

    // Hàm nhận hướng bay và tầm xa từ Player truyền qua khi Instantiate đạn
    public void SetDirection(Vector2 direction, float rangeLimit)
    {
        moveDirection = direction.normalized;
        maxRange = rangeLimit;

        // Nếu gọi hàm này sau khi Start() đã chạy, cập nhật lại vận tốc lập tức
        if (_rb != null)
        {
            _rb.linearVelocity = moveDirection * speed;
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void FixedUpdate()
    {
        // GIỚI HẠN TẦM BAY REAL-TIME: Vượt quá maxRange tự hủy đạn
        if (Vector2.Distance(startPosition, transform.position) > maxRange)
        {
            Destroy(gameObject);
            return;
        }

        // Đạn bay thẳng tuyến tính liên tục, đéo cần tính toán quay đầu tìm quái nữa
        if (_rb != null)
        {
            _rb.linearVelocity = moveDirection * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Né Player hoàn toàn để không tự bắn trúng mình
        if (col.CompareTag("Player")) return;

        // KIỂM TRA ĐA TẦNG: Xác định mục tiêu có phải là Quái hay Boss không
        bool isEnemyOrBossTag = col.CompareTag("Enemy") || col.CompareTag("Boss");
        bool isBossLayer = LayerMask.LayerToName(col.gameObject.layer) == "Boss";

        if (isEnemyOrBossTag || isBossLayer)
        {
            // Trừ máu quái/Boss bằng script tương ứng của nhóm mày
            if (col.TryGetComponent<BaseEnemyHealth>(out var baseHealth))
            {
                baseHealth.TakeDamage(damage);
            }
            else if (col.TryGetComponent<EnemyHeath>(out var oldHealth))
            {
                oldHealth.TakeDamage(damage);
            }

            Destroy(gameObject); // Bùm! Hủy đạn ngay lập tức khi trúng đích
            return;
        }

        // ĐÃ SỬA: Chỉ né những vùng Trigger ẩn KHÔNG PHẢI là quái/Boss (Ví dụ: Portal, vùng chuyển map)
        if (col.isTrigger) return;
    }
}