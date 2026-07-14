using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI manager cũ — giữ tương thích scene đã gắn sẵn.
/// Pause nên dùng PauseMenuController + GameManager.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;

    public void MainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ExitToMainMenu();
            return;
        }

        if (mainMenu != null)
            mainMenu.SetActive(true);
        if (gameOverMenu != null)
            gameOverMenu.SetActive(false);
        if (gamePauseMenu != null)
            gamePauseMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void GameOverMenu()
    {
        if (gameOverMenu != null)
            gameOverMenu.SetActive(true);
        if (mainMenu != null)
            mainMenu.SetActive(false);
        if (gamePauseMenu != null)
            gamePauseMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void PauseGameMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Pause();
            return;
        }

        if (gamePauseMenu != null)
            gamePauseMenu.SetActive(true);
        if (mainMenu != null)
            mainMenu.SetActive(false);
        if (gameOverMenu != null)
            gameOverMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        if (mainMenu != null)
            mainMenu.SetActive(false);
        if (gamePauseMenu != null)
            gamePauseMenu.SetActive(false);
        if (gameOverMenu != null)
            gameOverMenu.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.Resume();
        else
            Time.timeScale = 1f;
    }

    public void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Resume();
            return;
        }

        if (mainMenu != null)
            mainMenu.SetActive(false);
        if (gamePauseMenu != null)
            gamePauseMenu.SetActive(false);
        if (gameOverMenu != null)
            gameOverMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void SaveGame()
    {
        GameSessionSave.SaveCurrentSession();
    }

    public void LoadMainMenuScene(string sceneName = "MainMenu")
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ExitToMainMenu();
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
