using UnityEngine;

public class PowerPuzzleManager : MonoBehaviour
{
    [Header("All 9 switches (assign in order)")]
    public PowerSwitch[] switches;          // size 9

    [Header("Required pattern (true = must be ON)")]
    public bool[] correctPattern;           // size 9, e.g. 4 true, 5 false

    [Header("What to turn on when solved")]
    public GameObject[] houseLights;        // all the lights in the house

    [Header("Optional feedback")]
    public AudioSource solvedSound;

    private bool solved;

    private void Awake()
    {
        // Link each switch back to this puzzle manager automatically (if not already set)
        foreach (var sw in switches)
        {
            if (sw != null && sw.puzzleManager == null)
            {
                sw.puzzleManager = this;
            }
        }

        if (switches.Length != correctPattern.Length)
        {
            Debug.LogError("PowerPuzzleManager: switches and correctPattern must have the same length!");
        }
    }

    // Called by each switch when it toggles
    public void OnSwitchChanged()
    {
        if (solved) return;

        if (switches.Length != correctPattern.Length) return;

        // Check all switches against the correct pattern
        for (int i = 0; i < switches.Length; i++)
        {
            if (switches[i] == null) return;

            if (switches[i].isOn != correctPattern[i])
            {
                // Not solved yet
                return;
            }
        }

        // If we reach here, pattern matches
        PuzzleSolved();
    }

    private void PuzzleSolved()
    {
        solved = true;
        Debug.Log("Power puzzle solved!");

        // Turn on ALL house lights at once
        foreach (var lightObj in houseLights)
        {
            if (lightObj)
                lightObj.SetActive(true);
        }

        // Optional: sound
        if (solvedSound)
        {
            solvedSound.Play();
        }

        // Optional: disable switches after solving
        // foreach (var sw in switches)
        //     sw.enabled = false;
    }
}
