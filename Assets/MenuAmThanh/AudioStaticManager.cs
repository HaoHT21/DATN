using UnityEngine;
using UnityEngine.Audio;

public class AudioStaticManager : MonoBehaviour
{
    public static AudioStaticManager Instance;

    [Header("--- AUDIO MIXER GROUPS ---")]
    public AudioMixerGroup combatGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup envGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ bộ quản lý này xuyên suốt các Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }
}