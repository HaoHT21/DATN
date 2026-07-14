using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View Pause Menu: chỉ hiển thị panel và phát sự kiện nút (SRP / UI layer).
/// Không gọi Time.timeScale hay Save trực tiếp.
/// </summary>
public sealed class PauseMenuUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button mainMenuButton;

    public event Action OnResumeClicked;
    public event Action OnSaveGameClicked;
    public event Action OnMainMenuClicked;

    public bool IsVisible => pausePanel != null && pausePanel.activeSelf;

    private void Awake()
    {
        WireButtons();
        HideImmediate();
    }

    private void OnDestroy()
    {
        UnwireButtons();
    }

    public void Show()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void Hide()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void HideImmediate()
    {
        Hide();
    }

    private void WireButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(HandleResume);

        if (saveGameButton != null)
            saveGameButton.onClick.AddListener(HandleSave);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(HandleMainMenu);
    }

    private void UnwireButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(HandleResume);

        if (saveGameButton != null)
            saveGameButton.onClick.RemoveListener(HandleSave);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(HandleMainMenu);
    }

    private void HandleResume() => OnResumeClicked?.Invoke();
    private void HandleSave() => OnSaveGameClicked?.Invoke();
    private void HandleMainMenu() => OnMainMenuClicked?.Invoke();
}
