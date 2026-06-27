using UnityEngine;

public class PlayerBuffSkill : MonoBehaviour
{
    [Header("Cấu hình Skill Buff")]
    public GameObject buffVfxPrefab; // Kéo Prefab vòng phép ở Bước 1 vào đây
    public float skillDuration = 8f;   // Thời gian tác dụng (8 giây)
    public float cooldown = 15f;       // Thời gian hồi chiêu (15 giây)

    [Header("Cấu hình tiêu tốn Mana")]
    [Tooltip("Số lượng Mana tiêu hao mỗi lần bấm nút bật Buff")]
    public int skillManaCost = 20;     // Mặc định tốn 20 Mana, thích tăng giảm tùy ý mày ngoài Inspector

    [HideInInspector]
    public bool isBuffActive = false;  // Biến để PlayerController check xem có đang được buff không

    private float _cooldownTimer;
    private PlayerHealth _playerHealth; // Cầu nối để check và trừ mana

    void Awake()
    {
        // Lấy component quản lý Máu/Mana gắn chung trên người Player
        _playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Đếm ngược hồi chiêu
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        // Nhấn I để bật kỹ năng gồng dame
        if (Input.GetKeyDown(KeyCode.I) && _cooldownTimer <= 0 && !isBuffActive)
        {
            // Kiểm tra xem có script PlayerHealth không
            if (_playerHealth != null)
            {
                // Gọi hàm UseMana bên PlayerHealth. Nếu đủ mana, nó tự trừ và trả về true.
                if (_playerHealth.UseMana(skillManaCost))
                {
                    ActivateBuff(); // ĐỦ MANA -> BẬT BUFF LUÔN CHỨ CÒN CHỜ GÌ NỮA!
                }
                else
                {
                    Debug.LogWarning("ĐÉO ĐỦ MANA ĐỂ KÍCH HOẠT SKILL BUFF DAME RỒI!");
                }
            }
            else
            {
                // Dự phòng nếu không tìm thấy script PlayerHealth
                Debug.LogError("LỖI: Không tìm thấy script PlayerHealth trên Player!");
            }
        }
    }

    void ActivateBuff()
    {
        isBuffActive = true;
        _cooldownTimer = cooldown;

        // Sinh ra vòng hiệu ứng phép thuật tại vị trí Player
        if (buffVfxPrefab != null)
        {
            GameObject vfx = Instantiate(buffVfxPrefab, transform.position, Quaternion.identity);
            if (vfx.TryGetComponent<BuffEffectFollow>(out var followScript))
            {
                // Truyền Player và thời gian 8s vào để hiệu ứng đi theo chân Player
                followScript.Setup(transform, skillDuration);
            }
        }

        Debug.Log($"Đã bật Kỹ năng Buff! Tiêu tốn {skillManaCost} Mana. Sát thương súng tăng 10% trong {skillDuration} giây!");

        // Gọi hàm tự động tắt Buff sau 8 giây
        Invoke(nameof(DeactivateBuff), skillDuration);
    }

    void DeactivateBuff()
    {
        isBuffActive = false;
        Debug.Log("Hết thời gian Buff! Sát thương súng trở lại bình thường.");
    }

    // Hàm public giúp các script khác hoặc UI lấy thời gian hồi chiêu hiển thị (nếu cần)
    public float GetCooldownNormalized()
    {
        if (cooldown <= 0) return 0f;
        return Mathf.Clamp01(_cooldownTimer / cooldown);
    }
}