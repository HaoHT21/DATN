using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip); // Phát tiếng mà không làm ngắt tiếng cũ
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        // Nếu nhạc đang phát chính là bản nhạc này thì không làm gì cả
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.loop = true; // Luôn lặp lại nhạc nền
        musicSource.Play();
    }
}