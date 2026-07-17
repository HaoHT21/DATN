using UnityEngine;

[RequireComponent(typeof(BossFireController))]
public class BossFireAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS LỬA ---")]
    [Tooltip("Kéo file âm thanh bóng lửa bắn ra (Skill 1) vào đây")]
    public AudioClip fireballShootSound;

    [Tooltip("Kéo file âm thanh phun lửa phè phè kéo dài (Skill 2 - Flamethrower Loop) vào đây")]
    public AudioClip spitFireLoopSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private BossFireController _bossController;
    private int _lastFireballCount = 0;

    // Nguồn phát âm thanh lặp cho kỹ năng phun lửa
    private AudioSource _spitFireAudioSource;
    private bool _isSpittingFire = false;

    // Reflection cached để tối ưu hóa hiệu năng đọc trạng thái dùng chiêu của Boss
    private System.Reflection.FieldInfo _usingSkillField;
    private bool _hasField = false;

    private void Awake()
    {
        _bossController = GetComponent<BossFireController>();

        if (_bossController != null)
        {
            // Tìm biến private "usingSkill" từ class cha BossController
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

        // --- 1. PHÁT ÂM THANH SKILL 1 (BÃO BÓNG LỬA - FIREBALL) ---
        if (isCurrentlyUsingSkill && _bossController.fireballPrefab != null && _bossController.fireballPoint != null)
        {
            // Quét quanh điểm bắn xem có bóng lửa mới nào vừa được sinh ra không
            Collider2D[] col = Physics2D.OverlapCircleAll(_bossController.fireballPoint.position, 0.5f);
            int currentFireballCount = 0;
            foreach (var c in col)
            {
                if (c.name.Contains(_bossController.fireballPrefab.name))
                {
                    currentFireballCount++;
                }
            }

            // Nếu phát hiện bóng lửa mới -> Kích hoạt âm thanh vút lửa dồn dập
            if (currentFireballCount > _lastFireballCount)
            {
                PlayOneShotSound(fireballShootSound, _bossController.fireballPoint.position, finalVolume);
            }
            _lastFireballCount = currentFireballCount;
        }
        else
        {
            _lastFireballCount = 0;
        }

        // --- 2. PHÁT ÂM THANH SKILL 2 (PHUN LỬA LIÊN TỤC - SPIT FIRE LOOP) ---
        if (_bossController.spitFireObject != null)
        {
            // Nếu Object luồng lửa được kích hoạt bật lên (Active)
            if (_bossController.spitFireObject.activeSelf)
            {
                if (!_isSpittingFire)
                {
                    StartSpitFireLoop(finalVolume);
                }
            }
            else
            {
                if (_isSpittingFire)
                {
                    StopSpitFireLoop();
                }
            }
        }
    }

    private void StartSpitFireLoop(float volume)
    {
        _isSpittingFire = true;

        if (spitFireLoopSound != null)
        {
            if (_spitFireAudioSource == null)
            {
                _spitFireAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _spitFireAudioSource.clip = spitFireLoopSound;
            _spitFireAudioSource.spatialBlend = 0f; // Khóa 2D
            _spitFireAudioSource.volume = volume;
            _spitFireAudioSource.loop = true; // Chạy lặp liên tục khi đang phun lửa

            // ==========================================
            // LONG MẠCH 1: Gán nguồn phun lửa lặp đi qua đúng kênh CombatSFX
            if (AudioStaticManager.Instance != null)
            {
                _spitFireAudioSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
            }
            // ==========================================

            _spitFireAudioSource.Play();
        }
    }

    private void StopSpitFireLoop()
    {
        _isSpittingFire = false;
        if (_spitFireAudioSource != null && _spitFireAudioSource.isPlaying)
        {
            _spitFireAudioSource.Stop();
        }
    }

    private void PlayOneShotSound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossFireAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D nghe cực to rõ
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH 2: Gán bóng lửa bắn thường đi qua đúng kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Phát xong tự hủy
    }

    // Phòng hờ nếu Boss đột ngột bị chết hoặc bị tắt đi khi đang phun lửa
    private void OnDisable()
    {
        StopSpitFireLoop();
    }

    private void OnDestroy()
    {
        StopSpitFireLoop();
    }
}