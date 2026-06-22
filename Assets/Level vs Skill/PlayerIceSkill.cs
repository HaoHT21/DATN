using UnityEngine;

public class PlayerIceSkill : MonoBehaviour
{
    [Header("--- Cấu hình Kỹ năng Băng ---")]
    public KeyCode skillKey = KeyCode.M;   // Nút bấm M
    public GameObject icePrefab;          // Kéo thả cục Prefab viên đạn băng vào đây
    public Transform firePoint;            // Đầu nòng súng hoặc điểm xuất hiện của đạn
    public int manaCost = 70;             // Tốn 70 mana

    [Header("--- Giới hạn phạm vi (MỚI) ---")]
    [Tooltip("Khoảng cách tối đa để phát hiện enemy (Phạm vi dùng skill)")]
    public float castRange = 8f;

    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (_playerHealth != null && _playerHealth.IsDead) return;

        // Nhấn M để triển khai chiêu thức
        if (Input.GetKeyDown(skillKey))
        {
            // BƯỚC 1: Check xem có enemy nào nằm trong tầm đánh hay không trước
            Transform targetEnemy = FindClosestEnemyInRange();

            if (targetEnemy == null)
            {
                Debug.LogWarning("Không có enemy nào trong phạm vi dùng skill!");
                return; // Ngắt luôn, đéo cho dùng skill, đéo mất mana
            }

            // BƯỚC 2: Có quái rồi thì mới check mana và trừ mana
            if (_playerHealth != null)
            {
                if (_playerHealth.currentMana < manaCost)
                {
                    Debug.LogWarning("ĐÉO ĐỦ MANA ĐỂ PHÓNG BĂNG TIỄN!");
                    return;
                }
                _playerHealth.UseMana(manaCost); // Trừ chuẩn chỉ 70 mana
            }

            // BƯỚC 3: Sinh ra đạn băng
            if (icePrefab != null)
            {
                if (firePoint == null) firePoint = this.transform;

                // Tạo viên đạn
                GameObject projectedIce = Instantiate(icePrefab, firePoint.position, firePoint.rotation);

                // Truyền thẳng con quái mục tiêu vừa tìm được sang cho viên đạn đuổi theo luôn
                IceLance iceComponent = projectedIce.GetComponent<IceLance>();
                if (iceComponent != null)
                {
                    iceComponent.SetTarget(targetEnemy, castRange);
                }
            }
        }
    }

    // Hàm quét tìm quái gần nhất nhưng phải nằm trong tầm castRange
    private Transform FindClosestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = castRange; // Chỉ chấp nhận khoảng cách nhỏ hơn castRange
        Transform closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            // Check xem quái còn sống không
            BaseEnemyHealth baseHealth = enemy.GetComponent<BaseEnemyHealth>();
            EnemyHeath oldHealth = enemy.GetComponent<EnemyHeath>();
            bool isDead = (baseHealth != null && baseHealth.currentHealth <= 0) ||
                          (oldHealth != null && oldHealth.currentHealth <= 0);

            if (isDead) continue;

            // Tính khoảng cách từ Player đến con quái này
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }

    // Vẽ một vòng tròn đỏ ngoài Scene để mày dễ căn chỉnh độ rộng của tầm đánh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, castRange);
    }
}