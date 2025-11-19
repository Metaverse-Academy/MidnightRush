using UnityEngine;

public class PuzzleDebug : MonoBehaviour
{
    private FramePuzzle puzzle;
    
    void Start()
    {
        puzzle = FindFirstObjectByType<FramePuzzle>();
        if (puzzle != null)
        {
            // Debug.Log("FramePuzzle found - checking positions...");
            
            // Log all frame positions
            // Note: Replace 'Frames' with the actual property name from FramePuzzle class
            // For example, it might be 'frames', 'frameList', 'puzzleFrames', etc.
            // var frames = puzzle.GetFrames(); // or similar accessor method
            // if (frames != null && frames.Length > 0)
            // {
            //     for (int i = 0; i < frames.Length; i++)
            //     {
            //         Debug.Log($"Frame {i}: {frames[i].transform.position}");
            //     }
            // }
            
            // Log player position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Debug.Log($"Player position: {player.transform.position}");
            }
        }
    }
}