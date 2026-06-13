using UnityEngine;

public class PlayerLightningSkill : MonoBehaviour
{
    [Header("Cấu hình Skill Sét")]
    public GameObject lightningPrefab; // Kéo Prefab tia sét ở Bước 1 vào đây
    public float skillRadius = 4f;       // Phạm vi tự động dính chiêu (bán kính vòng tròn)
    public float cooldown = 1.5f;       // Thời gian hồi chiêu
    public int damage = 30;             // Sát thương tia sét

    [Header("Bộ lọc Layer")]
    public LayerMask enemyLayer;        // Chọn Layer của Quái để quét chính xác

    private float _cooldownTimer;

    void Update()
    {
        // Đếm ngược hồi chiêu
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        // Nhấn J để gọi sấm sét
        if (Input.GetKeyDown(KeyCode.J) && _cooldownTimer <= 0)
        {
            CastLightningSkill();
        }
    }

    void CastLightningSkill()
    {
        // Sử dụng một vòng tròn vật lý ẩn để quét tất cả vật thể thuộc Enemy Layer trong phạm vi
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, skillRadius, enemyLayer);

        if (hitEnemies.Length == 0)
        {
            Debug.Log("Không có quái nào trong phạm vi dùng chiêu Sét!");
            return;
        }

        // Đã tìm thấy mục tiêu -> Kích hoạt hồi chiêu
        _cooldownTimer = cooldown;

        // Tìm con quái ở gần Player nhất để ưu tiên giật sét
        Collider2D closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D enemy in hitEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }

        // Bắt đầu giật sét lên đầu con quái gần nhất tìm được
        if (closestEnemy != null)
        {
            // Lấy vị trí của quái (Trung có thể cộng thêm một chút Y nếu muốn tia sét xuất hiện từ trên đỉnh đầu rơi xuống)
            Vector3 spawnPosition = closestEnemy.transform.position + new Vector3(0, 0.5f, 0);

            // Sinh ra tia sét ngay tại vị trí quái
            Instantiate(lightningPrefab, spawnPosition, Quaternion.identity);

            // Gây sát thương thẳng vào máu của con quái đó
            if (closestEnemy.TryGetComponent<EnemyAI>(out var enemyScript))
            {
                // Gọi hàm Die() tạm thời để test quái chết nổ coin, hoặc trừ máu nếu bạn có hàm TakeDamage
                enemyScript.Die();
            }
        }
    }

    // Vẽ vòng tròn phạm vi ngoài Scene để Trung dễ căn chỉnh độ xa gần
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, skillRadius);
    }
}