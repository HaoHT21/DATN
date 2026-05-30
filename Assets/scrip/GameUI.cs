using UnityEngine;

/// <summary>
/// Cầu nối gắn Unity UI Button → GameUIManager / GameManager.
/// Đặt script này trên Canvas hoặc object UI và kéo tham chiếu trong Inspector.
/// </summary>
public class GameUi : MonoBehaviour
{
    [SerializeField] private GameUIManager uiManager;

    private GameUIManager UI => uiManager != null ? uiManager : GameUIManager.Instance;

    public void PauseGame() => UI?.PauseGame();
    public void ResumeGame() => UI?.ResumeGame();

    public void RequestExitLevel() => UI?.RequestExitLevel();
    public void RequestExitGame() => UI?.RequestExitGame();

    // Dùng khi nút "Có" nằm ngoài ConfirmationDialog (không khuyến nghị)
    public void ConfirmExitLevel() => UI?.ConfirmExitLevel();
    public void ConfirmExitGame() => UI?.ConfirmExitGame();

    // Tương thích tên hàm cũ
    public void ContinueGame() => ResumeGame();
    public void StartGame() => UI?.StartGameplay();
    public void ShowGameOver() => UI?.ShowGameOverMenu();
}
