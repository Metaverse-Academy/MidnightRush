using UnityEngine;
using System.Collections.Generic;

public class FramePuzzle : MonoBehaviour
{
    [Header("Frame Positions - Assign empty GameObjects here")]
    [SerializeField] private Transform[] framePositions; // 5 set positions for frames

    [Header("Picture Frames - Assign frame objects here")]
    [SerializeField] private InteractableFrame[] frames;

    [Header("Door to Open")]
    [SerializeField] private DoorController doorToOpen;

    [Header("Visual Settings")]
    [SerializeField] private float swapSpeed = 2f;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color solvedColor = Color.green;

    private bool solved = false;
    private InteractableFrame selectedFrame = null;
    private Dictionary<InteractableFrame, Transform> frameToPositionMap = new Dictionary<InteractableFrame, Transform>();

    public object Frames { get; internal set; }

    private void Start()
    {
        if (framePositions.Length != frames.Length)
        {
            Debug.LogError($"FramePuzzle: Need exactly {frames.Length} positions for {frames.Length} frames!");
            return;
        }

        InitializeFrames();
        ShuffleFrames();

    }

    private void InitializeFrames()
    {
        // Register all frames with this puzzle
        foreach (var frame in frames)
        {
            frame.SetPuzzleController(this);
        }


        // Initially assign each frame to a position
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i].transform.SetParent(null, true);
            frames[i].transform.position = framePositions[i].position;
            frames[i].transform.rotation = framePositions[i].rotation;
            frameToPositionMap[frames[i]] = framePositions[i];
        }
    }

    private void ShuffleFrames()
    {
        // Create a list of frame indices to shuffle
        List<int> indices = new List<int>();
        for (int i = 0; i < frames.Length; i++)
        {
            indices.Add(i);
        }

        // Fisher-Yates shuffle
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = indices[i];
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }

        // Assign frames to shuffled positions
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i].transform.SetParent(null, true);
            frames[i].transform.position = framePositions[indices[i]].position;
            frames[i].transform.rotation = framePositions[indices[i]].rotation;
            frameToPositionMap[frames[i]] = framePositions[indices[i]];
        }

        Debug.Log("Frames shuffled to set positions!");
    }

    public void OnFrameInteracted(InteractableFrame frame)
    {
        if (solved) return;

        Debug.Log($"Frame interacted: {frame.name}");

        if (selectedFrame == null)
        {
            // First selection
            selectedFrame = frame;
            frame.SetSelected(true, selectedColor);
            Debug.Log($"Selected frame: {frame.name}");
        }
        else
        {
            // Second selection - swap them
            if (selectedFrame != frame)
            {
                Debug.Log($"Swapping {selectedFrame.name} with {frame.name}");
                SwapFrames(selectedFrame, frame);
                selectedFrame.SetSelected(false, Color.white);
                selectedFrame = null;

                CheckSolution();
            }
            else
            {
                // Deselect if same frame interacted again
                selectedFrame.SetSelected(false, Color.white);
                selectedFrame = null;
                Debug.Log("Deselected frame");
            }
        }
    }

    private void SwapFrames(InteractableFrame frame1, InteractableFrame frame2)
    {
        // Swap their positions in the map
        Transform tempPos = frameToPositionMap[frame1];
        frameToPositionMap[frame1] = frameToPositionMap[frame2];
        frameToPositionMap[frame2] = tempPos;

        // Start swap animation
        StartCoroutine(SwapAnimation(frame1.transform, frame2.transform));
    }

    private System.Collections.IEnumerator SwapAnimation(Transform frame1, Transform frame2)
    {
        Vector3 startPos1 = frame1.position;
        Vector3 startPos2 = frame2.position;
        Quaternion startRot1 = frame1.rotation;
        Quaternion startRot2 = frame2.rotation;

        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * swapSpeed;
            frame1.position = Vector3.Lerp(startPos1, startPos2, elapsed);
            frame2.position = Vector3.Lerp(startPos2, startPos1, elapsed);
            frame1.rotation = Quaternion.Lerp(startRot1, startRot2, elapsed);
            frame2.rotation = Quaternion.Lerp(startRot2, startRot1, elapsed);
            yield return null;
        }

        // Ensure exact final positions
        frame1.position = startPos2;
        frame2.position = startPos1;
        frame1.rotation = startRot2;
        frame2.rotation = startRot1;
    }

    private void CheckSolution()
    {
        // Check if each frame is at its original correct position
        bool allCorrect = true;

        for (int i = 0; i < frames.Length; i++)
        {
            // Frame is correct if it's at the position with the same index
            if (frameToPositionMap[frames[i]] != framePositions[i])
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            solved = true;
            Debug.Log("✅ Frame puzzle solved!");

            // Visual feedback for all frames
            foreach (var frame in frames)
            {
                frame.SetSolved(solvedColor);
                frame.SetInteractable(false);
            }

            if (doorToOpen != null)
            {
                doorToOpen.OpenDoor();
            }
        }
        else
        {
            Debug.Log("Puzzle not solved yet");
        }
    }

    // Visualize positions in editor
    private void OnDrawGizmosSelected()
    {
        if (framePositions == null) return;

        Gizmos.color = Color.cyan;
        foreach (var pos in framePositions)
        {
            if (pos != null)
            {
                Gizmos.DrawWireCube(pos.position, Vector3.one * 0.3f);
            }
        }
    }
}