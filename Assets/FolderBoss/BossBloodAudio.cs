using UnityEngine;

[RequireComponent(typeof(BossBloodController))]
public class BossBloodAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS BLOOD ---")]
    [Tooltip("File âm thanh tà thuật máu bắn ra (Skill 1)")]
    public AudioClip bloodShootSound;

    [Tooltip("File âm thanh triệu hồi đệ tử tà ác (Skill 2)")]
    public AudioClip summonSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private float FinalVolume => Mathf.Clamp01(soundVolume / 100f);

    /// <summary>
    /// Gọi khi Boss bắn đạn máu (Skill 1)
    /// </summary>
    public void PlayBloodShootSound(Vector3 spawnPosition)
    {
        PlaySoundAtPosition(bloodShootSound, spawnPosition);
    }

    /// <summary>
    /// Gọi khi Boss triệu hồi đệ tử (Skill 2)
    /// </summary>
    public void PlaySummonSound(Vector3 spawnPosition)
    {
        PlaySoundAtPosition(summonSound, spawnPosition);
    }

    private void PlaySoundAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempBossBloodAudio_Independent");
        tempAudio.transform.position = position;

        AudioSource aSource = tempAudio.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Sound 2D
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