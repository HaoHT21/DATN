using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class BadEndingManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject badEndPanel; // Kéo GameObject Panel vào đây

    [Header("Timeline Reference")]
    [SerializeField] private PlayableDirector director; // Kéo GameObject cutscene vào đây

    [Header("Scene Names")]
    [SerializeField] private string playSceneName = "Sanh"; // Tên scene chơi lại (Scene Sanh)
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Tên scene Menu chính

    private void OnEnable()
    {
        // Đăng ký nhận sự kiện khi Timeline chạy xong
        if (director != null)
        {
            director.stopped += OnTimelineFinished;
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện để tránh lỗi memory leak
        if (director != null)
        {
            director.stopped -= OnTimelineFinished;
        }
    }

    private void Start()
    {
        // Ẩn Panel khi mới vào Scene
        if (badEndPanel != null)
        {
            badEndPanel.SetActive(false);
        }
    }

    // Hàm gọi tự động khi Timeline kết thúc
    private void OnTimelineFinished(PlayableDirector pd)
    {
        if (badEndPanel != null)
        {
            badEndPanel.SetActive(true);
        }
    }

    // --- CÁC HÀM GẮN CHO BUTTON ---

    // Gắn hàm này vào Button "CHƠI LẠI" (QUAYLAI)
    public void RestartGame()
    {
        Time.timeScale = 1f; // Đảm bảo thời gian chạy bình thường
        SceneManager.LoadScene(playSceneName);
    }

    // Gắn hàm này vào Button "VỀ MENU" (TIEP)
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}