using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Puzzle Implementation");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
