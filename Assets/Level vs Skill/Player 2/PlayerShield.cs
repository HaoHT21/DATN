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

    [Header("--- CẤU HÌNH ÂM THANH KHIÊN ---")]
    public AudioClip shieldActivateSound; // Ô kéo thả file nhạc kích hoạt khiên (.mp3, .wav)
    [Range(0f, 100f)] public float skillVolume = 100f; // Thanh trượt chỉnh to nhỏ từ 0 đến 100 ngoài Inspector

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
            // BƯỚC KHÓA CHIÊU: Check trực tiếp biến currentLevel từ PlayerHealth của Player 2
            // Đạt Level 4 trở lên mới cho phép bật Khiên
            if (_playerHealth != null && _playerHealth.currentLevel < 4)
            {
                Debug.LogWarning($"<color=yellow>[Skill L đang khóa]</color> Bạn cần đạt Level 4 để mở khóa Khiên Năng Lượng! (Cấp hiện tại của Player 2: {_playerHealth.currentLevel})");
                return; // Chặn đứng, đéo cho kích hoạt khiên hay trừ mana
            }

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

        // KHỔI TẠO TẠI ĐÚNG VỊ TRÍ ĐIỂM GỐC: Sinh ra khiên găm đúng tâm của ShieldPoint
        if (shieldPrefab != null)
        {
            spawnedShield = Instantiate(shieldPrefab, shieldSpawnPoint.position, Quaternion.identity);
        }

        // CHỈ SỬA KHÚC NÀY: Khởi tạo AudioSource 2D thủ công để tiếng bật khiên nổ to rõ, đè bẹp nhạc nền
        if (shieldActivateSound != null)
        {
            GameObject tempAudio = new GameObject("TempShieldActivateAudio");
            tempAudio.transform.position = transform.position;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = shieldActivateSound;
            aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn (bỏ qua khoảng cách Camera)
            aSource.volume = Mathf.Clamp01(skillVolume / 100f); // Quy đổi hệ 100 về hệ 1.0 chuẩn Unity

            // ==========================================
            // LONG MẠCH: Gán âm thanh kích hoạt khiên đi qua đúng kênh CombatSFX của Audio Mixer
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // ==========================================

            aSource.Play();
            Destroy(tempAudio, shieldActivateSound.length); // Chạy xong tự dọn dẹp Object tạm khỏi Hierarchy
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