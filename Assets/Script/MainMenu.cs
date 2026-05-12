using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        PauseMenu.isPaused = false;
        Time.timeScale = 1;

        SceneManager.LoadScene("Puzzle Implementation");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
