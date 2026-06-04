using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuUI;    // Kéo Canvas/Panel Menu chính vào đây
    public GameObject settingPanel;  // Kéo Panel Setting vào đây

    [Header("Intro Video Settings")]
    public VideoPlayer introVideo;   // Kéo đối tượng Video Player vào đây
    public string gameSceneName = "Level1"; // Điền tên Scene Game của bạn vào đây

    void Start()
    {
        // Lúc mới vào, đảm bảo Setting và Video đều tắt
        if (settingPanel != null) settingPanel.SetActive(false);
        if (introVideo != null)
        {
            introVideo.gameObject.SetActive(false);
            // Đăng ký sự kiện: Khi video chạy hết thì gọi hàm OnVideoFinished
            introVideo.loopPointReached += OnVideoFinished;
        }
    }

    // --- CHỨC NĂNG PLAY & INTRO ---

    public void PlayGame()
    {
        if (introVideo != null)
        {
            // 1. Ẩn Menu chính đi
            if (mainMenuUI != null) mainMenuUI.SetActive(false);

            // 2. Bật và phát Video Intro
            introVideo.gameObject.SetActive(true);
            introVideo.Play();
            Debug.Log("Đang chạy Intro...");
        }
        else
        {
            // Nếu quên không gán Video, vào thẳng game luôn
            LoadGameScene();
        }
    }

    // Hàm tự động chạy khi video kết thúc
    void OnVideoFinished(VideoPlayer vp)
    {
        LoadGameScene();
    }

    void LoadGameScene()
    {
        Debug.Log("Chuyển sang Scene Game!");
        SceneManager.LoadScene(gameSceneName);
    }

    // Nhấn phím bất kỳ để bỏ qua Intro (Skip)
    void Update()
    {
        if (introVideo != null && introVideo.isPlaying)
        {
            if (Input.anyKeyDown)
            {
                LoadGameScene();
            }
        }
    }

    // --- CHỨC NĂNG SETTING ---

    public void OpenSetting()
    {
        if (settingPanel != null) settingPanel.SetActive(true);
    }

    public void CloseSetting()
    {
        if (settingPanel != null) settingPanel.SetActive(false);
    }
}