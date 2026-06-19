using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [Header("--- Cấu hình Khiên (Kiểu Điểm Vị Trí) ---")]
    public KeyCode shieldKey = KeyCode.L; // Nút bấm
    public GameObject shieldPrefab;       // Cục Prefab Shield màu xanh ở thư mục Project
    [Tooltip("Kéo thả cái Object con ShieldPoint ngoài Hierarchy vào đây")]
    public Transform shieldSpawnPoint;    // Game Object xác định vị trí xuất hiện của khiên
    public int shieldManaCost = 20;       // Mana tiêu hao
    public float duration = 5f;           // Thời gian tồn tại của khiên

    [Header("--- Chỉ số chống chịu ---")]
    public int maxShieldHP = 50;          // Chắn tối đa 50 dame
    private int currentShieldHP;
    private bool isShieldActive = false;
    private float timer;

    private GameObject spawnedShield;     // Lưu con khiên thực tế đang chạy
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (_playerHealth != null && _playerHealth.IsDead)
        {
            DeactivateShield();
            return;
        }

        // Nhấn L để bật khiên
        if (Input.GetKeyDown(shieldKey) && !isShieldActive)
        {
            ActivateShield();
        }

        if (isShieldActive)
        {
            // DI CHUYỂN THEO ĐIỂM GỐC: Ép con khiên chạy khít khịt theo vị trí của ShieldPoint
            if (spawnedShield != null && shieldSpawnPoint != null)
            {
                spawnedShield.transform.position = shieldSpawnPoint.position;
            }

            // Đếm ngược thời gian hủy khiên
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                DeactivateShield();
            }
        }
    }

    private void ActivateShield()
    {
        if (_playerHealth != null)
        {
            if (_playerHealth.currentMana < shieldManaCost)
            {
                Debug.LogWarning("ĐÉO ĐỦ MANA BẬT KHIÊN ĐÂU ĐẠI CA ƠI!");
                return;
            }
            _playerHealth.UseMana(shieldManaCost);
        }

        // Đảm bảo phải kéo thả điểm gốc ngoài Inspector để tránh lỗi Null
        if (shieldSpawnPoint == null)
        {
            // Nếu quên kéo thả, tự động lấy chính vị trí Player làm điểm gốc dự phòng
            shieldSpawnPoint = this.transform;
            Debug.LogWarning("Mày chưa kéo thả ShieldPoint vào ô trống Inspector kìa Trung ơi!");
        }

        isShieldActive = true;
        currentShieldHP = maxShieldHP;
        timer = duration;

        // KHỞI TẠO TẠI ĐÚNG VỊ TRÍ ĐIỂM GỐC: Sinh ra khiên găm đúng tâm của ShieldPoint
        if (shieldPrefab != null)
        {
            spawnedShield = Instantiate(shieldPrefab, shieldSpawnPoint.position, Quaternion.identity);
        }

        Debug.Log($"<color=green>[Khiên]</color> Kích hoạt thành công tại vị trí {shieldSpawnPoint.name}!");
    }

    public bool AbsorbDamage(ref int damage)
    {
        if (!isShieldActive) return false;

        if (currentShieldHP >= damage)
        {
            currentShieldHP -= damage;
            Debug.Log($"<color=green>[Khiên]</color> Hấp thụ trọn vẹn {damage} dame. Khiên còn: {currentShieldHP}/{maxShieldHP}");
            damage = 0;
        }
        else
        {
            damage -= currentShieldHP;
            Debug.Log($"<color=red>[Khiên VỠ]</color> Cản được {currentShieldHP} dame. Sát thương lọt lưới vả vào Player: {damage}");
            currentShieldHP = 0;
            DeactivateShield();
        }

        return true;
    }

    public void DeactivateShield()
    {
        isShieldActive = false;
        if (spawnedShield != null)
        {
            Destroy(spawnedShield);
        }
        Debug.Log("<color=white>[Khiên]</color> Đã hủy bỏ.");
    }
}