using System;
using SceneTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý trạng thái gameplay toàn cục: pause, resume, thoát màn, thoát ứng dụng.
/// Không chứa logic hiển thị UI — UIManager lắng nghe sự kiện và cập nhật giao diện.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Điều khiển")]
    [SerializeField] private bool togglePauseWithEscape = true;

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public bool IsGameplayPaused => CurrentState == GameState.Paused;

    /// <summary>Phát khi chuyển Playing ↔ Paused.</summary>
    public event Action<GameState> OnStateChanged;

    private float _timeScaleBeforePause = 1f;

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
        if (!togglePauseWithEscape)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void OnApplicationQuit()
    {
        GameSessionSave.SaveCurrentSession();
    }

    public void TogglePause()
    {
        if (IsGameplayPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (IsGameplayPaused)
            return;

        _timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        if (!IsGameplayPaused)
            return;

        SetState(GameState.Playing);
        Time.timeScale = _timeScaleBeforePause;
        AudioListener.pause = false;
    }

    public void ExitToMainMenu()
    {
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
