using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private PlayerHealth player1Health;
    [SerializeField] private PlayerHealth player2Health;

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameWonUI;
    [SerializeField] private TMP_Text gameStatusText;

    [Header("Scene Management")]
    [Tooltip("Name of the main menu scene to load")]
    // [Tooltip("Name of the next level scene")]
    // [SerializeField] private string nextLevelSceneName = "Level2";

    [Header("Jump Scare")]
    [SerializeField] private JumpScareSequence jumpScareSequence;
    [SerializeField] private bool enableJumpScare = true;

    [Header("Objectives")]
    [SerializeField] private int totalObjectives = 3;
    private int completedObjectives = 0;

    private bool isGameEnded = false;
    private bool hasGameWon = false;

    void Start()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (gameWonUI != null)
            gameWonUI.SetActive(false);

        Time.timeScale = 1f;

        if (jumpScareSequence == null)
            jumpScareSequence = FindFirstObjectByType<JumpScareSequence>();

        Debug.Log("Co-op Game Started! Objectives: " + totalObjectives);
    }

    void Update()
    {
        // تحقق من صحة اللاعبين باستمرار
        if (!isGameEnded)
        {
            CheckGameStatus();
        }
    }

    public void CheckGameStatus()
    {
        if ((player1Health != null && player1Health.IsDead()) ||
            (player2Health != null && player2Health.IsDead()))
        {
            string deadPlayer = (player1Health != null && player1Health.IsDead()) ? "Player 1" : "Player 2";
            StartGameOverSequence(deadPlayer);
        }

        // إذا انجزا جميع الأهداف → Game Won
        if (completedObjectives >= totalObjectives && !hasGameWon)
        {
            StartGameWonSequence();
        }
    }
    public void CompleteObjective()
    {
        completedObjectives++;
        Debug.Log("Objective completed! " + completedObjectives + " / " + totalObjectives);

        if (gameStatusText != null)
        {
            gameStatusText.text = "Objectives: " + completedObjectives + " / " + totalObjectives;
        }

        // تأثير بصري/صوتي عند إكمال هدف
        if (completedObjectives >= totalObjectives)
        {
            Debug.Log("🎉 جميع الأهداف انجزت!");
        }
    }

    // عند وفاة أحد اللاعبين
    private void StartGameOverSequence(string deadPlayerName)
    {
        isGameEnded = true;
        Debug.Log("❌ " + deadPlayerName + " The player died. Game Over.");

        if (enableJumpScare && jumpScareSequence != null)
        {
            jumpScareSequence.Play();
            StartCoroutine(ShowGameOverAfterJumpScare(deadPlayerName));
        }
        else
        {
            ShowGameOverUI(deadPlayerName);
        }
    }

    // عند انجاز جميع الأهداف
    private void StartGameWonSequence()
    {
        hasGameWon = true;
        isGameEnded = true;
        Debug.Log("✅ You won! All objectives completed!");

        if (enableJumpScare && jumpScareSequence != null)
        {
            jumpScareSequence.Play();
            StartCoroutine(ShowGameWonAfterJumpScare());
        }
        else
        {
            ShowGameWonUI();
        }
    }

    private IEnumerator ShowGameOverAfterJumpScare(string deadPlayerName)
    {
        yield return new WaitForSeconds(0.5f);

        float jumpScareDuration = 3f;
        yield return new WaitForSeconds(jumpScareDuration);

        ShowGameOverUI(deadPlayerName);
    }

    private IEnumerator ShowGameWonAfterJumpScare()
    {
        yield return new WaitForSeconds(0.5f);

        float jumpScareDuration = 3f;
        yield return new WaitForSeconds(jumpScareDuration);

        ShowGameWonUI();
    }

    private void ShowGameOverUI(string deadPlayerName)
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        if (gameStatusText != null)
        {
            gameStatusText.text = deadPlayerName + " Died!\nGame Over";
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    private void ShowGameWonUI()
    {
        if (gameWonUI != null)
        {
            gameWonUI.SetActive(true);
        }

        if (gameStatusText != null)
        {
            gameStatusText.text = "You Won!\nAll Objectives Completed";
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    // استدعاء هذه الدالة عند إكمال لغز أو هدف
    public void OnPuzzleSolved(string puzzleName)
    {
        Debug.Log("✅ لغز انحل: " + puzzleName);
        CompleteObjective();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0);

        Debug.Log("Main menu loaded."); // Assuming main menu is at index 0
    }

    // دالة للحصول على عدد الأهداف المتبقية
    public int GetRemainingObjectives()
    {
        return totalObjectives - completedObjectives;
    }

    // دالة للتحقق من حالة اللعبة
    public bool IsGameEnded()
    {
        return isGameEnded;
    }

    public bool HasGameWon()
    {
        return hasGameWon;
    }
}