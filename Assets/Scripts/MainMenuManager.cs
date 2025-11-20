using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("NewMap");
    }
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
        Debug.Log("Game quit.");
    }
}