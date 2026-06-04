using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource sfxSource;

    private void Awake() { Instance = this; }

    public void PlaySound(AudioClip clip)
    {
        if (sfxSource != null) sfxSource.PlayOneShot(clip);
    }
}