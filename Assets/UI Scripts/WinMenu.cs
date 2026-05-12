using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMenu : MonoBehaviour
{
    public GameObject winPanel;

    void Start()
    {
        winPanel.SetActive(false); //hides panel
    }

    public void ShowWinScreen()
    {
        winPanel.SetActive(true); //shows panel 
        Time.timeScale = 0;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Puzzle Implementation");

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}