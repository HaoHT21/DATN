using UnityEngine;

/// <summary>
/// Thao tác đổi nhạc nền khi Player bước vào vùng Trigger của Bản đồ.
/// </summary>
public class MapMusicTrigger : MonoBehaviour
{
    [Header("Cấu hình nhạc")]
    public AudioClip mapBGM; // Kéo file nhạc của Map này vào đây

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra nếu là Player bước vào
        if (other.CompareTag("Player"))
        {
            if (AudioManager.Instance != null && mapBGM != null)
            {
                // Gọi đúng hàm PlaySound có sẵn trong AudioManager gốc của bạn
                AudioManager.Instance.PlaySound(mapBGM);
            }
        }
    }
}