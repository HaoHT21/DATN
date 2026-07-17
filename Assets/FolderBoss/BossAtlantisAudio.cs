using UnityEngine;

[RequireComponent(typeof(BossAtlantisController))]
public class BossAtlantisAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS ATLANTIS ---")]
    [Tooltip("Kéo file âm thanh đạn bắn thường (Skill 1) vào đây")]
    public AudioClip normalShootSound;

    [Tooltip("Kéo file âm thanh đạn mưa rơi xuống (Skill 2) vào đây")]
    public AudioClip flyShootSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private BossAtlantisController _bossController;
    private int _lastBulletCount = 0;
    private bool _wasUsingFlySkill = false;

    // Reflection cached fields để tối ưu hóa hiệu năng, tránh giật lag khi Update chạy liên tục
    private System.Reflection.FieldInfo _usingSkillField;
    private bool _hasField = false;

    private void Awake()
    {
        _bossController = GetComponent<BossAtlantisController>();

        if (_bossController != null)
        {
            // LONG MẠCH: Tìm biến private "usingSkill" ở class hiện tại hoặc class cha (BossController)
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
                    type = type.BaseType; // Nếu không thấy ở class con, nhảy lên class cha để tìm tiếp
                }
            }
        }
    }

    // Hàm phụ trợ để đọc an toàn giá trị của biến private 'usingSkill'
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
        bool isCurrentlyUsingSkill = IsBossUsingSkill(); // Đọc vượt rào biến private thành công!

        // --- 1. THEO DÕI & PHÁT ÂM THANH SKILL 1 (BẮN THƯỜNG) ---
        if (isCurrentlyUsingSkill && _bossController.bulletPrefab != null && _bossController.firePoint != null)
        {
            // Quét xem có viên đạn thường nào vừa được sinh ra tại firePoint không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.firePoint.position, 0.5f);
            int currentBulletCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.bulletPrefab.name))
                {
                    currentBulletCount++;
                }
            }

            // Nếu phát hiện có đạn mới xuất hiện -> Phát tiếng bắn thường ngay lập tức
            if (currentBulletCount > _lastBulletCount)
            {
                PlaySound(normalShootSound, _bossController.firePoint.position, finalVolume);
            }
            _lastBulletCount = currentBulletCount;
        }
        else
        {
            _lastBulletCount = 0;
        }

        // --- 2. THEO DÕI & PHÁT ÂM THANH SKILL 2 (MƯA ĐẠN FLY SKILL) ---
        if (isCurrentlyUsingSkill && _bossController.flyBulletPrefab != null)
        {
            _wasUsingFlySkill = true;

            // Quét diện rộng quanh Boss để tóm các viên đạn mưa vừa rơi xuống
            Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, _bossController.flyRadius + 1f);
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.flyBulletPrefab.name))
                {
                    // Đính kèm âm thanh phát một lần trực tiếp lên đạn mưa nếu nó chưa có tiếng
                    AudioSource existingAudio = c.gameObject.GetComponent<AudioSource>();
                    if (existingAudio == null && flyShootSound != null)
                    {
                        AudioSource aSource = c.gameObject.AddComponent<AudioSource>();
                        aSource.clip = flyShootSound;
                        aSource.spatialBlend = 0f; // Khóa 2D
                        aSource.volume = finalVolume;

                        // ==========================================
                        // LONG MẠCH: Gán đạn mưa đi qua đúng kênh CombatSFX của Audio Mixer
                        if (AudioStaticManager.Instance != null)
                        {
                            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
                        }
                        // ==========================================

                        aSource.Play();
                    }
                }
            }
        }
        else
        {
            _wasUsingFlySkill = false;
        }
    }

    private void PlaySound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossShootAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa chuẩn âm thanh 2D
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH: Gán âm thanh bắn thường đi qua đúng kênh CombatSFX của Audio Mixer
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Phát xong tự dọn dẹp độc lập ngoài Hierarchy
    }
}