using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private PlayerHealth player1Health;
    [SerializeField] private PlayerHealth player2Health;

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TMP_Text winnerText;

    [Header("Scene Management")]
    [Tooltip("Name of the main menu scene to load")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isGameEnded = false;

    void Start()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        Time.timeScale = 1f;
    }
    public void CheckForWinner()
    {
        if (isGameEnded)
        {
            return;
        }

        if (player1Health.IsDead())
        {
            EndGame("Player 2");
        }
        else if (player2Health.IsDead())
        {
            EndGame("Player 1");
        }
    }

    private void EndGame(string winnerName)
    {
        isGameEnded = true;
        Debug.Log(winnerName + " Wins!");

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        if (winnerText != null)
        {
            winnerText.text = winnerName + " Wins!";
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}