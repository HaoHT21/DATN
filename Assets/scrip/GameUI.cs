using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUi : MonoBehaviour
{
    public GameUIManager gameManager; // Trỏ tới GameUIManager trong scene

    public void StartGame() => gameManager?.StartGame();
    public void QuitGame() => Application.Quit();
    public void ContinueGame() => gameManager?.ResumeGame();
    public void MainMenu() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}