using UnityEngine;

public class PlayerDeathSkill : MonoBehaviour
{
    [Header("--- CẤU HÌNH PREFAB SKILL ---")]
    public GameObject deathSkillPrefab; // Kéo Prefab chiêu chữ Tử vào đây
    public float cooldown = 4f;
    private float _cooldownTimer = 0f;

    [Header("--- THỜI GIAN TỤ CHIÊU (FRAME 0-30) ---")]
    public float timeToCharge = 0.9f;
    private float _chargeCounter = 0f;
    private bool _isCharging = false;

    [Header("--- CẤU HÌNH ÂM THANH CHIÊU THỨC ---")]
    public AudioClip chargeSound;          // Ô kéo file âm thanh ngưng tụ năng lượng (nhỏ rồi to dần)
    public AudioClip enemyHitSound;       // Ô kéo file tiếng hét căm phẫn "TỬ" hoặc tiếng nổ lớn khi trúng địch
    [Range(0f, 100f)] public float skillVolume = 100f; // Thanh trượt chỉnh to nhỏ từ 0 đến 100 ngoài Inspector

    private GameObject _currentEffectInstance;
    private SpriteRenderer _sprite;
    private PlayerHealth _playerHealth; // Cầu nối lấy Level và trạng thái sống chết từ PlayerHealth
    private AudioSource _activeChargeAudio; // Biến tạm quản lý tiếng gồng lực real-time

    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        // Tự động tìm component PlayerHealth gắn chung trên người con Player 4
        _playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // TỐI ƯU CHÍ MẠNG: Nếu Player 4 đã nghẻo thì hủy tụ chiêu ngay lập tức và chặn hoàn toàn logic dưới
        if (_playerHealth != null && _playerHealth.IsDead)
        {
            if (_isCharging) CancelCharge();
            return;
        }

        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        // BƯỚC KHÓA CHIÊU CUỐI: Khi vừa nhấn phím M -> Check Level hệ thống liền
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_playerHealth != null && _playerHealth.currentLevel < 7)
            {
                // CHÈN DÒNG NÀY ĐỂ VĂNG THÔNG BÁO CHỮ ĐỎ LÊN MÀN HÌNH UI:
                if (SkillNotification.Instance != null)
                {
                    SkillNotification.Instance.ShowMessage("CHIÊU [M] ĐANG KHÓA! CẦN LEVEL 7", Color.red);
                }

                Debug.LogWarning($"<color=cyan>[Skill M đang khóa]</color> Tuyệt chiêu cuối CHỮ TỬ cần đạt Level 7 để mở khóa! (Cấp hiện tại của Player 4: {_playerHealth.currentLevel})");
                return; // Chặn đứng tại đây, đéo cho gồng gánh hay sinh hiệu ứng
            }
        }

        // 1. SỬA THÀNH PHÍM M: Vừa nhấn xuống phím M -> Bắt đầu tụ lực vẽ chữ Tử (Chỉ chạy khi đạt Lv7 trở lên)
        if (Input.GetKeyDown(KeyCode.M) && _cooldownTimer <= 0 && deathSkillPrefab != null)
        {
            StartCharging();
        }

        // 2. ĐANG ĐÈ GIỮ PHÍM M: Găm vị trí theo nòng súng Player
        if (_isCharging && Input.GetKey(KeyCode.M))
        {
            _chargeCounter += Time.deltaTime;

            if (_currentEffectInstance != null)
            {
                Transform myFirePoint = transform.Find("FP") ?? transform.Find("WeaponHolder/FP") ?? transform.Find("FirePoint") ?? transform.Find("WeaponHolder/FirePoint");
                _currentEffectInstance.transform.position = (myFirePoint != null) ? myFirePoint.position : transform.position;
            }
        }

        // 3. THẢ PHÍM M RA: Kiểm tra đủ thời gian tụ lực chuyên nghiệp chưa để phóng
        if (Input.GetKeyUp(KeyCode.M) && _isCharging)
        {
            if (_chargeCounter >= timeToCharge)
            {
                ReleaseDeathBlast(); // ĐỦ TẦM -> PHÓNG CHIÊU!
            }
            else
            {
                CancelCharge(); // THẢ SỚM -> HỦY CHIÊU!
            }
        }
    }

    void StartCharging()
    {
        _isCharging = true;
        _chargeCounter = 0f;

        Transform myFirePoint = transform.Find("FP") ?? transform.Find("WeaponHolder/FP") ?? transform.Find("FirePoint") ?? transform.Find("WeaponHolder/FirePoint");
        Vector3 spawnPos = (myFirePoint != null) ? myFirePoint.position : transform.position;

        // CHỈ SỬA KHÚC NÀY: Khởi tạo tiếng rú tụ năng lượng bám theo người Player
        if (chargeSound != null)
        {
            GameObject chargeAudioObj = new GameObject("TempDeathChargeSound");
            chargeAudioObj.transform.position = transform.position;
            chargeAudioObj.transform.SetParent(transform); // Găm thẳng làm con Player để di chuyển theo real-time

            _activeChargeAudio = chargeAudioObj.AddComponent<AudioSource>();
            _activeChargeAudio.clip = chargeSound;
            _activeChargeAudio.spatialBlend = 0f; // Khóa chuẩn 2D nghe cực to rõ
            _activeChargeAudio.volume = Mathf.Clamp01(skillVolume / 100f);

            // ==========================================
            // LONG MẠCH: Gán âm thanh gồng chiêu chữ Tử đi qua đúng kênh CombatSFX của Audio Mixer
            if (AudioStaticManager.Instance != null)
            {
                _activeChargeAudio.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // ==========================================

            _activeChargeAudio.Play();
        }

        _currentEffectInstance = Instantiate(deathSkillPrefab, spawnPos, Quaternion.identity);

        if (_currentEffectInstance.TryGetComponent<DeathBulletLogic>(out var bullet))
        {
            bullet.SetupCharge(transform);
        }
    }

    void ReleaseDeathBlast()
    {
        _cooldownTimer = cooldown;
        _isCharging = false;

        // Xả chiêu thành công -> Dọn dẹp dứt điểm tiếng gồng lực
        if (_activeChargeAudio != null)
        {
            Destroy(_activeChargeAudio.gameObject);
        }

        if (_currentEffectInstance != null && _currentEffectInstance.TryGetComponent<DeathBulletLogic>(out var bullet))
        {
            Vector2 dir = (_sprite != null && _sprite.flipX) ? Vector2.left : Vector2.right;

            // NẠP THÊM LOGIC: Chuyển giao file âm thanh căm phẫn/nổ lớn và mức volume sang cho viên đạn chữ Tử tự nổ khi trúng đích
            bullet.hitSound = enemyHitSound;
            bullet.soundVolume = Mathf.Clamp01(skillVolume / 100f);

            bullet.Fire(dir);
        }
        _currentEffectInstance = null;
    }

    void CancelCharge()
    {
        _isCharging = false;

        // Thả nút sớm làm hủy chiêu -> Tắt tiếng gồng lực lập tức
        if (_activeChargeAudio != null)
        {
            Destroy(_activeChargeAudio.gameObject);
        }

        if (_currentEffectInstance != null)
        {
            Destroy(_currentEffectInstance);
            Debug.Log("<color=red>[Hủy Chiêu]</color> Thả phím M quá sớm!");
        }
    }
}