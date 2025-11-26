using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Load the Instruction Scene first
        SceneManager.LoadScene("instructions");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits");
    }
}
