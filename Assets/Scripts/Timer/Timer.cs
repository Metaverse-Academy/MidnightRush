using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Timer1 : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float timeRemaining = 60f;

    private bool hasTriggeredGameOver = false;

    void Update()
    {
        if (hasTriggeredGameOver) return; // don't keep firing after time is over

        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f) timeRemaining = 0f;
        }
        else
        {
            // Timer reached zero
            hasTriggeredGameOver = true;
            GameOver();
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void GameOver()
    {
        Debug.Log("Time is up → Game Over");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeUp();
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null! Make sure there is a GameManager in the scene.");
        }
    }
}
