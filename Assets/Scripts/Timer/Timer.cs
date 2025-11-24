using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using NUnit.Framework;

public class Timer1 : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;

    // Default starting time (used if you run out of custom entries).
    [SerializeField] float defaultTime = 60f;

    // Optional: different times for each trigger entry
    [SerializeField] float timesPerEntry;

    private static float timeRemaining;
    private bool hasTriggeredGameOver = false;
    private int triggerCount = 0;

    bool isPlayed =false;

    void Start()
    {
        // Start with default time (or first entry if you want)
        timeRemaining = defaultTime;
    }

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

    private void ResetTimer(float newDuration)
    {
        timeRemaining = newDuration;
        hasTriggeredGameOver = false; // allow timer to run again
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isPlayed) return;
        if (!other.CompareTag("Player")) return;

        // Decide which time to use for this entry
        float newTime;

        if (timesPerEntry != null )
        {
            newTime = timesPerEntry;
            Debug.Log("NewTime");  // use the next value in the array
        }
        else
        {
            newTime = defaultTime; // fallback
        }

        triggerCount++;

        // Restart the timer with the chosen time
        ResetTimer(newTime);
        isPlayed = true;
        Debug.Log($"Trigger entered #{triggerCount}, timer reset to {newTime} seconds.");
    }
}