using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CanvasGroup promptCanvas;
    [SerializeField] private TMP_Text promptText;

    [Header("Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float interactDebounce = 0.12f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private bool showDebugRay = false;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float scalePop = 1.1f;
    public bool IsHoldingBattery { get; set; } = false;
    private IInteractable currentTarget;
    private bool promptVisible;
     private float cooldownUntil;

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
        Vector3 origin = cameraTransform.position;
        Vector3 dir = cameraTransform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, interactDistance, interactableLayer))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                currentTarget = interactable;
                ShowPrompt(interactable.GetPrompt());
            }
            else HidePrompt();
        }
        else HidePrompt();

        if (showDebugRay)
            Debug.DrawRay(origin, dir * interactDistance, currentTarget != null ? Color.green : Color.red);
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentTarget != null)
        {
            currentTarget.Interact(gameObject);
        }
        if (!ctx.performed) return;

    var carry = GetComponent<PlayerCarry>(); // on the player

        if (currentTarget != null)
        {
            currentTarget.Interact(gameObject);
        }
        else if (carry && carry.IsHolding)
        {
            // Drop a bit in front of the player
            Vector3 dropPos = cameraTransform.position + cameraTransform.forward * 1.0f;
            // Try to put it on the ground if there is one below the dropPos
            if (Physics.Raycast(dropPos + Vector3.up, Vector3.down, out var hit, 2.0f, ~0, QueryTriggerInteraction.Ignore))
                dropPos = hit.point;

            carry.DropTo(dropPos, Quaternion.identity);
        }
        if (carry && carry.IsHolding)
        {
            // If we’re holding: place if socket, otherwise drop
            if (currentTarget is PlacePoint socket)
            {
                socket.Interact(gameObject);            // socket will call DropTo + snap
            }
            else
            {
                carry.Drop();                           // drop anywhere
            }
            cooldownUntil = Time.time + interactDebounce;
            return;
        }

        // Not holding → call the looked-at interactable (e.g., PickableItem)
        if (currentTarget != null)
        {
            currentTarget.Interact(gameObject);
            cooldownUntil = Time.time + interactDebounce;
        }
    
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
    
}