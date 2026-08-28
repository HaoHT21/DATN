using SceneTransition;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuUI;
    public GameObject settingPanel;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton; // Nút Exit/Quit trong Menu (nếu có)

    [Header("Intro Video Settings")]
    public VideoPlayer introVideo;
    public string gameSceneName = "Sanh";

    [Header("Chuyển cảnh sau Intro")]
    [SerializeField] private SceneTransitionMode transitionMode = SceneTransitionMode.Asynchronous;

    private bool _isEnteringGame;

    void Start()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (continueButton != null)
            continueButton.interactable = SaveGameService.HasSave();

        // Gán sự kiện click cho QuitButton nếu được kéo thả trong Inspector
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (introVideo != null)
        {
            introVideo.gameObject.SetActive(false);
            introVideo.loopPointReached += OnVideoFinished;
        }
        Debug.Log("Đường dẫn file save: " + Application.persistentDataPath);
    }

    void OnDestroy()
    {
        if (introVideo != null)
            introVideo.loopPointReached -= OnVideoFinished;
    }

    /// <summary>Bắt đầu intro — transition chỉ chạy khi video kết thúc hoặc người chơi skip.</summary>
    public void PlayGame()
    {
        if (_isEnteringGame)
            return;

        if (introVideo != null)
        {
            if (mainMenuUI != null)
                mainMenuUI.SetActive(false);

            introVideo.gameObject.SetActive(true);
            introVideo.Play();
            return;
        }

        BeginTransitionToGame();
    }

    /// <summary>
    /// New Game:
    /// Xóa save cũ → Reset dữ liệu Hero đã sở hữu về mặc định → load scene đầu tiên → tạo save mới sau khi scene load xong.
    /// </summary>
    public void NewGame()
    {
        // Reset danh sách Hero đã sở hữu trong RAM về ban đầu
        HeroSlot.ResetOwnedHeroes();

        // Xóa Key lưu cũ nếu có
        PlayerPrefs.DeleteKey("OwnedHeroes");
        PlayerPrefs.Save();

        // Xóa file Save và bắt đầu Game Mới
        SaveGameService.DeleteSave();
        SaveGameRuntime.BeginNewGameCreateSaveAfterLoad();
        PlayGame();
    }

    /// <summary>
    /// Continue:
    /// Load save → load scene đã lưu → restore dữ liệu → tiếp tục chơi.
    /// </summary>
    public void Continue()
    {
        SaveData data = SaveGameService.Load();
        if (data == null || string.IsNullOrWhiteSpace(data.sceneName))
            return;

        if (_isEnteringGame)
            return;

        _isEnteringGame = true;

        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        SaveGameRuntime.BeginContinue(data);

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError(
                "[MenuManager] Thiếu SceneTransitionSystem trong MainMenu. " +
                "Không load thẳng để tránh bỏ qua fade — hãy thêm prefab Scene Transition.");
            _isEnteringGame = false;
            return;
        }

        SceneTransitionManager.Instance.LoadScene(
            new SceneTransitionRequest(data.sceneName, transitionMode));
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        BeginTransitionToGame();
    }

    void Update()
    {
        if (_isEnteringGame || introVideo == null || !introVideo.isPlaying)
            return;

        if (Input.anyKeyDown)
            SkipIntroAndBeginTransition();
    }

    void SkipIntroAndBeginTransition()
    {
        if (_isEnteringGame)
            return;

        introVideo.Stop();
        introVideo.gameObject.SetActive(false);
        BeginTransitionToGame();
    }

    void BeginTransitionToGame()
    {
        if (_isEnteringGame)
            return;

        _isEnteringGame = true;

        if (introVideo != null)
        {
            introVideo.Stop();
            introVideo.gameObject.SetActive(false);
        }

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError(
                "[MenuManager] Thiếu SceneTransitionSystem trong MainMenu. " +
                "Không load thẳng để tránh bỏ qua fade — hãy thêm prefab Scene Transition.");
            _isEnteringGame = false;
            return;
        }

        Debug.Log($"Intro xong — fade out rồi load '{gameSceneName}'.");
        SceneTransitionManager.Instance.LoadScene(
            new SceneTransitionRequest(gameSceneName, transitionMode));
    }

    public void OpenSetting()
    {
        if (settingPanel != null)
            settingPanel.SetActive(true);
    }

    public void CloseSetting()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);
    }

    // ==========================================
    // TÍNH NĂNG THOÁT GAME (QUIT GAME)
    // ==========================================

    /// <summary>
    /// Hàm gọi khi nhấn nút Exit/Quit Game trong Menu UI
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[MenuManager] Người chơi đang thoát game...");

#if UNITY_EDITOR
        // Tắt Play Mode nếu đang chạy trong Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Thoát ứng dụng hoàn toàn khi đã Build game
        Application.Quit();
#endif
    }
}