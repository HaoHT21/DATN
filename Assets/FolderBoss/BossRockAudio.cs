using UnityEngine;

[RequireComponent(typeof(BossRockController))]
public class BossRockAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS ĐÁ ---")]
    [Tooltip("Kéo file âm thanh đạn đá tảng bắn ra (Skill 2 - Shoot) vào đây")]
    public AudioClip rockShootSound;

    [Tooltip("Kéo file âm thanh tia laser quét hủy diệt kéo dài (Skill 1 - Laser Loop) vào đây")]
    public AudioClip laserLoopSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private BossRockController _bossController;
    private int _lastBulletCount = 0;

    // Nguồn phát âm thanh lặp cho tia laser
    private AudioSource _laserAudioSource;
    private bool _isLaserActive = false;

    // Reflection cached để tối ưu hóa hiệu năng đọc trạng thái dùng chiêu
    private System.Reflection.FieldInfo _usingSkillField;
    private bool _hasField = false;

    private void Awake()
    {
        _bossController = GetComponent<BossRockController>();

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

        // --- 1. PHÁT ÂM THANH SKILL 2 (BẮN ĐÁ - SHOOT) ---
        if (isCurrentlyUsingSkill && _bossController.bulletPrefab != null && _bossController.shootPoint != null)
        {
            // Quét quanh shootPoint xem có tảng đá mới nào vừa được ném ra không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.shootPoint.position, 0.5f);
            int currentBulletCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.bulletPrefab.name))
                {
                    currentBulletCount++;
                }
            }

            // Nếu phát hiện có đạn đá mới được sinh ra -> Kích hoạt tiếng đá vỡ rầm rập
            if (currentBulletCount > _lastBulletCount)
            {
                PlayOneShotSound(rockShootSound, _bossController.shootPoint.position, finalVolume);
            }
            _lastBulletCount = currentBulletCount;
        }
        else
        {
            _lastBulletCount = 0;
        }

        // --- 2. PHÁT ÂM THANH SKILL 1 (BẮN TIA LASER - LASER SKILL LOOP) ---
        if (_bossController.laserObject != null)
        {
            // Nếu Object tia Laser được kích hoạt bật lên (Active)
            if (_bossController.laserObject.activeSelf)
            {
                if (!_isLaserActive)
                {
                    StartLaserLoop(finalVolume);
                }
            }
            else
            {
                if (_isLaserActive)
                {
                    StopLaserLoop();
                }
            }
        }
    }

    private void StartLaserLoop(float volume)
    {
        _isLaserActive = true;

        if (laserLoopSound != null)
        {
            if (_laserAudioSource == null)
            {
                _laserAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _laserAudioSource.clip = laserLoopSound;
            _laserAudioSource.spatialBlend = 0f; // Khóa 2D nghe to rõ toàn bản đồ
            _laserAudioSource.volume = volume;
            _laserAudioSource.loop = true; // Bật lặp liên tục khi laser đang chiếu quét

            // ==========================================
            // LONG MẠCH 1: Gán âm thanh vòng lặp tia laser đi qua đúng kênh CombatSFX
            if (AudioStaticManager.Instance != null)
            {
                _laserAudioSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // ==========================================

            _laserAudioSource.Play();
        }
    }

    private void StopLaserLoop()
    {
        _isLaserActive = false;
        if (_laserAudioSource != null && _laserAudioSource.isPlaying)
        {
            _laserAudioSource.Stop();
        }
    }

    private void PlayOneShotSound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossRockAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D nghe cực đanh thép
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH 2: Gán âm thanh ném đá đi qua đúng kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Phát xong tự dọn dẹp khỏi Scene
    }

    // Đề phòng trường hợp Boss đột ngột bị chết hoặc bị Deactivate khi laser đang bắn dở
    private void OnDisable()
    {
        StopLaserLoop();
    }

    private void OnDestroy()
    {
        StopLaserLoop();
    }
}