using SceneTransition;
using UnityEngine;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuUI;
    public GameObject settingPanel;

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

        if (introVideo != null)
        {
            introVideo.gameObject.SetActive(false);
            introVideo.loopPointReached += OnVideoFinished;
        }
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
}
