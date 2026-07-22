using UnityEngine;

[RequireComponent(typeof(Bomb))]
public class BombAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH THÙNG NỔ ---")]
    [Tooltip("Kéo file âm thanh nổ thùng (.mp3, .wav) vào đây")]
    public AudioClip explosionSound;

    [Range(0f, 100f)]
    [Tooltip("Âm lượng tối đa của riêng thùng này (từ 0 đến 100)")]
    public float soundVolume = 100f;

    private Bomb _bombComponent;
    private bool _hasPlayedSound = false;
    private int _initialHP;

    private void Awake()
    {
        _bombComponent = GetComponent<Bomb>();
    }

    private void Start()
    {
        if (_bombComponent != null)
        {
            _initialHP = _bombComponent.hp;
        }
    }

    private void Update()
    {
        if (_bombComponent == null || _hasPlayedSound) return;

        // Đón đầu vụ nổ: Nếu HP của thùng giảm về 0 hoặc ít hơn, lập tức nổ phát tiếng qua Mixer!
        if (_bombComponent.hp <= 0 && _initialHP > 0)
        {
            PlayExplosionSound();
        }
    }

    private void PlayExplosionSound()
    {
        _hasPlayedSound = true;

        if (explosionSound != null)
        {
            // Tạo GameObject âm thanh độc lập tạm thời
            GameObject tempAudio = new GameObject("TempBombExplosionAudio");
            tempAudio.transform.position = transform.position;
            AudioSource aSource = tempAudio.AddComponent<AudioSource>();

            aSource.clip = explosionSound;
            aSource.spatialBlend = 0f; // Khóa chuẩn 2D
            aSource.volume = Mathf.Clamp01(soundVolume / 100f);

            // Gán bẫy thùng đỏ đi qua đúng kênh EnvSFX của Audio Mixer
            if (AudioStaticManager.Instance != null)
            {
                aSource.outputAudioMixerGroup = AudioStaticManager.Instance.envGroup;
            }

            aSource.Play();
            Destroy(tempAudio, explosionSound.length); // Phát xong tự hủy
        }
    }
}