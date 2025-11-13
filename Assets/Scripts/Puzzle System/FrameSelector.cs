using UnityEngine;

public class FrameSelector : MonoBehaviour
{
    private SlidingPuzzle puzzleController;
    
    public void SetPuzzleController(SlidingPuzzle controller)
    {
        puzzleController = controller;
    }
    
    private void OnMouseDown()
    {
        puzzleController?.OnFrameSelected(this.transform);
    }
    
    // Optional: Integrate with your existing Interactable system
    public string GetPrompt()
    {
        return "Select Frame";
    }
}