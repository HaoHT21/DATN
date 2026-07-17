using UnityEngine;

[RequireComponent(typeof(BossIceController))]
public class BossIceAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS BĂNG ---")]
    [Tooltip("Kéo file âm thanh bùng nổ bão băng vòng tròn (Skill 1 - Ice Burst) vào đây")]
    public AudioClip iceBurstSound;

    [Tooltip("Kéo file âm thanh phóng gai băng nhắm mục tiêu (Skill 2 - Attack Ice) vào đây")]
    public AudioClip attackIceShootSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private BossIceController _bossController;
    private int _lastIceBurstCount = 0;
    private int _lastAttackIceCount = 0;

    // Reflection cached để tối ưu hóa hiệu năng đọc trạng thái dùng chiêu của Boss
    private System.Reflection.FieldInfo _usingSkillField;
    private bool _hasField = false;

    private void Awake()
    {
        _bossController = GetComponent<BossIceController>();

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

        // --- 1. PHÁT ÂM THANH SKILL 1 (BÃO BĂNG VÒNG TRÒN - ICE BURST) ---
        if (isCurrentlyUsingSkill && _bossController.icePrefab != null && _bossController.icePoint != null)
        {
            // Quét quanh icePoint xem có loạt đạn băng 360 độ mới nào vừa sinh ra không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.icePoint.position, 0.5f);
            int currentIceBurstCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.icePrefab.name))
                {
                    currentIceBurstCount++;
                }
            }

            // Vì loạt đạn này bung ra hàng chục viên cùng lúc, nếu số lượng đạn tăng đột biến 
            // Chúng ta chỉ cần phát 1 tiếng nổ băng bùng phát đại diện cho cả đợt (wave) đó
            if (currentIceBurstCount > _lastIceBurstCount && (currentIceBurstCount - _lastIceBurstCount) > 5)
            {
                PlaySound(iceBurstSound, _bossController.icePoint.position, finalVolume);
            }
            _lastIceBurstCount = currentIceBurstCount;
        }
        else
        {
            _lastIceBurstCount = 0;
        }

        // --- 2. PHÁT ÂM THANH SKILL 2 (PHÓNG GAI BĂNG - ATTACK ICE) ---
        if (isCurrentlyUsingSkill && _bossController.attackIcePrefab != null && _bossController.attackIcePoint != null)
        {
            // Quét quanh attackIcePoint xem có gai băng nhọn nào vừa bắn ra không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.attackIcePoint.position, 0.5f);
            int currentAttackIceCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.attackIcePrefab.name))
                {
                    currentAttackIceCount++;
                }
            }

            // Nếu phát hiện gai băng mới được phóng đi -> Phát tiếng rít băng sắc lẹm
            if (currentAttackIceCount > _lastAttackIceCount)
            {
                PlaySound(attackIceShootSound, _bossController.attackIcePoint.position, finalVolume);
            }
            _lastAttackIceCount = currentAttackIceCount;
        }
        else
        {
            _lastAttackIceCount = 0;
        }
    }

    private void PlaySound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossIceAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D to, rõ, phủ khắp màn hình đấu Boss
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH: Gán âm thanh bão băng/gai băng đi qua đúng kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Phát xong tự hủy độc lập để giải phóng RAM
    }
}