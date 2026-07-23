using UnityEngine;

public class PoisonPoolAudio : MonoBehaviour
{
    [HideInInspector] public AudioClip loopSound;
    [HideInInspector] public float soundVolume = 1f;
    [HideInInspector] public float audioDuration = 5f; // Sẽ nhận trị số (5 giây) được truyền sang từ Spawner

    private AudioSource _audioSource;
    private float _timer;
    private bool _isPlaying = false;

    private void Start()
    {
        _timer = audioDuration;

        if (loopSound != null)
        {
            PlayLoopSound();
        }
    }

    private void PlayLoopSound()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = loopSound;
        _audioSource.spatialBlend = 0f; // Khóa chuẩn âm thanh 2D nghe to rõ nhất
        _audioSource.volume = soundVolume;
        _audioSource.loop = true; // Bật lặp liên tục

        // ==========================================
        // LONG MẠCH: Gán tiếng sủi bọt vũng độc đi qua đúng kênh EnvSFX của Audio Mixer
        if (AudioStaticManager.Instance != null)
        {
            _audioSource.outputAudioMixerGroup = AudioStaticManager.Instance.envGroup;
        }
        // ==========================================

        _audioSource.Play();
        _isPlaying = true;
    }

    private void Update()
    {
        if (!_isPlaying || _audioSource == null) return;

        // Đếm ngược thời gian chạy của âm thanh
        _timer -= Time.deltaTime;

        // Hết thời gian (5 giây) -> Tắt tiếng lập tức
        if (_timer <= 0f)
        {
            StopAndCleanUp();
        }
    }

    private void OnDisable()
    {
        StopAndCleanUp();
    }

    private void OnDestroy()
    {
        StopAndCleanUp();
    }

    private void StopAndCleanUp()
    {
        _isPlaying = false;
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
}