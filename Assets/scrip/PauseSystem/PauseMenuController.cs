using UnityEngine;

/// <summary>
/// Presenter / Controller: nối Input + Pause Logic + UI + Save (SRP orchestration).
/// Gắn vào Canvas Pause Menu trong scene gameplay.
/// </summary>
public sealed class PauseMenuController : MonoBehaviour
{
    [Header("Dependencies (kéo thả hoặc tự tìm)")]
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private PauseInputReader pauseInputReader;

    [Header("Tuỳ chọn")]
    [SerializeField] private bool listenEscape = true;
    [Tooltip("Khi về Main Menu cũng lưu session (an toàn dữ liệu).")]
    [SerializeField] private bool saveBeforeMainMenu = true;

    private IGamePauseService _pauseService;
    private bool _bound;

    private void Awake()
    {
        if (pauseMenuUI == null)
            pauseMenuUI = GetComponent<PauseMenuUI>();

        if (pauseInputReader == null)
            pauseInputReader = GetComponent<PauseInputReader>();

        if (pauseInputReader != null)
            pauseInputReader.Initialize(new DefaultPauseInputGate());
    }

    private void OnEnable()
    {
        TryBind();
    }

    private void Start()
    {
        // GameManager có thể Awake sau — bind lại ở Start.
        TryBind();
        SyncUiToState();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Update()
    {
        if (!listenEscape || pauseInputReader == null || _pauseService == null)
            return;

        if (pauseInputReader.WasPausePressedThisFrame())
            _pauseService.TogglePause();
    }

    private void TryBind()
    {
        if (_bound)
            return;

        if (GameManager.Instance == null)
            return;

        _pauseService = GameManager.Instance;
        _pauseService.OnStateChanged += HandleStateChanged;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.OnResumeClicked += HandleResume;
            pauseMenuUI.OnSaveGameClicked += HandleSaveGame;
            pauseMenuUI.OnMainMenuClicked += HandleMainMenu;
        }

        // ESC chỉ xử lý ở đây — tắt trùng lặp trên GameManager nếu có.
        GameManager.Instance.SetEscapeOwnedByExternal(true);
        _bound = true;
    }

    private void Unbind()
    {
        if (!_bound)
            return;

        if (_pauseService != null)
            _pauseService.OnStateChanged -= HandleStateChanged;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.OnResumeClicked -= HandleResume;
            pauseMenuUI.OnSaveGameClicked -= HandleSaveGame;
            pauseMenuUI.OnMainMenuClicked -= HandleMainMenu;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.SetEscapeOwnedByExternal(false);

        _bound = false;
        _pauseService = null;
    }

    private void HandleStateChanged(GameState state)
    {
        if (pauseMenuUI == null)
            return;

        if (state == GameState.Paused)
            pauseMenuUI.Show();
        else
            pauseMenuUI.Hide();
    }

    private void SyncUiToState()
    {
        if (_pauseService == null || pauseMenuUI == null)
            return;

        HandleStateChanged(_pauseService.CurrentState);
    }

    private void HandleResume()
    {
        _pauseService?.Resume();
    }

    private void HandleSaveGame()
    {
        GameSessionSave.SaveCurrentSession();
        Debug.Log("[PauseMenu] Đã lưu game.");
    }

    private void HandleMainMenu()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.ExitToMainMenu(saveBeforeMainMenu);
    }
}
