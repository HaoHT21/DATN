using UnityEngine;

public class PlayerBuffSkill : MonoBehaviour
{
    [Header("Cấu hình Skill Buff")]
    public GameObject buffVfxPrefab; // Kéo Prefab vòng phép ở Bước 1 vào đây
    public float skillDuration = 8f;   // Thời gian tác dụng (8 giây)
    public float cooldown = 15f;       // Thời gian hồi chiêu (15 giây)

    [Header("--- CẤU HÌNH ÂM THANH BUFF ---")]
    public AudioClip buffLoopSound;      // Ô kéo file âm thanh vòng năng lượng duy trì (Aura, Shield Loop)
    [Range(0f, 100f)] public float skillVolume = 100f; // Thanh trượt chỉnh to nhỏ từ 0 đến 100 ngoài Inspector

    [Header("Cấu hình tiêu tốn Mana")]
    [Tooltip("Số lượng Mana tiêu hao mỗi lần bấm nút bật Buff")]
    public int skillManaCost = 20;     // Mặc định tốn 20 Mana, thích tăng giảm tùy ý mày ngoài Inspector

    [HideInInspector]
    public bool isBuffActive = false;  // Biến để PlayerController check xem có đang được buff không

    private float _cooldownTimer;
    private PlayerHealth _playerHealth; // Cầu nối để check và trừ mana
    private AudioSource _activeBuffAudio; // Biến tạm quản lý tiếng vòng năng lượng đang bám theo người

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

        // CHỈ SỬA KHÚC NÀY: Khởi tạo nguồn âm thanh 2D chạy lặp (Loop) bám cứng theo Player
        if (buffLoopSound != null)
        {
            GameObject buffAudioObj = new GameObject("TempBuffLoopSound");
            buffAudioObj.transform.position = transform.position;
            buffAudioObj.transform.SetParent(transform); // Găm làm con của Player để di chuyển theo real-time

            _activeBuffAudio = buffAudioObj.AddComponent<AudioSource>();
            _activeBuffAudio.clip = buffLoopSound;
            _activeBuffAudio.spatialBlend = 0f; // Khóa âm thanh dạng 2D to rõ (đè bẹp nhạc nền)
            _activeBuffAudio.volume = Mathf.Clamp01(skillVolume / 100f); // Quy đổi hệ 100 về chuẩn 1.0 của Unity
            _activeBuffAudio.loop = true; // Bật lặp liên tục trong suốt 8 giây gồng buff

            // ==========================================
            // LONG MẠCH: Gán âm thanh vòng lặp gồng Buff đi qua đúng kênh CombatSFX của Audio Mixer
            if (AudioStaticManager.Instance != null)
            {
                _activeBuffAudio.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // ==========================================

            _activeBuffAudio.Play();
        }

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

        // CHỈ SỬA KHÚC NÀY: Khi hết thời gian buff, xóa bỏ ngay object âm thanh lặp để dập tắt tiếng kêu
        if (_activeBuffAudio != null)
        {
            Destroy(_activeBuffAudio.gameObject);
        }

        Debug.Log("Hết thời gian Buff! Sát thương súng trở lại bình thường.");
    }

    // Hàm public giúp các script khác hoặc UI lấy thời gian hồi chiêu hiển thị (nếu cần)
    public float GetCooldownNormalized()
    {
        if (cooldown <= 0) return 0f;
        return Mathf.Clamp01(_cooldownTimer / cooldown);
    }
}