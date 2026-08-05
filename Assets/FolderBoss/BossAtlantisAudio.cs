using UnityEngine;

[RequireComponent(typeof(BossAtlantisController))]
public class BossAtlantisAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS ATLANTIS ---")]
    [Tooltip("File âm thanh đạn bắn thường (Skill 1)")]
    public AudioClip normalShootSound;

    [Tooltip("File âm thanh đạn mưa rơi xuống (Skill 2)")]
    public AudioClip flyShootSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private float FinalVolume => Mathf.Clamp01(soundVolume / 100f);

    /// <summary>
    /// Gọi khi Boss bắn đạn thường (Skill 1)
    /// </summary>
    public void PlayNormalShootSound(Vector3 spawnPosition)
    {
        PlaySoundAtPosition(normalShootSound, spawnPosition);
    }

    /// <summary>
    /// Gọi khi viên đạn mưa được tạo ra (Skill 2)
    /// </summary>
    public void PlayFlyShootSound(GameObject flyBulletInstance)
    {
        if (flyShootSound == null || flyBulletInstance == null) return;

        // Gán trực tiếp AudioSource vào đạn mưa vừa khởi tạo
        AudioSource aSource = flyBulletInstance.GetComponent<AudioSource>();
        if (aSource == null)
        {
            aSource = flyBulletInstance.AddComponent<AudioSource>();
        }

        aSource.clip = flyShootSound;
        aSource.spatialBlend = 0f; // Sound 2D
        aSource.volume = FinalVolume;

        // Kênh Audio Mixer
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        aSource.Play();
    }

    private void PlaySoundAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossShootAudio_Independent");
        tempAudio.transform.position = position;

        AudioSource aSource = tempAudio.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Sound 2D
        aSource.volume = FinalVolume;

        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        aSource.Play();
        Destroy(tempAudio, clip.length);
    }
}