using UnityEngine;

[RequireComponent(typeof(BossIceController))]
public class BossIceAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS BĂNG ---")]
    [Tooltip("Kéo file âm thanh bùng nổ bão băng vòng tròn (Skill 1 - Ice Burst) vào đây")]
    public AudioClip iceBurstSound;

    [Tooltip("Kéo file âm thanh phóng gai băng nhắm mục tiêu (Skill 2 - Attack Ice) vào đây")]
    public AudioClip attackIceShootSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    /// <summary>
    /// Phát âm thanh khi bung đợt bão băng (Skill 1)
    /// </summary>
    public void PlayIceBurstSound(Vector3 position)
    {
        PlaySound(iceBurstSound, position);
    }

    /// <summary>
    /// Phát âm thanh khi bắn mỗi viên gai băng (Skill 2)
    /// </summary>
    public void PlayAttackIceSound(Vector3 position)
    {
        PlaySound(attackIceShootSound, position);
    }

    private void PlaySound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        GameObject tempAudio = new GameObject("TempBossIceAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D to, rõ, phủ khắp màn hình đấu Boss
        aSource.volume = finalVolume;

        // Gán kênh âm thanh vào Mixer Combat
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        aSource.Play();
        Destroy(tempAudio, clip.length); // Tự hủy sau khi phát xong
    }
}