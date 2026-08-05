using UnityEngine;

[RequireComponent(typeof(BossRockController))]
public class BossRockAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS ĐÁ ---")]
    [Tooltip("Kéo file âm thanh đạn đá tảng bắn ra (Skill 2 - Shoot) vào đây")]
    public AudioClip rockShootSound;

    [Tooltip("Kéo file âm thanh tia laser quét hủy diệt kéo dài (Skill 1 - Laser Loop) vào đây")]
    public AudioClip laserLoopSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private AudioSource _laserAudioSource;

    private void Awake()
    {
        // Khởi tạo AudioSource dùng riêng cho hiệu ứng Laser Loop
        _laserAudioSource = gameObject.AddComponent<AudioSource>();
        _laserAudioSource.playOnAwake = false;
        _laserAudioSource.loop = true;
        _laserAudioSource.spatialBlend = 0f; // Âm thanh 2D rõ ràng
    }

    /// <summary>
    /// Phát tiếng bắn đạn đá tảng (Skill 2)
    /// </summary>
    public void PlayRockShootSound(Vector3 position)
    {
        PlayOneShotSound(rockShootSound, position);
    }

    /// <summary>
    /// Bắt đầu phát âm thanh tia Laser quét (Skill 1 Loop)
    /// </summary>
    public void StartLaserLoopSound()
    {
        if (laserLoopSound == null || _laserAudioSource == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        _laserAudioSource.clip = laserLoopSound;
        _laserAudioSource.volume = finalVolume;

        if (AudioStaticManager.Instance != null)
        {
            _laserAudioSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        if (!_laserAudioSource.isPlaying)
        {
            _laserAudioSource.Play();
        }
    }

    /// <summary>
    /// Dừng âm thanh tia Laser
    /// </summary>
    public void StopLaserLoopSound()
    {
        if (_laserAudioSource != null && _laserAudioSource.isPlaying)
        {
            _laserAudioSource.Stop();
        }
    }

    private void PlayOneShotSound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        GameObject tempAudio = new GameObject("TempBossRockAudio_Independent");
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
        StopLaserLoopSound();
    }

    private void OnDestroy()
    {
        StopLaserLoopSound();
    }
}