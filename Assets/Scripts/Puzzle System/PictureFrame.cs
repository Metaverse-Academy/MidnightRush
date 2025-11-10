using UnityEngine;

public class PictureFrame : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject highlight; // optional: glow/outline child
    private PictureOrderPuzzleSwap controller;

    public void BindController(PictureOrderPuzzleSwap c) => controller = c;

    public void SetHighlight(bool on)
    {
        if (highlight) highlight.SetActive(on);
    }

    // IInteractable
    public void Interact(GameObject interactor)
    {
        if (controller == null) return;
        controller.OnFrameClicked(this);
    }

    public string GetPrompt() => "Swap picture";
}
