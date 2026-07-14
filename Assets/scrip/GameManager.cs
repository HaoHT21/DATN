using System;
using SceneTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý trạng thái gameplay toàn cục: pause, resume, thoát màn, thoát ứng dụng.
/// Không chứa logic hiển thị UI — PauseMenuUI / PauseMenuController lắng nghe sự kiện.
/// </summary>
public class GameManager : MonoBehaviour, IGamePauseService
{
    public static GameManager Instance { get; private set; }

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Điều khiển")]
    [Tooltip("ESC nội bộ — tắt khi PauseMenuController đã sở hữu ESC.")]
    [SerializeField] private bool togglePauseWithEscape = true;

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public bool IsGameplayPaused => CurrentState == GameState.Paused;
    public bool IsPaused => IsGameplayPaused;

    /// <summary>Phát khi chuyển Playing ↔ Paused.</summary>
    public event Action<GameState> OnStateChanged;

    private float _timeScaleBeforePause = 1f;
    private bool _escapeOwnedByExternal;
    private readonly IPauseInputGate _pauseInputGate = new DefaultPauseInputGate();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!togglePauseWithEscape || _escapeOwnedByExternal)
            return;

        if (!_pauseInputGate.CanTogglePause)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void OnApplicationQuit()
    {
        GameSessionSave.SaveCurrentSession();
    }

    /// <summary>
    /// Khi PauseMenuController gắn trong scene, gọi true để tránh ESC bị xử lý hai lần.
    /// </summary>
    public void SetEscapeOwnedByExternal(bool owned)
    {
        _escapeOwnedByExternal = owned;
    }

    public void TogglePause()
    {
        if (IsGameplayPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (IsGameplayPaused)
            return;

        _timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void Resume()
    {
        if (!IsGameplayPaused)
            return;

        SetState(GameState.Playing);
        Time.timeScale = _timeScaleBeforePause;
        AudioListener.pause = false;
    }

    /// <summary>Alias tương thích API cũ.</summary>
    public void PauseGame() => Pause();

    /// <summary>Alias tương thích API cũ.</summary>
    public void ResumeGame() => Resume();

    public void ExitToMainMenu(bool saveSession = true)
    {
        if (saveSession)
            GameSessionSave.SaveCurrentSession();

        RestoreTimeBeforeSceneLoad();

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitApplication()
    {
        GameSessionSave.SaveCurrentSession();
        RestoreTimeBeforeSceneLoad();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

    private void RestoreTimeBeforeSceneLoad()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        CurrentState = GameState.Playing;
    }
}
