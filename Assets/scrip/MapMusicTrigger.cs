using UnityEngine;

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
                // Gọi AudioManager để đổi nhạc nền
                AudioManager.Instance.PlayMusic(mapBGM);
            }
        }
    }
}