using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{
    [Header("--- AUDIO MIXER ---")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("--- UI SLIDERS ---")]
    [SerializeField] private Slider combatSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider envSlider;

    private void Start()
    {
        // Tải lại cài đặt âm lượng đã lưu (mặc định là 0f - âm thanh gốc)
        float savedCombat = PlayerPrefs.GetFloat("CombatVolume", 0f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0f);
        float savedEnv = PlayerPrefs.GetFloat("EnvVolume", 0f);

        // Đưa giá trị lên thanh trượt UI
        combatSlider.value = savedCombat;
        musicSlider.value = savedMusic;
        envSlider.value = savedEnv;

        // Áp dụng âm lượng ngay khi vào game
        SetCombatVolume(savedCombat);
        SetMusicVolume(savedMusic);
        SetEnvVolume(savedEnv);

        // Lắng nghe sự kiện kéo thanh trượt từ người chơi
        combatSlider.onValueChanged.AddListener(SetCombatVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        envSlider.onValueChanged.AddListener(SetEnvVolume);
    }

    public void SetCombatVolume(float value)
    {
        // Nếu kéo slider về tối thiểu (-80) thì tắt hẳn âm thanh tránh tiếng rè rè của dB
        if (value <= -79f)
            audioMixer.SetFloat("combatVol", -80f);
        else
            audioMixer.SetFloat("combatVol", value);

        PlayerPrefs.SetFloat("CombatVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        if (value <= -79f)
            audioMixer.SetFloat("musicVol", -80f);
        else
            audioMixer.SetFloat("musicVol", value);

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetEnvVolume(float value)
    {
        if (value <= -79f)
            audioMixer.SetFloat("envVol", -80f);
        else
            audioMixer.SetFloat("envVol", value);

        PlayerPrefs.SetFloat("EnvVolume", value);
    }
}