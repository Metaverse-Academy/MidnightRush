using UnityEngine;
using System.Collections.Generic;

public class SlidingPuzzle : MonoBehaviour
{
    [Header("Picture Frames - Assign all frames here")]
    [SerializeField] private Transform[] frames;
    
    [Header("Door to Open")]
    [SerializeField] private DoorController doorToOpen;
    
    [Header("Visual Settings")]
    [SerializeField] private float swapSpeed = 2f;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color solvedColor = Color.green;
    
    private bool solved = false;
    private Transform selectedFrame = null;
    private Vector3[] originalPositions;
    
    private void Start()
    {
        StoreOriginalPositions();
        ShuffleFrames();
    }
    
    private void StoreOriginalPositions()
    {
        originalPositions = new Vector3[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            originalPositions[i] = frames[i].position;
        }
    }
    
    private void ShuffleFrames()
    {
        // Create a copy of original positions
        List<Vector3> positions = new List<Vector3>(originalPositions);
        
        // Fisher-Yates shuffle
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector3 temp = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }
        
        // Assign shuffled positions to frames
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i].position = positions[i];
        }
        
    }
    
    public void OnFrameSelected(Transform frame)
    {
        if (solved) return;
        
        if (selectedFrame == null)
        {
            // First selection
            selectedFrame = frame;
            SetFrameColor(frame, selectedColor);
        }
        else
        {
            // Second selection - swap them
            if (selectedFrame != frame)
            {
                SwapFrames(selectedFrame, frame);
                SetFrameColor(selectedFrame, Color.white);
                selectedFrame = null;
                
                CheckSolution();
            }
            else
            {
                // Deselect if same frame clicked again
                SetFrameColor(selectedFrame, Color.white);
                selectedFrame = null;
            }
        }
    }
    
    private void SwapFrames(Transform frame1, Transform frame2)
    {
        StartCoroutine(SwapAnimation(frame1, frame2));
    }
    
    private System.Collections.IEnumerator SwapAnimation(Transform frame1, Transform frame2)
    {
        Vector3 startPos1 = frame1.position;
        Vector3 startPos2 = frame2.position;
        float elapsed = 0f;
        
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * swapSpeed;
            frame1.position = Vector3.Lerp(startPos1, startPos2, elapsed);
            frame2.position = Vector3.Lerp(startPos2, startPos1, elapsed);
            yield return null;
        }
        
        // Ensure exact final positions
        frame1.position = startPos2;
        frame2.position = startPos1;
    }
    
    private void CheckSolution()
    {
        // Check if all frames are in their original positions
        bool allCorrect = true;
        
        for (int i = 0; i < frames.Length; i++)
        {
            float distance = Vector3.Distance(frames[i].position, originalPositions[i]);
            if (distance > 0.01f) // Small tolerance for floating point errors
            {
                allCorrect = false;
                break;
            }
        }
        
        if (allCorrect)
        {
            solved = true;
            
            // Visual feedback for all frames
            foreach (var frame in frames)
            {
                SetFrameColor(frame, solvedColor);
            }
            
            if (doorToOpen != null)
            {
                doorToOpen.OpenDoor();
            }
        }
    }
    
    private void SetFrameColor(Transform frame, Color color)
    {
        MeshRenderer renderer = frame.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
    
    // Visualize original positions in editor
    private void OnDrawGizmosSelected()
    {
        if (frames == null || frames.Length == 0) return;
        
        Gizmos.color = Color.green;
        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] != null)
            {
                Vector3 pos = Application.isPlaying && originalPositions != null ? 
                    originalPositions[i] : frames[i].position;
                Gizmos.DrawWireCube(pos, Vector3.one * 0.3f);
            }
        }
    }
}