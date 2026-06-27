using UnityEngine;

public class PoisonChargeSkill : MonoBehaviour
{
    [Header("--- CẤU HÌNH PREFAB CHIÊU ---")]
    public GameObject chargeSkillPrefab; // Kéo Prefab chiêu lấp lánh mới vào đây
    public float cooldown = 3f;
    private float _cooldownTimer = 0f;

    [Header("--- THỜI GIAN TỤ LỰC ĐỂ THÀNH CẦU (FRAME 0-8) ---")]
    [Tooltip("Thời gian tối thiểu cần đè nút L để hoạt ảnh chạy tới Frame 08 (Ví dụ: 0.8 giây)")]
    public float timeToCharge = 0.8f;
    private float _chargeCounter = 0f;
    private bool _isCharging = false;

    private GameObject _currentEffectInstance;
    private SpriteRenderer _sprite;
    private PlayerHealth _playerHealth; // Cầu nối lấy Level và trạng thái sống chết gốc

    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        // Tự động tìm component PlayerHealth gắn chung trên người con Player 4
        _playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // TỐI ƯU CHÍ MẠNG: Nếu Player 4 đã nghẻo thì hủy tụ lực ngay lập tức và chặn hoàn toàn logic dưới
        if (_playerHealth != null && _playerHealth.IsDead)
        {
            if (_isCharging) CancelCharge();
            return;
        }

        // Đếm ngược hồi chiêu
        if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;

        // BƯỚC KHÓA CHIÊU: Nếu người chơi nhấn phím L mà chưa đủ Level 4 -> Chặn đứng luôn!
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (_playerHealth != null && _playerHealth.currentLevel < 4)
            {
                // CHÈN DÒNG NÀY ĐỂ VĂNG CHỮ ĐỎ THÔNG BÁO RA MÀN HÌNH UI:
                if (SkillNotification.Instance != null)
                {
                    SkillNotification.Instance.ShowMessage("CHIÊU [L] ĐANG KHÓA! CẦN LEVEL 4", Color.red);
                }

                Debug.LogWarning($"<color=yellow>[Skill L đang khóa]</color> Bạn cần đạt Level 4 để mở khóa chiêu Tụ Lực Độc! (Cấp hiện tại của Player 4: {_playerHealth.currentLevel})");
                return; // Chặn đứng tại đây, đéo cho gồng gánh gì hết
            }
        }

        // 1. VỪA NHẤN XUỐNG PHÍM L: Bắt đầu tụ lực (Chỉ chạy khi đã qua được đoạn check Level ở trên)
        if (Input.GetKeyDown(KeyCode.L) && _cooldownTimer <= 0 && chargeSkillPrefab != null)
        {
            StartCharging();
        }

        // 2. ĐANG ĐÈ GIỮ PHÍM L: Đếm thời gian nén đạn và găm vị trí theo nòng súng
        if (_isCharging && Input.GetKey(KeyCode.L))
        {
            _chargeCounter += Time.deltaTime;

            if (_currentEffectInstance != null)
            {
                // Ép cục tụ năng lượng bám theo đầu nòng súng của Player 4
                Transform myFirePoint = transform.Find("FP") ?? transform.Find("WeaponHolder/FP") ?? transform.Find("FirePoint");
                _currentEffectInstance.transform.position = (myFirePoint != null) ? myFirePoint.position : transform.position;
            }
        }

        // 3. THẢ PHÍM L RA: Kiểm tra xem đã tụ đủ công lực chưa
        if (Input.GetKeyUp(KeyCode.L) && _isCharging)
        {
            if (_chargeCounter >= timeToCharge)
            {
                ReleaseBigBlast(); // ĐỦ TẦM -> PHÓNG!
            }
            else
            {
                CancelCharge(); // CHƯA ĐỦ TẦM -> HỦY CHIÊU!
            }
        }
    }

    void StartCharging()
    {
        _isCharging = true;
        _chargeCounter = 0f;

        Transform myFirePoint = transform.Find("FP") ?? transform.Find("WeaponHolder/FP") ?? transform.Find("FirePoint");
        Vector3 spawnPos = (myFirePoint != null) ? myFirePoint.position : transform.position;

        // Đẻ ra hiệu ứng tụ lực
        _currentEffectInstance = Instantiate(chargeSkillPrefab, spawnPos, Quaternion.identity);

        // Gọi lệnh báo hiệu ứng: "Bắt đầu tụ lực đi"
        if (_currentEffectInstance.TryGetComponent<ChargeBulletLogic>(out var bullet))
        {
            bullet.SetupCharge(transform);
        }
    }

    void ReleaseBigBlast()
    {
        _cooldownTimer = cooldown;
        _isCharging = false;

        if (_currentEffectInstance != null && _currentEffectInstance.TryGetComponent<ChargeBulletLogic>(out var bullet))
        {
            Vector2 dir = (_sprite != null && _sprite.flipX) ? Vector2.left : Vector2.right;
            bullet.Fire(dir); // Ra lệnh cho đạn phụt bay đi
        }
        _currentEffectInstance = null;
    }

    void CancelCharge()
    {
        _isCharging = false;
        if (_currentEffectInstance != null)
        {
            Destroy(_currentEffectInstance); // Xóa sổ ngay lập tức vì thả nút non tay
            Debug.Log("<color=red>[Tụ Lực Failure]</color> Thả nút quá sớm! Bị hủy chiêu.");
        }
    }
}