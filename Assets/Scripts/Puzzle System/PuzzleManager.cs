using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    public event System.Action<int> OnPuzzleSolved;
    private bool[] solvedPuzzles;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        solvedPuzzles = new bool[8]; // headroom
    }

    public void MarkSolved(int index)
    {
        if (index < 0 || index >= solvedPuzzles.Length) return;
        if (solvedPuzzles[index]) return;
        solvedPuzzles[index] = true;
        OnPuzzleSolved?.Invoke(index);
        Debug.Log($"[PuzzleManager] Puzzle {index+1} solved");
    }

    public bool IsSolved(int index) => (index >= 0 && index < solvedPuzzles.Length) && solvedPuzzles[index];
}
