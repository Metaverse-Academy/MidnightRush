using UnityEngine;

public class InteractableFrame : MonoBehaviour, IInteractable
{
    [Header("Frame Visuals")]
    [SerializeField] private MeshRenderer frameRenderer;
    
    private Material originalMaterial;
    private FramePuzzle puzzleController;
    private bool isInteractable = true;
    
    public void SetPuzzleController(FramePuzzle controller)
    {
        puzzleController = controller;
        Debug.Log($"Frame {name} registered with puzzle controller");
    }
    
    public void SetSelected(bool selected, Color color)
    {
        if (frameRenderer != null)
        {
            frameRenderer.material.color = selected ? color : Color.white;
        }
    }
    
    public void SetSolved(Color solvedColor)
    {
        if (frameRenderer != null)
        {
            frameRenderer.material.color = solvedColor;
        }
    }
    
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
    }
    
    // IInteractable implementation
    public string GetPrompt()
    {
        return "Select Frame";
    }
    
    public void Interact(GameObject interactor)
    {
        if (!isInteractable) 
        {
            Debug.Log($"Frame {name} is not interactable");
            return;
        }
        
        Debug.Log($"Interact called on frame: {name}");
        puzzleController?.OnFrameInteracted(this);
    }
    
    private void Awake()
    {
        // Ensure there's a collider for interaction
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
            Debug.Log($"Added collider to frame: {name}");
        }
        
        if (frameRenderer != null)
        {
            originalMaterial = frameRenderer.material;
        }
    }
    
    private void Start()
    {
        Debug.Log($"Frame {name} is ready for interaction");
    }
}