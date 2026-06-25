using UnityEngine;

public class PlayerBlueLightningSkill : MonoBehaviour
{
    [Header("--- CẤU HÌNH SKILL SÉT XANH (PHÍM M) ---")]
    public KeyCode skillKey = KeyCode.M; // Phím M mặc định
    public GameObject lightningPrefab;  // Kéo Prefab hiệu ứng scifi_warp_003 vào đây
    public float skillRadius = 5f;       // Phạm vi tự động quét quái (bán kính vòng tròn)
    public float cooldown = 2f;          // Thời gian hồi chiêu
    public int damage = 70;              // Sát thương lớn: 70 dame theo yêu cầu

    [Header("--- BỘ LỌC LAYER ---")]
    public LayerMask enemyLayer;        // Kéo Layer của Quái (Enemy) vào đây

    private float _cooldownTimer;
    private PlayerHealth _playerHealth; // Cầu nối lấy Level và trạng thái sống chết từ PlayerHealth

    void Awake()
    {
        // Lấy component PlayerHealth gắn chung trên người Player 2
        _playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Kiểm tra nếu Player đã chết thì chặn hoàn toàn, không cho chạy hồi chiêu hay bấm nút
        if (_playerHealth != null && _playerHealth.IsDead) return;

        // Đếm ngược hồi chiêu liên tục
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        // Nhấn phím M để gọi sấm sét xanh dương
        if (Input.GetKeyDown(skillKey))
        {
            // BƯỚC KHÓA CHIÊU CUỐI: Check Level từ hệ thống PlayerHealth
            if (_playerHealth != null && _playerHealth.currentLevel < 7)
            {
                // CHÈN DÒNG NÀY ĐỂ VĂNG CHỮ ĐỎ THÔNG BÁO KHÓA CHIÊU:
                if (SkillNotification.Instance != null)
                {
                    SkillNotification.Instance.ShowMessage("CHIÊU [M] ĐANG KHÓA! CẦN LEVEL 7", Color.red);
                }

                Debug.LogWarning($"<color=cyan>[Skill M đang khóa]</color> Tuyệt chiêu cuối cần đạt Level 7 để mở khóa! (Cấp hiện tại của bạn: {_playerHealth.currentLevel})");
                return; // Chặn đứng tại đây
            }

            // Đủ Level 7 + Hết hồi chiêu + Có Prefab hiệu ứng thì mới cho triển chiêu
            if (_cooldownTimer <= 0 && lightningPrefab != null)
            {
                CastBlueLightning();
            }
        }
    }

    void CastBlueLightning()
    {
        // Tạo vòng tròn vật lý ẩn quét toàn bộ Collider của Quái trong tầm
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, skillRadius, enemyLayer);

        // NÂNG CẤP: Nếu không có con địch nào lọt vào tầm đánh thì văng chữ cảnh báo lên UI liền
        if (hitEnemies.Length == 0)
        {
            if (SkillNotification.Instance != null)
            {
                SkillNotification.Instance.ShowMessage("KHÔNG CÓ ĐỊCH TRONG PHẠM VI SÉT GIẬT!", Color.yellow);
            }

            Debug.Log("<color=cyan>[Sét Xanh]</color> Không có quái nào trong phạm vi để giật sét!");
            return; // Thoát sớm, đéo trừ hồi chiêu hay làm gì cả để người chơi không bị phí skill
        }

        // Kích hoạt hồi chiêu ngay khi chắc chắn có mục tiêu
        _cooldownTimer = cooldown;

        // Thuật toán tìm con quái ở gần Player 2 nhất để ưu tiên đánh trước
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

        // Thực hiện giật sét lên đầu con quái gần nhất
        if (closestEnemy != null)
        {
            // Tạo vị trí xuất hiện ngay tại quái (cộng thêm 0.2f để tia năng lượng ôm trọn người quái)
            Vector3 spawnPosition = closestEnemy.transform.position + new Vector3(0, 0.2f, 0);

            // Sinh ra hiệu ứng tia sét xanh dương tại vị trí quái
            Instantiate(lightningPrefab, spawnPosition, Quaternion.identity);

            // Gây 70 sát thương thẳng vào máu quái
            if (closestEnemy.TryGetComponent<EnemyHeath>(out var enemyHP))
            {
                if (!enemyHP.IsDead)
                {
                    enemyHP.TakeDamage(damage);
                    Debug.Log($"<color=blue>[Sét Xanh phím M]</color> Đã nã {damage} HP vào đầu {closestEnemy.name}!");
                }
            }
        }
    }

    // Vẽ vòng tròn phạm vi màu xanh dương ngoài Scene để dễ căn chỉnh độ xa gần
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, skillRadius);
    }
}