using UnityEngine;

[RequireComponent(typeof(GodDessController))]
public class GodDessAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH NỮ THẦN (GODDESS) ---")]
    [Tooltip("Kéo file âm thanh tia Laser quét hủy diệt (Laser Skill Loop) vào đây")]
    public AudioClip laserLoopSound;

    [Tooltip("Kéo file âm thanh mưa sao băng rơi từ trên trời xuống (Meteor Skill) vào đây")]
    public AudioClip meteorRainSound;

    [Tooltip("Kéo file âm thanh phân thân tạo ảo ảnh (Clone Skill) vào đây")]
    public AudioClip cloneSkillSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Chỉnh âm lượng ngoài Inspector

    private GodDessController _bossController;

    // Lưu số lượng đạn/đệ tử cũ để phát hiện khoảnh khắc kích hoạt mới
    private int _lastMeteorBulletCount = 0;
    private int _lastCloneCount = 0;

    // Nguồn phát âm thanh lặp (Loop) cho chiêu Laser
    private AudioSource _laserAudioSource;
    private bool _isLaserSoundPlaying = false;

    // Các thành phần kỹ năng được tự động tham chiếu từ Inspector
    private MonoBehaviour _laserSkill;
    private MonoBehaviour _meteorSkill;
    private MonoBehaviour _cloneSkill;

    private void Awake()
    {
        _bossController = GetComponent<GodDessController>();

        // Tự động tìm kiếm 3 script kỹ năng con găm trên Nữ Thần
        _laserSkill = GetComponent("GoddessLaserSkill") as MonoBehaviour;
        _meteorSkill = GetComponent("GoddessMeteorSkill") as MonoBehaviour;
        _cloneSkill = GetComponent("GoddessCloneSkill") as MonoBehaviour;
    }

    private void Update()
    {
        if (_bossController == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        // --- 1. ÂM THANH SKILL 1: LASER SKILL (LOOP) ---
        if (_laserSkill != null)
        {
            // Kiểm tra xem laserPrefab có đang hoạt động trên Scene không hoặc thông qua logic active của Laser
            if (GetPrefabFromScript(_laserSkill, "laserPrefab", out GameObject laserPrefab))
            {
                Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, 10f);
                bool hasActiveLaser = false;
                foreach (var c in col)
                {
                    if (c.name.Contains(laserPrefab.name))
                    {
                        hasActiveLaser = true;
                        break;
                    }
                }

                if (hasActiveLaser)
                {
                    if (!_isLaserSoundPlaying)
                    {
                        StartLaserLoop(finalVolume);
                    }
                }
                else
                {
                    if (_isLaserSoundPlaying)
                    {
                        StopLaserLoop();
                    }
                }
            }
        }

        // --- 2. ÂM THANH SKILL 2: METEOR SKILL (MƯA SAO BĂNG) ---
        if (_meteorSkill != null && GetPrefabFromScript(_meteorSkill, "bulletPrefab", out GameObject meteorBullet))
        {
            Transform firePt = GetTransformFromScript(_meteorSkill, "firePoint");
            if (firePt != null && meteorBullet != null)
            {
                // Quét diện rộng quanh firePoint xem có sao băng mới nào dội xuống không
                Collider2D[] col = Physics2D.OverlapCircleAll(firePt.position, 8f);
                int currentCount = 0;
                foreach (var c in col)
                {
                    if (c.name.Contains(meteorBullet.name)) currentCount++;
                }

                // Phát âm thanh dồn dập khi có đợt sao băng mới rơi xuống
                if (currentCount > _lastMeteorBulletCount && (currentCount - _lastMeteorBulletCount) >= 1)
                {
                    PlaySound(meteorRainSound, firePt.position, finalVolume);
                }
                _lastMeteorBulletCount = currentCount;
            }
        }
        else
        {
            _lastMeteorBulletCount = 0;
        }

        // --- 3. ÂM THANH SKILL 3: CLONE SKILL (PHÂN THÂN) ---
        if (_cloneSkill != null && GetPrefabFromScript(_cloneSkill, "clonePrefab", out GameObject clonePrefab))
        {
            float spawnRadius = GetFloatFromScript(_cloneSkill, "spawnRadius", 5f);
            if (clonePrefab != null)
            {
                // Quét xem có bản sao phân thân nào vừa được gọi ra quanh Nữ Thần không
                Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, spawnRadius + 2f);
                int currentCount = 0;
                foreach (var c in col)
                {
                    if (c.name.Contains(clonePrefab.name)) currentCount++;
                }

                // Phát ra tiếng nổ ma thuật phân tách khi clone xuất hiện
                if (currentCount > _lastCloneCount)
                {
                    PlaySound(cloneSkillSound, transform.position, finalVolume);
                }
                _lastCloneCount = currentCount;
            }
        }
        else
        {
            _lastCloneCount = 0;
        }
    }

    private void StartLaserLoop(float volume)
    {
        _isLaserSoundPlaying = true;
        if (laserLoopSound != null)
        {
            if (_laserAudioSource == null)
            {
                _laserAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _laserAudioSource.clip = laserLoopSound;
            _laserAudioSource.spatialBlend = 0f; // Khóa 2D to rõ
            _laserAudioSource.volume = volume;
            _laserAudioSource.loop = true;

            // ==========================================
            // LONG MẠCH 1: Gán âm thanh lặp Laser đi qua đúng kênh CombatSFX
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
        _isLaserSoundPlaying = false;
        if (_laserAudioSource != null && _laserAudioSource.isPlaying)
        {
            _laserAudioSource.Stop();
        }
    }

    // --- CÁC HÀM PHỤ TRỢ DÙNG REFLECTION ĐỂ ĐỌC DỮ LIỆU AN TOÀN ---
    private bool GetPrefabFromScript(MonoBehaviour script, string fieldName, out GameObject prefab)
    {
        prefab = null;
        if (script == null) return false;
        System.Reflection.FieldInfo field = script.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field != null)
        {
            prefab = field.GetValue(script) as GameObject;
            return prefab != null;
        }
        return false;
    }

    private Transform GetTransformFromScript(MonoBehaviour script, string fieldName)
    {
        if (script == null) return null;
        System.Reflection.FieldInfo field = script.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return field != null ? field.GetValue(script) as Transform : null;
    }

    private float GetFloatFromScript(MonoBehaviour script, string fieldName, float defaultValue)
    {
        if (script == null) return defaultValue;
        System.Reflection.FieldInfo field = script.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return field != null ? (float)field.GetValue(script) : defaultValue;
    }

    private void PlaySound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempGodDessAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D chuẩn chỉ
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH 2: Gán âm thanh mưa sao băng/clone đi qua đúng kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Tự động xóa dọn dẹp RAM
    }

    private void OnDisable()
    {
        StopLaserLoop();
    }

    private void OnDestroy()
    {
        StopLaserLoop();
    }
}