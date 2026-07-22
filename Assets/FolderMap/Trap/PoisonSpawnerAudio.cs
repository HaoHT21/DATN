using UnityEngine;

[RequireComponent(typeof(PoisonSpawner))]
public class PoisonSpawnerAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH THÙNG ĐỘC ---")]
    [Tooltip("Kéo file âm thanh thùng độc nổ/axit bắn tung tóe (Acid/Poison Splash) vào đây")]
    public AudioClip poisonSplashSound;

    [Tooltip("Kéo file âm thanh vũng độc sủi bọt liên tục (Acid Sizzle/Bubbling Loop) vào đây")]
    public AudioClip poisonPoolLoopSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    [Header("--- GIỚI HẠN THỜI GIAN ---")]
    [Tooltip("Thời gian ép âm thanh sủi bọt kêu (Mặc định là 5 giây)")]
    public float poolAudioDuration = 5f;

    private PoisonSpawner _poisonSpawner;
    private bool _hasPlayedSplash = false;
    private Vector3 _lastPosition;

    private void Awake()
    {
        _poisonSpawner = GetComponent<PoisonSpawner>();
    }

    private void Update()
    {
        // Liên tục ghi nhớ vị trí cuối cùng của thùng độc trước khi bị xóa sổ khỏi Hierarchy
        _lastPosition = transform.position;

        // Dự phòng nếu Update chạy kịp lúc HP về 0 trước khi bị Destroy
        if (_poisonSpawner != null && !_hasPlayedSplash && _poisonSpawner.hp <= 0)
        {
            TriggerPoisonAudio(_lastPosition);
        }
    }

    // BẮT BUỘC CHẠY: Khi thùng độc gọi Destroy(gameObject), Unity ép hàm này phải kích hoạt trước khi bốc hơi
    private void OnDestroy()
    {
        if (!_hasPlayedSplash && _poisonSpawner != null && _poisonSpawner.hp <= 0)
        {
            TriggerPoisonAudio(_lastPosition);
        }
    }

    private void TriggerPoisonAudio(Vector3 spawnPos)
    {
        _hasPlayedSplash = true;
        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        // 1. Phát âm thanh nổ chất độc 2D độc lập (không lo bị hủy theo thùng)
        if (poisonSplashSound != null)
        {
            GameObject tempAudio = new GameObject("TempPoisonSplashAudio_Independent");
            tempAudio.transform.position = spawnPos;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = poisonSplashSound;
            aSource.spatialBlend = 0f; // Khóa chuẩn 2D to rõ
            aSource.volume = finalVolume;

            // ==========================================
            // LONG MẠCH: Gán âm thanh nổ thùng độc đi qua đúng kênh EnvSFX của Audio Mixer
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.envGroup;
            }
            // ==========================================

            aSource.Play();
            Destroy(tempAudio, poisonSplashSound.length);
        }

        // 2. Tìm vũng độc con vừa được sinh ra để găm âm thanh sủi bọt xèo xèo chạy lặp (Loop)
        // Quét phạm vi nhỏ ngay tại tâm vụ nổ để túm lấy Prefab vũng độc
        Collider2D[] spawnedObjects = Physics2D.OverlapCircleAll(spawnPos, 1.5f);
        foreach (Collider2D col in spawnedObjects)
        {
            if (col.gameObject != gameObject)
            {
                // Tìm kiếm linh hoạt: Kiểm tra xem chính nó hoặc Object cha của nó đã được gán tiếng chưa
                PoisonPoolAudio poolAudio = col.GetComponentInParent<PoisonPoolAudio>();

                // Nếu chưa từng được gán âm thanh -> Tiến hành gán ngay lên Object chính (Parent) của nó!
                if (poolAudio == null)
                {
                    // Đảm bảo lấy đúng Object cha cao nhất (nếu collider nằm ở Object con) để gán
                    GameObject targetObject = col.transform.parent != null ? col.transform.parent.gameObject : col.gameObject;

                    // Thử lấy lại một lần nữa ở Object mục tiêu
                    poolAudio = targetObject.GetComponent<PoisonPoolAudio>();
                    if (poolAudio == null)
                    {
                        poolAudio = targetObject.AddComponent<PoisonPoolAudio>();
                    }

                    // Gán đồng bộ thời gian và âm thanh kêu đúng 5 giây
                    poolAudio.loopSound = poisonPoolLoopSound;
                    poolAudio.soundVolume = finalVolume;
                    poolAudio.audioDuration = poolAudioDuration;
                }
            }
        }
    }
}