using UnityEngine;

[RequireComponent(typeof(BossDarkController))]
public class BossDarkAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS DARK ---")]
    [Tooltip("Kéo file âm thanh đạn bắn thường (Skill 1 - Cast Skill) vào đây")]
    public AudioClip castShootSound;

    [Tooltip("Kéo file âm thanh lúc Boss kích hoạt tàng hình (Bắt đầu Skill 2) vào đây")]
    public AudioClip invisibleEnterSound;

    [Tooltip("Kéo file âm thanh lúc Boss hiện hình và bùng nổ bão đạn vòng tròn (Kết thúc Skill 2) vào đây")]
    public AudioClip circleBurstSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private float FinalVolume => Mathf.Clamp01(soundVolume / 100f);

    /// <summary>
    /// Gọi khi Boss xả đạn ma thuật (Skill 1)
    /// </summary>
    public void PlayCastShootSound(Vector3 spawnPosition)
    {
        PlaySoundAtPosition(castShootSound, spawnPosition);
    }

    /// <summary>
    /// Gọi khi Boss kích hoạt tàng hình (Bắt đầu Skill 2)
    /// </summary>
    public void PlayInvisibleEnterSound()
    {
        PlaySoundAtPosition(invisibleEnterSound, transform.position);
    }

    /// <summary>
    /// Gọi khi Boss hiện hình bùng nổ bão đạn vòng tròn (Kết thúc Skill 2)
    /// </summary>
    public void PlayCircleBurstSound(Vector3 spawnPosition)
    {
        PlaySoundAtPosition(circleBurstSound, spawnPosition);
    }

    private void PlaySoundAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossDarkAudio_Independent");
        tempAudio.transform.position = position;

        AudioSource aSource = tempAudio.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D
        aSource.volume = FinalVolume;

        // Gán vào kênh CombatSFX của Audio Mixer
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        aSource.Play();
        Destroy(tempAudio, clip.length);
    }
}