using UnityEngine;

[RequireComponent(typeof(BossSlimeController))]
public class BossSlimeAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BOSS SLIME ---")]
    [Tooltip("Kéo file âm thanh lúc Slime nện xuống đất bộc phát đạn (Skill 1) vào đây")]
    public AudioClip jumpAttackBurstSound;

    [Tooltip("Kéo file âm thanh nhầy nhụa phân rã triệu hồi Slime con (Skill 2) vào đây")]
    public AudioClip summonSlimeSound;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    /// <summary>
    /// Phát âm thanh cú nện đất bộc phát đạn 360 độ (Skill 1)
    /// </summary>
    public void PlayJumpAttackSound(Vector3 position)
    {
        PlayOneShotSound(jumpAttackBurstSound, position);
    }

    /// <summary>
    /// Phát âm thanh phân rã triệu hồi Slime con (Skill 2)
    /// </summary>
    public void PlaySummonSound(Vector3 position)
    {
        PlayOneShotSound(summonSlimeSound, position);
    }

    private void PlayOneShotSound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        GameObject tempAudio = new GameObject("TempBossSlimeAudio_Independent");
        tempAudio.transform.position = position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = clip;
        aSource.spatialBlend = 0f; // Khóa 2D nghe rõ ràng
        aSource.volume = finalVolume;

        // Route vào kênh CombatSFX
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.combatGroup;
        }

        aSource.Play();
        Destroy(tempAudio, clip.length); // Tự động dọn dẹp sau khi phát xong
    }
}