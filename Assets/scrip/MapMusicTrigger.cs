using UnityEngine;
using System.Collections;

/// <summary>
/// Thao tác đổi nhạc nền mượt mà khi Player bước vào vùng Trigger của Bản đồ.
/// </summary>
public class MapMusicTrigger : MonoBehaviour
{
    [Header("Cấu hình nhạc")]
    public AudioClip mapBGM; // Kéo file nhạc (.mp3, .wav) của Map này vào đây

    private AudioSource _currentMapAudioSource;
    private static MapMusicTrigger _activeMap; // Giữ trạng thái con map đang phát nhạc hiện tại

    private void Awake()
    {
        // Tự động thêm hoặc lấy AudioSource trên chính Object Map này để quản lý độc lập
        _currentMapAudioSource = GetComponent<AudioSource>();
        if (_currentMapAudioSource == null)
        {
            _currentMapAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Cấu hình chuẩn cho nhạc nền
        _currentMapAudioSource.clip = mapBGM;
        _currentMapAudioSource.loop = true;
        _currentMapAudioSource.playOnAwake = false;
        _currentMapAudioSource.volume = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra an toàn: Nếu không phải Player hoặc trùng map đang kích hoạt thì bỏ qua
        if (!other.CompareTag("Player") || mapBGM == null || _activeMap == this) return;

        // 1. Nếu đang có một con map khác phát nhạc, ép nó nhỏ dần rồi tắt hẳn
        if (_activeMap != null && _activeMap._currentMapAudioSource != null)
        {
            _activeMap.StopMusic();
        }

        // 2. Cập nhật con map này thành map chính
        _activeMap = this;

        // 3. Bật nhạc map mới lớn dần lên (Fade In)
        if (_currentMapAudioSource != null)
        {
            StopAllCoroutines(); // Xóa các lệnh hoãn cũ đề phòng xung đột
            _currentMapAudioSource.Play();
            StartCoroutine(FadeAudio(_currentMapAudioSource, 1f, 0.8f)); // Đẩy volume lên 1 trong 0.8 giây
        }
    }

    public void StopMusic()
    {
        if (_currentMapAudioSource != null && _currentMapAudioSource.isPlaying)
        {
            StopAllCoroutines();
            StartCoroutine(FadeAudio(_currentMapAudioSource, 0f, 0.8f, true)); // Giảm volume về 0 rồi tắt
        }
    }

    // Hàm Coroutine xử lý tăng/giảm âm lượng mượt mà không bị giật cục
    private IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration, bool stopAtEnd = false)
    {
        float currentTime = 0;
        float startVolume = source.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            if (source == null) yield break;
            source.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        if (source != null)
        {
            source.volume = targetVolume;
            if (stopAtEnd) source.Stop();
        }
    }
}