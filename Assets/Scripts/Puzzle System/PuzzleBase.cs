using UnityEngine;


public abstract class PuzzleBase : MonoBehaviour
{
    [SerializeField] protected int puzzleIndex; // 0..N
    protected bool isSolved;

    protected virtual void Solve()
    {
        if (isSolved) return;
        isSolved = true;
        PuzzleManager.Instance.MarkSolved(puzzleIndex);
    }
}
