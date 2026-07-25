using UnityEngine;

public class PlayerIceSkill : MonoBehaviour
{
    [Header("--- Cấu hình Kỹ năng Băng (Nút L) ---")]
    public KeyCode skillKey = KeyCode.L;
    public GameObject icePrefab;          // Kéo thả cục Prefab viên đạn IceLance vào đây
    public Transform firePoint;            // Đầu nòng súng (FP hoặc FirePoint)
    public int manaCost = 70;

    [Header("--- CẤU HÌNH ÂM THANH CHIÊU THỨC ---")]
    public AudioClip iceCastSound;         // Ô kéo thả file nhạc tiếng phóng băng (.mp3, .wav)
    [Range(0f, 100f)] public float skillVolume = 100f; // Thanh trượt chỉnh to nhỏ từ 0 đến 100 ngoài Inspector

    [Header("--- Giới hạn phạm vi ---")]
    public float castRange = 8f;

    private PlayerHealth _playerHealth;   // Thằng này giữ cả Máu, Mana và currentLevel của mày luôn
    private SpriteRenderer _sprite;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_playerHealth != null && _playerHealth.IsDead) return;

        // Nhấn nút L để bắn Giáo Băng
        if (Input.GetKeyDown(skillKey))
        {
            // BƯỚC ĐỒNG BỘ: Check trực tiếp biến currentLevel nằm trong PlayerHealth gốc của mày
            if (_playerHealth != null && _playerHealth.currentLevel < 4)
            {
                // CHÈN DÒNG NÀY ĐỂ VĂNG CHỮ CHỮ ĐỎ RA MÀN HÌNH UI ĐẬP VÀO MẮT NGƯỜI CHƠI:
                if (SkillNotification.Instance != null)
                {
                    SkillNotification.Instance.ShowMessage("CHIÊU [L] ĐANG KHÓA! CẦN LEVEL 4", Color.red);
                }

                Debug.LogWarning($"<color=yellow>[Chiêu L đang khóa]</color> Bạn cần đạt Level 4 để mở khóa Giáo Băng! (Cấp hiện tại: {_playerHealth.currentLevel})");
                return; // Bị khóa thì chặn đứng ở đây, không cho trừ mana hay đẻ đạn
            }

            // BƯỚC 1: Check mana
            if (_playerHealth != null)
            {
                if (_playerHealth.currentMana < manaCost)
                {
                    Debug.LogWarning("ĐÉO ĐỦ MANA ĐỂ PHÓNG BĂNG TIỄN!");
                    return;
                }
                _playerHealth.UseMana(manaCost);
            }

            // BƯỚC 2: Sinh ra đạn giáo băng bay thẳng theo hướng mặt
            if (icePrefab != null)
            {
                if (firePoint == null) firePoint = this.transform;

                // CHỈ SỬA KHÚC NÀY: Khởi tạo AudioSource 2D thủ công để tiếng phóng giáo băng to rõ, đè bẹp nhạc nền
                if (iceCastSound != null)
                {
                    GameObject tempAudio = new GameObject("TempIceCastAudio");
                    tempAudio.transform.position = transform.position;
                    AudioSource aSource = tempAudio.AddComponent<AudioSource>();

                    aSource.clip = iceCastSound;
                    aSource.spatialBlend = 0f; // Ép về âm thanh 2D hoàn toàn (bỏ qua khoảng cách Camera)
                    aSource.volume = Mathf.Clamp01(skillVolume / 100f); // Quy đổi hệ 100 về hệ 1.0 chuẩn Unity

                    // ==========================================
                    // LONG MẠCH: Gán âm thanh skill Player đi qua đúng kênh CombatSFX của Audio Mixer
                    if (AudioStaticManager.Instance != null)
                    {
                        aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
                    }
                    // ==========================================

                    aSource.Play();
                    Destroy(tempAudio, iceCastSound.length); // Phát xong tự dọn dẹp Object tạm khỏi Hierarchy
                }

                GameObject projectedIce = Instantiate(icePrefab, firePoint.position, Quaternion.identity);

                // Lấy hướng Trái/Phải dựa theo flipX của Sprite Player
                Vector2 shootDir = (_sprite != null && _sprite.flipX) ? Vector2.left : Vector2.right;

                // Nạp hướng bay cho file IceLance mà mày vừa gửi
                IceLance iceComponent = projectedIce.GetComponent<IceLance>();
                if (iceComponent != null)
                {
                    iceComponent.SetDirection(shootDir, castRange);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, castRange);
    }
}