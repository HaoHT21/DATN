using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;

    private bool isPaused = false;

    public void MainMenu()
    {
        mainMenu.SetActive(true);
        gameOverMenu.SetActive(false);
        gamePauseMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void GameOverMenu()
    {
        gameOverMenu.SetActive(true);
        mainMenu.SetActive(false);
        gamePauseMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void PauseGameMenu()
    {
        gamePauseMenu.SetActive(true);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        Time.timeScale = 0f; // dừng game
    }

    public void StartGame()
    {
        mainMenu.SetActive(false);
        gamePauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        Time.timeScale = 1f; // chạy game
    }

    public void ResumeGame()
    {
        mainMenu.SetActive(false);
        gamePauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        Time.timeScale = 1f; // tiếp tục game
    }

}
