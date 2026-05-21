using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource effectAudioSource;   // Âm thanh hiệu ứng
    [SerializeField] private AudioSource musicAudioSource;    // Nhạc nền

    [Header("Clips")]
    [SerializeField] private AudioClip moveClip;      // Âm thanh di chuyển
    [SerializeField] private AudioClip slashClip;     // Âm thanh chém
    [SerializeField] private AudioClip lobbyMusic;    // Nhạc nền sảnh

    private void Start()
    {
        // Phát nhạc nền sảnh khi bắt đầu game
        PlayLobbyMusic();
    }

    public void PlayMoveSound()
    {
        effectAudioSource.PlayOneShot(moveClip);
    }

    public void PlaySlashSound()
    {
        effectAudioSource.PlayOneShot(slashClip);
    }

    public void PlayLobbyMusic()
    {
        musicAudioSource.clip = lobbyMusic;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }
}
