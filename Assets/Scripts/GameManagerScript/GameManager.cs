using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using NUnit.Framework;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Player Health")]
    // [SerializeField] private PlayerHealth player1Health;
    // [SerializeField] private PlayerHealth player2Health;

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameWonUI;
    [SerializeField] private TMP_Text gameStatusText;
    [SerializeField] private GameObject PauseMenuUI;

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
    public bool isGameEnded = false;
    public bool hasGameWon = false;
    private bool isPaused = false;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }


        if (gameWonUI != null)
            gameWonUI.SetActive(false);
        if (PauseMenuUI != null)
            PauseMenuUI.SetActive(false);

        Time.timeScale = 1f;

        if (jumpScareSequence == null)
            jumpScareSequence = FindFirstObjectByType<JumpScareSequence>();

        Debug.Log("Co-op Game Started! Objectives: " + totalObjectives);
    }

    void Update()
    {
        if (!isGameEnded)
        {
            CheckGameStatus();
        }
    }

    public void CheckGameStatus()
    {
        // if ((player1Health != null && player1Health.IsDead()) ||
        //     (player2Health != null && player2Health.IsDead()))
        // {
        //     string deadPlayer = (player1Health != null && player1Health.IsDead()) ? "Player 1" : "Player 2";
        //     StartGameOverSequence(deadPlayer);
        // }

        // // إذا انجزا جميع الأهداف → Game Won
        // if (completedObjectives >= totalObjectives && !hasGameWon)
        // {
        //     StartGameWonSequence();
        // }

        if (completedObjectives >= totalObjectives && !hasGameWon)
        {
            StartGameWonSequence();
        }
    }
    // في GameManager.cs
    public void CompleteObjective()
    {
        if (isGameEnded) return; // لا تكمل الأهداف إذا انتهت اللعبة بالفعل

        completedObjectives++;
        Debug.Log("Objective completed! " + completedObjectives + " / " + totalObjectives);

        if (gameStatusText != null)
        {
            gameStatusText.text = "Objectives: " + completedObjectives + " / " + totalObjectives;
        }
        CheckGameStatus();
    }


    // عند وفاة أحد اللاعبين
    private void StartGameOverSequence(string deadPlayerName)
    {
        if (isGameEnded) return;
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
        // if (isGameEnded) return; // <--- أضف هذا السطر


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
        yield return new WaitForSeconds(2f);

        float jumpScareDuration = 2.3f;
        yield return new WaitForSeconds(jumpScareDuration);

        ShowGameOverUI(deadPlayerName);
    }

    private IEnumerator ShowGameWonAfterJumpScare()
    {
        yield return new WaitForSeconds(2f);

        float jumpScareDuration = 2.3f;
        yield return new WaitForSeconds(jumpScareDuration);

        ShowGameWonUI();
    }

    private void ShowGameOverUI(string deadPlayerName)
    {
        Debug.Log("Showing Game Over UI for " + deadPlayerName);
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

    public void ShowGameWonUI()
    {

        // if (gameWonUI != null)
        // {
        //     gameWonUI.SetActive(true);
        // }

        // if (gameStatusText != null)
        // {
        //     gameStatusText.text = "You Won!\nAll Objectives Completed";
        // }
        SceneManager.LoadSceneAsync("Credits");
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
    public void PlayGame()
    {
        Time.timeScale = 1f;
        IsGameEnded();
        SceneManager.LoadScene("NewMap");
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("NewMap");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
    public void TogglePauseGame()
    {
        if (isGameEnded || hasGameWon) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            PauseMenuUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPaused = true;
        }
        else
        {
            Time.timeScale = 1f;
            PauseMenuUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isPaused = false;
        }
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
    public bool GamePaused()
    {
        return isPaused;
    }

    public void OnTimeUp()
    {
        if (isGameEnded) return;   // don't trigger twice
        StartGameOverSequence("Time's Up");
    }

}