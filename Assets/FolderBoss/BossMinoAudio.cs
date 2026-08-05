using UnityEngine;

[RequireComponent(typeof(BossMinoController))]
public class BossMinoAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS MINO ---")]
    [Tooltip("Kéo file âm thanh quạt đạn bắn ra (Skill 1) vào đây")]
    public AudioClip shootSpreadSound;

    [Tooltip("Kéo file âm thanh tiếng bò tót gầm rống lúc chuẩn bị húc (Skill 2 - Chuẩn bị) vào đây")]
    public AudioClip chargeRoarSound;

    [Tooltip("Kéo file âm thanh tiếng dẫm đạp/phóng nhanh kéo dài (Skill 2 - Đang húc Loop) vào đây")]
    public AudioClip chargeRunLoopSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private AudioSource _chargeAudioSource;

    private void Awake()
    {
        // Khởi tạo AudioSource dùng riêng cho hiệu ứng âm thanh lặp (Loop) khi húc
        _chargeAudioSource = gameObject.AddComponent<AudioSource>();
        _chargeAudioSource.playOnAwake = false;
        _chargeAudioSource.loop = true;
        _chargeAudioSource.spatialBlend = 0f; // Âm thanh 2D rõ ràng
    }

    /// <summary>
    /// Phát tiếng quạt đạn (Skill 1)
    /// </summary>
    public void PlayShootSpreadSound(Vector3 position)
    {
        PlayOneShotSound(shootSpreadSound, position);
    }

    /// <summary>
    /// Phát tiếng gầm chuẩn bị húc (Skill 2)
    /// </summary>
    public void PlayChargeRoarSound(Vector3 position)
    {
        PlayOneShotSound(chargeRoarSound, position);
    }

    /// <summary>
    /// Bắt đầu phát âm thanh húc chạy lặp (Loop)
    /// </summary>
    public void StartChargeLoopSound()
    {
        if (chargeRunLoopSound == null || _chargeAudioSource == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        _chargeAudioSource.clip = chargeRunLoopSound;
        _chargeAudioSource.volume = finalVolume;

        if (AudioStaticManager.Instance != null)
        {
            _chargeAudioSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        if (!_chargeAudioSource.isPlaying)
        {
            _chargeAudioSource.Play();
        }
    }

    /// <summary>
    /// Dừng âm thanh húc lặp
    /// </summary>
    public void StopChargeLoopSound()
    {
        if (_chargeAudioSource != null && _chargeAudioSource.isPlaying)
        {
            _chargeAudioSource.Stop();
        }
    }

    private void PlayOneShotSound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        GameObject tempAudio = new GameObject("TempBossMinoAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f;
        aSource.volume = finalVolume;

        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        aSource.Play();
        Destroy(tempAudio, clip.length);
    }

    private void OnDisable()
    {
        StopChargeLoopSound();
    }

    private void OnDestroy()
    {
        StopChargeLoopSound();
    }
}