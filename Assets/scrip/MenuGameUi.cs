using UnityEngine;

/// <summary>
/// Điều phối hiển thị menu trong scene gameplay (Pause, Game Over, xác nhận thoát).
/// Gọi GameManager để thay đổi timeScale và trạng thái gameplay.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverMenuPanel;
    [SerializeField] private GameObject inGameMainMenuPanel;

    [Header("Xác nhận")]
    [SerializeField] private ConfirmationDialogUI confirmationDialog;

    [Header("Thông báo thoát")]
    [SerializeField] private string exitLevelMessage = "Bạn có chắc muốn thoát màn chơi?\nTiến trình sẽ được lưu.";
    [SerializeField] private string exitGameMessage = "Bạn có chắc muốn thoát game?";

    private GameManager _gameManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _gameManager = FindFirstObjectByType<GameManager>();

        if (_gameManager != null)
            _gameManager.OnStateChanged += HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
            _gameManager.OnStateChanged -= HandleGameStateChanged;

        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        HideAllMenus();
        EnsureGameRunning();
    }

    // --- Pause / Resume (gọi từ UI Button) ---

    public void PauseGame()
    {
        if (_gameManager == null)
        {
            Debug.LogWarning("[GameUIManager] Thiếu GameManager trong scene.");
            return;
        }

        confirmationDialog?.Hide();
        _gameManager.PauseGame();
        ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (_gameManager == null)
            return;

        confirmationDialog?.Hide();
        _gameManager.ResumeGame();
        HidePauseMenu();
    }

    // --- Thoát màn / Thoát game ---

    public void RequestExitLevel()
    {
        ShowConfirmation(exitLevelMessage, ConfirmExitLevel);
    }

    public void RequestExitGame()
    {
        ShowConfirmation(exitGameMessage, ConfirmExitGame);
    }

    private void ShowConfirmation(string message, System.Action onConfirm)
    {
        if (confirmationDialog == null)
        {
            onConfirm?.Invoke();
            return;
        }

        bool wasPaused = _gameManager != null && _gameManager.IsGameplayPaused;
        if (!wasPaused)
        {
            _gameManager?.PauseGame();
            HidePauseMenu();
        }

        confirmationDialog.Show(
            message,
            onConfirm,
            onCancel: () =>
            {
                if (!wasPaused)
                    _gameManager?.ResumeGame();
            });
    }

    public void ConfirmExitLevel()
    {
        if (_gameManager == null)
        {
            Debug.LogWarning("[GameUIManager] Thiếu GameManager — không thể load Main Menu.");
            return;
        }

        HideAllMenus();
        _gameManager.ExitToMainMenu();
    }

    public void ConfirmExitGame()
    {
        if (_gameManager == null)
        {
            Debug.LogWarning("[GameUIManager] Thiếu GameManager — không thể thoát ứng dụng.");
            return;
        }

        _gameManager.QuitApplication();
    }

    // --- Menu phụ (giữ tương thích code cũ) ---

    public void ShowGameOverMenu()
    {
        SetPanelActive(gameOverMenuPanel, true);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(inGameMainMenuPanel, false);
        _gameManager?.PauseGame();
    }

    public void ShowInGameMainMenu()
    {
        SetPanelActive(inGameMainMenuPanel, true);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverMenuPanel, false);
        _gameManager?.PauseGame();
    }

    public void StartGameplay()
    {
        HideAllMenus();
        _gameManager?.ResumeGame();
    }

    // Tên cũ — tránh gãy reference trong scene/prefab
    public void PauseGameMenu() => PauseGame();
    public void MainMenu() => ShowInGameMainMenu();
    public void GameOverMenu() => ShowGameOverMenu();
    public void StartGame() => StartGameplay();

    // --- Nội bộ ---

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Paused)
        {
            confirmationDialog?.Hide();
            ShowPauseMenu();
        }
        else
        {
            HidePauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        SetPanelActive(pauseMenuPanel, true);
        SetPanelActive(gameOverMenuPanel, false);
        SetPanelActive(inGameMainMenuPanel, false);
    }

    private void HidePauseMenu()
    {
        SetPanelActive(pauseMenuPanel, false);
    }

    private void HideAllMenus()
    {
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverMenuPanel, false);
        SetPanelActive(inGameMainMenuPanel, false);
        confirmationDialog?.Hide();
    }

    private void EnsureGameRunning()
    {
        if (_gameManager != null && !_gameManager.IsGameplayPaused)
            return;

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
