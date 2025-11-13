using UnityEngine;

public class PictureFrame : MonoBehaviour
{
    [Header("Frame Visuals")]
    [SerializeField] private MeshRenderer frameRenderer;
    [SerializeField] private TextMesh letterText; // Now using TextMesh for clear letter display
    
    private Material originalMaterial;
    private SlidingPuzzle puzzleController;
    private string letter;
    private int correctIndex;
    private int currentPositionIndex;
    
    public string Letter => letter;
    public int CorrectIndex => correctIndex;
    public int CurrentPositionIndex 
    { 
        get => currentPositionIndex; 
        set => currentPositionIndex = value; 
    }
    
    public void Initialize(SlidingPuzzle controller, string frameLetter, int correctPosIndex)
    {
        puzzleController = controller;
        letter = frameLetter;
        correctIndex = correctPosIndex;
        currentPositionIndex = correctPosIndex; // Starts correct, but positions will be shuffled
        
        // Setup visual components
        if (frameRenderer != null)
        {
            originalMaterial = frameRenderer.material;
        }
        
        if (letterText != null)
        {
            letterText.text = frameLetter;
        }
        
        // Ensure there's a collider for interaction
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }
    
    public void SetSelected(bool selected, Color color)
    {
        if (frameRenderer != null)
        {
            if (selected)
            {
                frameRenderer.material.color = color;
            }
            else
            {
                frameRenderer.material.color = Color.white;
            }
        }
    }
    
    public void SetSolved(Color solvedColor)
    {
        if (frameRenderer != null)
        {
            frameRenderer.material.color = solvedColor;
        }
        
        // Optional: Disable further interaction
        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
    }
    
    // Mouse interaction
    private void OnMouseDown()
    {
        puzzleController?.OnFrameSelected(transform);
    }
    
    // Optional: Integrate with your existing Interactable system
    public string GetPrompt()
    {
        return $"Rearrange - Letter {letter}";
    }
}