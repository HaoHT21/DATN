using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(HealthPotion))]
public class HealthPotionAudio : MonoBehaviour
{
    [Header("--- CẤU HÌNH ÂM THANH BÌNH MÁU ---")]
    [Tooltip("Kéo file âm thanh uống thuốc hoặc hồi máu (Drink/Heal Sound) vào đây")]
    public List<AudioClip> healSounds;

    [Range(0f, 100f)]
    public float soundVolume = 100f; // Thanh trượt chỉnh âm lượng ngoài Inspector

    private bool _isQuitting = false;

    private void Awake()
    {
        // Đảm bảo khi tắt game hoặc dừng Playmode không sinh ra Object âm thanh rác ngoài Hierarchy
        Application.quitting += () => _isQuitting = true;
    }

    // BẮT BUỘC CHẠY: Khi bình máu được sử dụng và gọi hàm Destroy(), Unity ép hàm này phải kích hoạt trước khi bốc hơi
    private void OnDestroy()
    {
        // Chỉ phát nhạc nếu Object thực sự bị hủy trong màn chơi (do Player sử dụng)
        if (!_isQuitting && gameObject.scene.isLoaded)
        {
            TriggerHealAudio();
        }
    }

    private void TriggerHealAudio()
    {
        if (healSounds == null || healSounds.Count == 0) return;

        // Chọn ngẫu nhiên 1 file âm thanh nếu mày bỏ vào nhiều biến thể (hoặc phát file duy nhất ở phần tử 0)
        AudioClip randomClip = healSounds[Random.Range(0, healSounds.Count)];
        if (randomClip == null) return;

        float finalVolume = Mathf.Clamp01(soundVolume / 100f);

        // Tạo Object âm thanh độc lập không lo bị biến mất theo bình máu
        GameObject tempAudio = new GameObject("TempHealthPotionAudio_Independent");
        tempAudio.transform.position = transform.position;
        AudioSource aSource = tempAudio.AddComponent<AudioSource>();

        aSource.clip = randomClip;
        aSource.spatialBlend = 0f; // Khóa chuẩn âm thanh 2D to rõ khắp màn hình
        aSource.volume = finalVolume;

        // ==========================================
        // ROUTE QUA AUDIO MIXER ĐỂ TĂNG GIẢM ÂM THANH THEO MENU
        if (AudioStaticManager.Instance != null)
        {
            aSource.outputAudioMixerGroup = AudioStaticManager.Instance.envGroup;
        }
        // ==========================================

        aSource.Play();
        Destroy(tempAudio, randomClip.length); // Phát xong tự động xóa sạch bộ nhớ
    }
}