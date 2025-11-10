using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzleDoor : MonoBehaviour
{
    [SerializeField] private int puzzleIndexToUnlock; // which puzzle unlocks this door
    [SerializeField] private Animator doorAnimator;   // optional
    [SerializeField] private bool autoDisableColliderIfNoAnimator = true;

    private Collider col;
    private bool opened;

    private void Awake() { col = GetComponent<Collider>(); }
    private void Start()
    {
        PuzzleManager.Instance.OnPuzzleSolved += HandleSolved;
        // Optional: if entering scene mid-progress
        if (PuzzleManager.Instance.IsSolved(puzzleIndexToUnlock)) OpenNow();
    }
    private void OnDestroy()
    {
        if (PuzzleManager.Instance) PuzzleManager.Instance.OnPuzzleSolved -= HandleSolved;
    }

    private void HandleSolved(int index)
    {
        if (opened || index != puzzleIndexToUnlock) return;
        OpenNow();
    }

    private void OpenNow()
    {
        opened = true;
        if (doorAnimator) doorAnimator.SetTrigger("Open");
        else if (autoDisableColliderIfNoAnimator && col) col.enabled = false;
        Debug.Log($"[Door] Unlocked by puzzle {puzzleIndexToUnlock+1}");
    }
}
