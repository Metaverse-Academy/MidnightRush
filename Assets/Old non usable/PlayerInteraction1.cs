using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction2 : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup promptCanvas;
    [SerializeField] private TMP_Text promptText;

    [Header("Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private bool showDebugRay = false;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float scalePop = 1.1f;

    // NEW: where the “eyes” are relative to the player
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private Vector3 eyeLocalOffset = Vector3.zero; // e.g. slight forward offset if you want

    private IInteractable currentTarget;
    private bool promptVisible;

    private void Awake()
    {
        if (promptCanvas)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.transform.localScale = Vector3.one;
        }
    }

    private void Update()
    {
        // Player-centric origin & direction (no camera)
        Vector3 origin = transform.position + Vector3.up * eyeHeight + transform.TransformVector(eyeLocalOffset);
        Vector3 dir = transform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, interactDistance, interactableLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                currentTarget = interactable;
                ShowPrompt(interactable.GetPrompt());
            }
            else
            {
                HidePrompt();
                currentTarget = null;
            }
        }
        else
        {
            HidePrompt();
            currentTarget = null;
        }

        if (showDebugRay)
            Debug.DrawRay(origin, dir * interactDistance, currentTarget != null ? Color.green : Color.red);
    }

    private void ShowPrompt(string text)
    {
        if (!promptCanvas) return;

        promptText.text = text;

        if (!promptVisible)
        {
            promptVisible = true;

            LeanTween.cancel(promptCanvas.gameObject);
            promptCanvas.transform.localScale = Vector3.one * 0.8f;

            LeanTween.scale(promptCanvas.gameObject, Vector3.one * scalePop, fadeDuration).setEaseOutBack();
            LeanTween.alphaCanvas(promptCanvas, 1f, fadeDuration);
        }
    }

    private void HidePrompt()
    {
        if (!promptCanvas || !promptVisible) return;

        promptVisible = false;

        LeanTween.cancel(promptCanvas.gameObject);
        LeanTween.scale(promptCanvas.gameObject, Vector3.one * 0.8f, fadeDuration).setEaseInBack();
        LeanTween.alphaCanvas(promptCanvas, 0f, fadeDuration);
    }

    // Input System
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentTarget != null)
        {
            currentTarget.Interact(gameObject);
        }
    }
}
