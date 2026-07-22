using UnityEngine;

[RequireComponent(typeof(BulletSpawnerIce))]
public class BulletSpawnerIceAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH THÙNG BĂNG ---")]
    [Tooltip("Kéo file âm thanh nổ vỡ băng (Ice Shatter/Freeze) vào đây")]
    public AudioClip iceShatterSound;

    [Tooltip("Kéo file âm thanh đạn băng găm trúng người (Ice Hit/Impact) vào đây")]
    public AudioClip bulletHitSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng từ 0 đến 100 ngoài Inspector

    private BulletSpawnerIce _iceSpawner;
    private bool _hasPlayedShatter = false;
    private Vector3 _lastPosition;

    private void Awake()
    {
        _iceSpawner = GetComponent<BulletSpawnerIce>();
    }

    private void Update()
    {
        // Liên tục ghi nhớ vị trí cuối cùng của thùng trước khi bị xóa sổ
        _lastPosition = transform.position;

        // Dự phòng: Nếu máu về 0 mà chưa kịp OnDestroy thì nổ luôn
        if (_iceSpawner != null && !_hasPlayedShatter && _iceSpawner.hp <= 0)
        {
            TriggerShatterAndAssignHitSounds(_lastPosition);
        }
    }

    // LONG MẠCH Ở ĐÂY: Khi cái thùng bị Destroy gốc xóa sổ, hàm này BẮT BUỘC phải chạy
    private void OnDestroy()
    {
        // Nếu thùng bị hủy do hết máu (chứ không phải do đổi map hay tắt game)
        if (!_hasPlayedShatter && _iceSpawner != null && _iceSpawner.hp <= 0)
        {
            TriggerShatterAndAssignHitSounds(_lastPosition);
        }
    }

    private void TriggerShatterAndAssignHitSounds(Vector3 spawnPos)
    {
        _hasPlayedShatter = true;
        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        // 1. Khởi tạo Object âm thanh độc lập hoàn toàn tại vị trí cuối cùng của thùng
        if (iceShatterSound != null)
        {
            GameObject tempAudio = new GameObject("TempIceShatterAudio_Independent");
            tempAudio.transform.position = spawnPos;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = iceShatterSound;
            aSource.spatialBlend = 0f; // Khóa chuẩn 2D nghe cực to rõ
            aSource.volume = finalVolume;

            // ==========================================
            // LONG MẠCH: Gán âm thanh nổ thùng băng đi qua đúng kênh EnvSFX của Audio Mixer
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.envGroup;
            }
            // ==========================================

            aSource.Play();
            Destroy(tempAudio, iceShatterSound.length); // Tự dọn dẹp sau khi phát xong
        }

        // 2. Quét diện rộng xung quanh điểm nổ để găm âm thanh va chạm cho các viên đạn con vừa bay ra
        // Tăng bán kính quét lên 2.5f để bao quát toàn bộ đạn vừa bắn ra
        Collider2D[] spawnedObjects = Physics2D.OverlapCircleAll(spawnPos, 2.5f);
        foreach (Collider2D col in spawnedObjects)
        {
            // Kiểm tra xem có đúng là các viên đạn con vừa bay ra không
            if (col.gameObject != gameObject && (col.CompareTag("Bullet") || col.name.Contains("Bullet") || col.name.Contains("Ice")))
            {
                BulletIceAudio bulletAudio = col.gameObject.GetComponent<BulletIceAudio>();
                if (bulletAudio == null)
                {
                    bulletAudio = col.gameObject.AddComponent<BulletIceAudio>();
                }
                bulletAudio.hitSound = bulletHitSound;
                bulletAudio.soundVolume = finalVolume;
            }
        }
    }
}