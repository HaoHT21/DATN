using UnityEngine;

[RequireComponent(typeof(BossDarkController))]
public class BossDarkAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS DARK ---")]
    [Tooltip("Kéo file âm thanh đạn bắn thường (Skill 1 - Cast Skill) vào đây")]
    public AudioClip castShootSound;

    [Tooltip("Kéo file âm thanh lúc Boss kích hoạt tàng hình (Bắt đầu Skill 2) vào đây")]
    public AudioClip invisibleEnterSound;

    [Tooltip("Kéo file âm thanh lúc Boss hiện hình và bùng nổ bão đạn vòng tròn (Kết thúc Skill 2) vào đây")]
    public AudioClip circleBurstSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private BossDarkController _bossController;
    private int _lastCastBulletCount = 0;
    private bool _wasInvisible = false;

    // Reflection cached để tối ưu hóa hiệu năng, chống giật lag
    private System.Reflection.FieldInfo _usingSkillField;
    private System.Reflection.FieldInfo _isInvisibleField;
    private bool _hasFields = false;

    private void Awake()
    {
        _bossController = GetComponent<BossDarkController>();

        if (_bossController != null)
        {
            System.Type type = _bossController.GetType();

            // Tìm biến private "isInvisible" của BossDarkController
            _isInvisibleField = type.GetField("isInvisible",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            // Tìm biến private "usingSkill" từ class cha BossController
            System.Type baseType = type;
            while (baseType != null)
            {
                _usingSkillField = baseType.GetField("usingSkill",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (_usingSkillField != null) break;
                baseType = baseType.BaseType;
            }

            if (_isInvisibleField != null && _usingSkillField != null)
            {
                _hasFields = true;
            }
        }
    }

    private bool IsBossUsingSkill()
    {
        if (_hasFields && _usingSkillField != null && _bossController != null)
        {
            return (bool)_usingSkillField.GetValue(_bossController);
        }
        return false;
    }

    private bool IsBossInvisible()
    {
        if (_hasFields && _isInvisibleField != null && _bossController != null)
        {
            return (bool)_isInvisibleField.GetValue(_bossController);
        }
        return false;
    }

    private void Update()
    {
        if (_bossController == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);
        bool isCurrentlyUsingSkill = IsBossUsingSkill();
        bool isCurrentlyInvisible = IsBossInvisible();

        // --- 1. PHÁT ÂM THANH SKILL 1 (BẮN ĐẠN CAST MA THUẬT) ---
        if (isCurrentlyUsingSkill && !isCurrentlyInvisible && _bossController.bulletCastPrefab != null && _bossController.castPoint != null)
        {
            // Quét quanh castPoint xem có viên đạn cast mới nào vừa xuất hiện không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.castPoint.position, 0.5f);
            int currentCastBulletCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.bulletCastPrefab.name))
                {
                    currentCastBulletCount++;
                }
            }

            // Nếu số lượng đạn cast tăng -> Kích hoạt tiếng bắn ma thuật
            if (currentCastBulletCount > _lastCastBulletCount)
            {
                PlaySound(castShootSound, _bossController.castPoint.position, finalVolume);
            }
            _lastCastBulletCount = currentCastBulletCount;
        }
        else
        {
            _lastCastBulletCount = 0;
        }

        // --- 2. PHÁT ÂM THANH SKILL 2 (TÀNG HÌNH & BÙNG NỔ BÃO ĐẠN) ---
        if (isCurrentlyInvisible && !_wasInvisible)
        {
            // Khoảnh khắc bắt đầu tàng hình
            PlaySound(invisibleEnterSound, transform.position, finalVolume);
            _wasInvisible = true;
        }
        else if (!isCurrentlyInvisible && _wasInvisible)
        {
            // Khoảnh khắc vừa hiện hình trở lại và kích hoạt SpawnCircleBullet()
            if (_bossController.spawnPoint != null)
            {
                PlaySound(circleBurstSound, _bossController.spawnPoint.position, finalVolume);
            }
            else
            {
                PlaySound(circleBurstSound, transform.position, finalVolume);
            }
            _wasInvisible = false;
        }
    }

    private void PlaySound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossDarkAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D nghe to, rõ và phủ đều màn hình
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH: Gán âm thanh đi qua đúng kênh CombatSFX của Audio Mixer
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Tự dọn dẹp sau khi phát xong
    }
}