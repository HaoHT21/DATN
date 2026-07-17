using UnityEngine;

[RequireComponent(typeof(BossSlimeController))]
public class BossSlimeAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS SLIME ---")]
    [Tooltip("Kéo file âm thanh lúc Slime nện xuống đất bộc phát đạn (Skill 1) vào đây")]
    public AudioClip jumpAttackBurstSound;

    [Tooltip("Kéo file âm thanh nhầy nhụa phân rã triệu hồi Slime con (Skill 2) vào đây")]
    public AudioClip summonSlimeSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private BossSlimeController _bossController;
    private int _lastBulletCount = 0;
    private int _lastSummonCount = 0;

    // Reflection cached để tối ưu hiệu năng đọc trạng thái của Boss
    private System.Reflection.FieldInfo _usingSkillField;
    private bool _hasField = false;

    private void Awake()
    {
        _bossController = GetComponent<BossSlimeController>();

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

        // --- 1. PHÁT ÂM THANH SKILL 1 (JUMP ATTACK - BẮN ĐẠN 360 ĐỘ) ---
        if (isCurrentlyUsingSkill && _bossController.bulletPrefab != null && _bossController.firePoint != null)
        {
            // Quét quanh firePoint xem loạt đạn Slime mới nào vừa nổ ra không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.firePoint.position, 0.5f);
            int currentBulletCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.bulletPrefab.name))
                {
                    currentBulletCount++;
                }
            }

            // Vì loạt đạn bung ra 20-30 viên cùng lúc, khi số lượng đạn tăng đột biến
            // Chúng ta chỉ phát đúng 1 tiếng "bẹp/bùm" đại diện cho cú tiếp đất bộc phát đạn đó
            if (currentBulletCount > _lastBulletCount && (currentBulletCount - _lastBulletCount) > 5)
            {
                PlaySound(jumpAttackBurstSound, _bossController.firePoint.position, finalVolume);
            }
            _lastBulletCount = currentBulletCount;
        }
        else
        {
            _lastBulletCount = 0;
        }

        // --- 2. PHÁT ÂM THANH SKILL 2 (TRIỆU HỒI ĐỆ TỬ SLIME - SUMMON) ---
        if (isCurrentlyUsingSkill && _bossController.enemyPrefab != null)
        {
            // Quét diện rộng quanh Boss xem có Slime con nào vừa được sinh ra không
            Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, _bossController.summonRadius + 1f);
            int currentSummonCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.enemyPrefab.name))
                {
                    currentSummonCount++;
                }
            }

            // Phát hiện số lượng đàn em tăng lên -> Phát âm thanh nhớp nháp sinh sản
            if (currentSummonCount > _lastSummonCount)
            {
                PlaySound(summonSlimeSound, transform.position, finalVolume);
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

        GameObject tempAudio = new GameObject("TempBossSlimeAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D nghe cực to rõ và bao trùm
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH: Gán âm thanh đập đất/triệu hồi Slime đi qua đúng kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Phát xong tự dọn dẹp sạch sẽ
    }
}