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
        // Tải lại cài đặt âm lượng đã lưu (mặc định là 1f - âm thanh gốc to nhất)
        float savedCombat = PlayerPrefs.GetFloat("CombatVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedEnv = PlayerPrefs.GetFloat("EnvVolume", 1f);

        // Đưa giá trị tuyến tính (0 -> 1) lên thanh trượt UI
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
        // Chuyển đổi giá trị Slider từ tuyến tính (0 -> 1) sang Logarit (Decibel)
        if (value <= 0.0001f)
        {
            audioMixer.SetFloat("combatVol", -80f); // Tắt hẳn tiếng khi kéo về hết bên trái
        }
        else
        {
            float dB = Mathf.Log10(value) * 20f;
            audioMixer.SetFloat("combatVol", dB);
        }

        PlayerPrefs.SetFloat("CombatVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        if (value <= 0.0001f)
        {
            audioMixer.SetFloat("musicVol", -80f);
        }
        else
        {
            float dB = Mathf.Log10(value) * 20f;
            audioMixer.SetFloat("musicVol", dB);
        }

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetEnvVolume(float value)
    {
        if (value <= 0.0001f)
        {
            audioMixer.SetFloat("envVol", -80f);
        }
        else
        {
            float dB = Mathf.Log10(value) * 20f;
            audioMixer.SetFloat("envVol", dB);
        }

        PlayerPrefs.SetFloat("EnvVolume", value);
    }
}