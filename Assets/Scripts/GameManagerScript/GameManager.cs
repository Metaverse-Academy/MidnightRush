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

    [Header("Jump Scare")]
    [SerializeField] private JumpScareSequence jumpScareSequence;
    [SerializeField] private bool enableJumpScare = true;

    private bool isGameEnded = false;
    private bool jumpScareCompleted = false;

    void Start()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        Time.timeScale = 1f;

        // Find jump scare if not assigned
        if (jumpScareSequence == null)
        {
            jumpScareSequence = FindFirstObjectByType<JumpScareSequence>();
        }
    }

    public void CheckForWinner()
    {
        if (isGameEnded)
        {
            return;
        }

        if (player1Health.IsDead())
        {
            StartGameEndSequence("Player 2");
        }
        else if (player2Health.IsDead())
        {
            StartGameEndSequence("Player 1");
        }
    }

    private void StartGameEndSequence(string winnerName)
    {
        isGameEnded = true;
        Debug.Log(winnerName + " Wins!");

        // Play jump scare before showing game over UI
        if (enableJumpScare && jumpScareSequence != null)
        {
            jumpScareSequence.Play();
            
            // Wait for jump scare to complete before showing game over UI
            // You can use a coroutine or callback method
            StartCoroutine(ShowGameOverAfterJumpScare(winnerName));
        }
        else
        {
            // If no jump scare, show game over immediately
            ShowGameOverUI(winnerName);
        }
    }

    private System.Collections.IEnumerator ShowGameOverAfterJumpScare(string winnerName)
    {
        // Wait a moment for jump scare to start
        yield return new WaitForSeconds(0.5f);
        
        // Wait until jump scare is complete (you might need to adjust this timing)
        // If your jump scare has a known duration, wait that long
        float jumpScareDuration = 3f; // Adjust based on your jump scare length
        yield return new WaitForSeconds(jumpScareDuration);
        
        ShowGameOverUI(winnerName);
    }

    private void ShowGameOverUI(string winnerName)
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        if (winnerText != null)
        {
            winnerText.text = winnerName + " Wins!";
        }

        Time.timeScale = 0f;
        jumpScareCompleted = true;
    }

    // Alternative method using callback from JumpScareSequence
    public void OnJumpScareCompleted()
    {
        if (isGameEnded && !jumpScareCompleted)
        {
            // This would be called from the JumpScareSequence when it finishes
            // You'll need to modify JumpScareSequence to call this method
            ShowGameOverUI(GetWinnerName());
        }
    }

    private string GetWinnerName()
    {
        if (player1Health.IsDead()) return "Player 2";
        if (player2Health.IsDead()) return "Player 1";
        return "Unknown";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Optional: If you want different jump scares for each player's death
    public void PlayPlayerDeathJumpScare(int playerNumber)
    {
        if (enableJumpScare && jumpScareSequence != null)
        {
            // You could modify JumpScareSequence to have different scare types
            jumpScareSequence.Play();
            Debug.Log("Playing jump scare for Player " + playerNumber + " death");
        }
    }
}