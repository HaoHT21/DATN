using UnityEngine;

[RequireComponent(typeof(BossMinoController))]
public class BossMinoAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS MINO ---")]
    [Tooltip("Kéo file âm thanh quạt đạn bắn ra (Skill 1) vào đây")]
    public AudioClip shootSpreadSound;

    [Tooltip("Kéo file âm thanh tiếng bò tót gầm rống lúc chuẩn bị húc (Skill 2 - Chuẩn bị) vào đây")]
    public AudioClip chargeRoarSound;

    [Tooltip("Kéo file âm thanh tiếng dẫm đạp/phóng nhanh kéo dài (Skill 2 - Đang húc Loop) vào đây")]
    public AudioClip chargeRunLoopSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private BossMinoController _bossController;
    private int _lastBulletCount = 0;

    // Quản lý luồng âm thanh chạy lặp khi húc
    private AudioSource _chargeAudioSource;
    private bool _isCharging = false;

    // Reflection cached để tối ưu hóa hiệu năng đọc trạng thái dùng chiêu
    private System.Reflection.FieldInfo _usingSkillField;
    private bool _hasField = false;

    private void Awake()
    {
        _bossController = GetComponent<BossMinoController>();

        if (_bossController != null)
        {
            // Tìm biến private "usingSkill" từ class cha BossController một cách an toàn
            System.Type type = _bossController.GetType();
            while (type != null && !_hasField)
            {
                _usingSkillField = type.GetField("usingSkill",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (_usingSkillField != null)
                {
                    _hasField = true;
                }
                else
                {
                    type = type.BaseType;
                }
            }
        }
    }

    private bool IsBossUsingSkill()
    {
        if (_hasField && _usingSkillField != null && _bossController != null)
        {
            return (bool)_usingSkillField.GetValue(_bossController);
        }
        return false;
    }

    private void Update()
    {
        if (_bossController == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);
        bool isCurrentlyUsingSkill = IsBossUsingSkill();

        // --- 1. PHÁT ÂM THANH SKILL 1 (QUẠT ĐẠN RÌU - SHOOT SKILL) ---
        if (isCurrentlyUsingSkill && _bossController.bulletPrefab != null && _bossController.firePoint != null)
        {
            // Quét quanh firePoint xem có loạt đạn hình quạt mới nào xuất hiện không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.firePoint.position, 0.5f);
            int currentBulletCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.bulletPrefab.name))
                {
                    currentBulletCount++;
                }
            }

            // Vì loạt đạn bung ra nhiều viên cùng lúc, khi phát hiện số lượng đạn tăng đột biến
            // Chúng ta chỉ cần phát 1 tiếng vút gió/bắn đạn mạnh mẽ đại diện cho cả đợt đó
            if (currentBulletCount > _lastBulletCount && (currentBulletCount - _lastBulletCount) >= 1)
            {
                PlayOneShotSound(shootSpreadSound, _bossController.firePoint.position, finalVolume);
            }
            _lastBulletCount = currentBulletCount;
        }
        else
        {
            _lastBulletCount = 0;
        }

        // --- 2. PHÁT ÂM THANH SKILL 2 (BÒ TÓT HÚC ĐIÊN - REDBULL SKILL) ---
        if (_bossController.redBullEffect != null)
        {
            // Nếu hiệu ứng húc được kích hoạt bật lên (Active)
            if (_bossController.redBullEffect.activeSelf)
            {
                if (!_isCharging)
                {
                    StartChargeEffects(finalVolume);
                }
            }
            else
            {
                if (_isCharging)
                {
                    StopChargeLoop();
                }
            }
        }
    }

    private void StartChargeEffects(float volume)
    {
        _isCharging = true;

        // 1. Phát tiếng gầm chuẩn bị húc (One-shot ngay tại vị trí Boss)
        if (chargeRoarSound != null)
        {
            PlayOneShotSound(chargeRoarSound, transform.position, volume);
        }

        // 2. Bật tiếng dẫm đạp dồn dập chạy lặp (Loop) bám theo Boss khi đang lao đi
        if (chargeRunLoopSound != null)
        {
            if (_chargeAudioSource == null)
            {
                _chargeAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _chargeAudioSource.clip = chargeRunLoopSound;
            _chargeAudioSource.spatialBlend = 0f; // Khóa 2D nghe to rõ
            _chargeAudioSource.volume = volume;
            _chargeAudioSource.loop = true; // Bật lặp liên tục

            // ==========================================
            // LONG MẠCH 1: Gán âm thanh húc chạy lặp đi qua đúng kênh CombatSFX
            if (AudioStaticManager.Instance != null)
            {
                _chargeAudioSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // ==========================================

            _chargeAudioSource.Play();
        }
    }

    private void StopChargeLoop()
    {
        _isCharging = false;
        if (_chargeAudioSource != null && _chargeAudioSource.isPlaying)
        {
            _chargeAudioSource.Stop();
        }
    }

    private void PlayOneShotSound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossMinoAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa chuẩn 2D
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH 2: Gán âm thanh bắn rìu/tiếng gầm đi qua đúng kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Phát xong tự dọn dẹp khỏi Hierarchy
    }

    private void OnDisable()
    {
        StopChargeLoop();
    }

    private void OnDestroy()
    {
        StopChargeLoop();
    }
}