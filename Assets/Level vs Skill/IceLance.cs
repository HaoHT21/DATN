using UnityEngine;

public class IceLance : MonoBehaviour
{
    [Header("--- Chỉ số Đạn Bằng ---")]
    public float speed = 12f;
    public int damage = 150;
    public float rotateSpeed = 300f;

    private Transform target;
    private Rigidbody2D _rb;
    private float maxRange;
    private Vector2 startPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Lưu lại vị trí xuất phát để tính quãng đường bay
        startPosition = transform.position;

        // Phòng hờ tự hủy sau 3 giây nếu lỗi không va chạm
        Destroy(gameObject, 3f);
    }

    // Hàm nhận dữ liệu mục tiêu từ Player truyền qua
    public void SetTarget(Transform enemyTarget, float rangeLimit)
    {
        target = enemyTarget;
        maxRange = rangeLimit;
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        // GIỚI HẠN TẦM BAY: Nếu đạn bay quá xa vị trí ban đầu vượt tầm castRange -> Tự hủy luôn
        if (Vector2.Distance(startPosition, transform.position) > maxRange)
        {
            Destroy(gameObject);
            return;
        }

        // Đuổi theo mục tiêu
        if (target != null)
        {
            // Check nếu quái nghẻo giữa đường thì đạn bay thẳng tiếp
            BaseEnemyHealth baseHealth = target.GetComponent<BaseEnemyHealth>();
            EnemyHeath oldHealth = target.GetComponent<EnemyHeath>();
            if ((baseHealth != null && baseHealth.currentHealth <= 0) || (oldHealth != null && oldHealth.currentHealth <= 0))
            {
                target = null;
                return;
            }

            Vector2 direction = (Vector2)target.position - _rb.position;
            direction.Normalize();

            float rotateAmount = Vector3.Cross(direction, transform.right).z;
            _rb.angularVelocity = -rotateAmount * rotateSpeed;

            _rb.linearVelocity = transform.right * speed;
        }
        else
        {
            // Nếu không có mục tiêu hoặc quái chết, đạn bay thẳng theo hướng hiện tại
            _rb.linearVelocity = transform.right * speed;
            _rb.angularVelocity = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") || col.isTrigger) return;

        if (col.CompareTag("Enemy") || col.CompareTag("Boss"))
        {
            if (col.TryGetComponent<BaseEnemyHealth>(out var baseHealth))
            {
                baseHealth.TakeDamage(damage);
            }
            else if (col.TryGetComponent<EnemyHeath>(out var oldHealth))
            {
                oldHealth.TakeDamage(damage);
            }
            Destroy(gameObject); // Chạm quái thì nổ, hủy đạn
        }
    }
}