using UnityEngine;

[RequireComponent(typeof(BossEndController))]
public class BossEndAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS END ---")]
    [Tooltip("Kéo file âm thanh xả đạn thường (Skill Shoot) vào đây")]
    public AudioClip shootNormalSound;

    [Tooltip("Kéo file âm thanh tiếng vút gió khi lướt và bắn (Skill Dash Shoot) vào đây")]
    public AudioClip dashShootSound;

    [Tooltip("Kéo file âm thanh tiếng sấm sét/mưa đạn rơi từ trên trời xuống (Skill Bullet Rain) vào đây")]
    public AudioClip bulletRainSound;

    [Tooltip("Kéo file âm thanh biến dịch chuyển tức thời (Skill Teleport) vào đây")]
    public AudioClip teleportSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Chỉnh âm lượng ngoài Inspector

    private BossEndController _bossController;

    // Lưu số lượng đạn cũ của các chiêu thức để nhận biết phát bắn mới
    private int _lastShootBulletCount = 0;
    private int _lastDashBulletCount = 0;
    private int _lastRainBulletCount = 0;
    private Vector3 _lastPosition;

    // Các thành phần kỹ năng được tự động tham chiếu
    private MonoBehaviour _skillShoot;
    private MonoBehaviour _skillDashShoot;
    private MonoBehaviour _skillBulletRain;
    private MonoBehaviour _skillTeleport;

    private void Awake()
    {
        _bossController = GetComponent<BossEndController>();
        _lastPosition = transform.position;

        // Tự động tìm kiếm 4 script kỹ năng con găm trên Boss
        _skillShoot = GetComponent("BossSkillShoot") as MonoBehaviour;
        _skillDashShoot = GetComponent("BossSkillDashShoot") as MonoBehaviour;
        _skillBulletRain = GetComponent("BossSkillBulletRain") as MonoBehaviour;
        _skillTeleport = GetComponent("BossSkillTeleport") as MonoBehaviour;
    }

    private void Update()
    {
        if (_bossController == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        // --- 1. ÂM THANH CHIÊU 1: SHOOT SKILL ---
        if (_skillShoot != null && GetPrefabFromScript(_skillShoot, "bulletPrefab", out GameObject normalBullet))
        {
            Transform firePt = GetTransformFromScript(_skillShoot, "firePoint");
            if (firePt != null && normalBullet != null)
            {
                Collider2D[] col = Physics2D.OverlapCircleAll(firePt.position, 0.5f);
                int currentCount = 0;
                foreach (var c in col)
                {
                    if (c.name.Contains(normalBullet.name)) currentCount++;
                }

                if (currentCount > _lastShootBulletCount)
                {
                    PlaySound(shootNormalSound, firePt.position, finalVolume);
                }
                _lastShootBulletCount = currentCount;
            }
        }

        // --- 2. ÂM THANH CHIÊU 2: DASH SHOOT SKILL ---
        if (_skillDashShoot != null && GetPrefabFromScript(_skillDashShoot, "bulletPrefab", out GameObject dashBullet))
        {
            Transform firePt = GetTransformFromScript(_skillDashShoot, "firePoint");
            if (firePt != null && dashBullet != null)
            {
                Collider2D[] col = Physics2D.OverlapCircleAll(firePt.position, 0.5f);
                int currentCount = 0;
                foreach (var c in col)
                {
                    if (c.name.Contains(dashBullet.name)) currentCount++;
                }

                if (currentCount > _lastDashBulletCount)
                {
                    PlaySound(dashShootSound, firePt.position, finalVolume);
                }
                _lastDashBulletCount = currentCount;
            }
        }

        // --- 3. ÂM THANH CHIÊU 3: BULLET RAIN SKILL ---
        if (_skillBulletRain != null && GetPrefabFromScript(_skillBulletRain, "bulletPrefab", out GameObject rainBullet))
        {
            float areaRadius = GetFloatFromScript(_skillBulletRain, "areaRadius", 6f);
            if (rainBullet != null)
            {
                // Quét diện rộng quanh Boss để bắt mưa đạn dội xuống
                Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, areaRadius + 2f);
                int currentCount = 0;
                foreach (var c in col)
                {
                    if (c.name.Contains(rainBullet.name)) currentCount++;
                }

                if (currentCount > _lastRainBulletCount)
                {
                    PlaySound(bulletRainSound, transform.position, finalVolume);
                }
                _lastRainBulletCount = currentCount;
            }
        }

        // --- 4. ÂM THANH CHIÊU 4: TELEPORT SKILL (Biến hình dịch chuyển) ---
        // Nhận biết Teleport bằng cách so sánh khoảng cách thay đổi đột ngột giữa 2 khung hình (lớn hơn 3 mét)
        if (Vector3.Distance(transform.position, _lastPosition) > 3f)
        {
            PlaySound(teleportSound, _lastPosition, finalVolume); // Kêu tại điểm biến đi
            PlaySound(teleportSound, transform.position, finalVolume); // Kêu tại điểm hiện ra
        }
        _lastPosition = transform.position;
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

        GameObject tempAudio = new GameObject("TempBossEndAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D nghe cực to, rõ và phủ đều màn hình
        aSource.volume = volume;

        // ==========================================
        // LONG MẠCH: Gán toàn bộ âm thanh của Boss End đi qua đúng kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, clip.length); // Phát xong tự động xóa dọn dẹp RAM
    }
}