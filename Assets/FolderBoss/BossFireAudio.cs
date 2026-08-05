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

    private AudioSource _spitFireAudioSource;
    private float FinalVolume => Mathf.Clamp01(soundVolume / 100f);

    /// <summary>
    /// Gọi mỗi khi Boss sinh ra một viên cầu lửa (Skill 1)
    /// </summary>
    public void PlayFireballSound(Vector3 spawnPosition)
    {
        PlayOneShotSound(fireballShootSound, spawnPosition);
    }

    /// <summary>
    /// Bắt đầu phát tiếng phun lửa lặp (Skill 2)
    /// </summary>
    public void StartSpitFireLoop()
    {
        if (spitFireLoopSound == null) return;

        if (_spitFireAudioSource == null)
        {
            _spitFireAudioSource = gameObject.AddComponent<AudioSource>();
        }

        _spitFireAudioSource.clip = spitFireLoopSound;
        _spitFireAudioSource.spatialBlend = 0f; // Khóa 2D
        _spitFireAudioSource.volume = FinalVolume;
        _spitFireAudioSource.loop = true;

        if (AudioStaticManager.Instance != null)
        {
            _spitFireAudioSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        if (!_spitFireAudioSource.isPlaying)
        {
            _spitFireAudioSource.Play();
        }
    }

    /// <summary>
    /// Dừng tiếng phun lửa lặp (Skill 2)
    /// </summary>
    public void StopSpitFireLoop()
    {
        if (_spitFireAudioSource != null && _spitFireAudioSource.isPlaying)
        {
            _spitFireAudioSource.Stop();
        }
    }

    private void PlayOneShotSound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossFireAudio_Independent");
        tempAudio.transform.position = position;

        AudioSource aSource = tempAudio.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D
        aSource.volume = FinalVolume;

        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        aSource.Play();
        Destroy(tempAudio, clip.length);
    }

    private void OnDisable()
    {
        StopSpitFireLoop();
    }

    private void OnDestroy()
    {
        StopSpitFireLoop();
    }
}