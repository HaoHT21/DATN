using UnityEngine;

[RequireComponent(typeof(BossBloodController))]
public class BossBloodAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS BLOOD ---")]
    [Tooltip("Kéo file âm thanh tà thuật máu bắn ra (Skill 1) vào đây")]
    public AudioClip bloodShootSound;

    [Tooltip("Kéo file âm thanh triệu hồi đệ tử tà ác (Skill 2) vào đây")]
    public AudioClip summonSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private BossBloodController _bossController;
    private int _lastBulletCount = 0;
    private int _lastSummonCount = 0;

    // Reflection cached để tối ưu hóa hiệu năng, chống giật lag
    private System.Reflection.FieldInfo _usingSkillField;
    private bool _hasField = false;

    private void Awake()
    {
        _bossController = GetComponent<BossBloodController>();

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

        // --- 1. PHÁT ÂM THANH SKILL 1 (BẮN ĐẠN MÁU) ---
        if (isCurrentlyUsingSkill && _bossController.bulletPrefab != null && _bossController.firePoint != null)
        {
            // Quét quanh firePoint xem có viên đạn máu mới nào vừa sinh ra không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.firePoint.position, 0.5f);
            int currentBulletCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.bulletPrefab.name))
                {
                    currentBulletCount++;
                }
            }

            // Nếu phát hiện số lượng đạn tăng lên -> Kích hoạt tiếng bắn
            if (currentBulletCount > _lastBulletCount)
            {
                PlaySound(bloodShootSound, _bossController.firePoint.position, finalVolume);
            }
            _lastBulletCount = currentBulletCount;
        }
        else
        {
            _lastBulletCount = 0;
        }

        // --- 2. PHÁT ÂM THANH SKILL 2 (TRIỆU HỒI ĐỆ TỬ - SUMMON) ---
        if (isCurrentlyUsingSkill && _bossController.summonPrefab != null && _bossController.summonPoint != null)
        {
            // Quét diện rộng quanh summonPoint để phát hiện đệ tử tà ác vừa bước ra từ cổng dịch chuyển
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.summonPoint.position, 3f);
            int currentSummonCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.summonPrefab.name))
                {
                    currentSummonCount++;
                }
            }

            // Phát hiện số lượng đệ tử tăng lên -> Phát tiếng sấm sét triệu hồi tà ác
            if (currentSummonCount > _lastSummonCount)
            {
                PlaySound(summonSound, _bossController.summonPoint.position, finalVolume);
            }
            _lastSummonCount = currentSummonCount;
        }
        else
        {
            _lastSummonCount = 0;
        }
    }

    private void PlaySound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossBloodAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D nghe to, rõ và chân thực nhất
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH: Gán âm thanh skill của Boss đi qua đúng kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Phát hết nhạc tự động xóa sạch bộ nhớ ngoài Hierarchy
    }
}