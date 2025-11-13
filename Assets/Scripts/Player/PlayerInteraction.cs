using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;


public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CanvasGroup promptCanvas;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text promptText2;

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

    [Header("Startup Prompts")]

    [SerializeField] private TMP_Text startupPromptText; // Drag your new Text object here
    [SerializeField]
    private string[] startupPrompts = {
    "Use WASD to move, Mouse to look around",
    "Right Click to toggle flashlight",
    "Press E to interact with objects"

    };

    [SerializeField] private float startupDelay = 1f;

    [SerializeField] private float timeBetweenPrompts = 3f;

    [SerializeField] private float startupFadeDuration = 0.5f;


    // Add this private variable with the other private ones
    private bool hasShownStartupPrompts = false;


    private void Start()
    {
        // Show startup prompts after a delay
    StartCoroutine(ShowStartupPrompts());
    }

    private IEnumerator ShowStartupPrompts()
    {
        if (startupPromptText == null) yield break;

        // Make sure it's hidden at start
        startupPromptText.alpha = 0f;

        yield return new WaitForSeconds(startupDelay);

        foreach (string prompt in startupPrompts)
        {
            yield return StartCoroutine(ShowStartupPromptCoroutine(prompt));
            yield return new WaitForSeconds(0.5f); // Brief pause between prompts
        }

        hasShownStartupPrompts = true;
    }
    private IEnumerator ShowStartupPromptCoroutine(string prompt)
    {
        if (startupPromptText == null) yield break;

        // Set the text
        startupPromptText.text = prompt;

        // Fade in
        float timer = 0f;
        while (timer < startupFadeDuration)
        {
            timer += Time.deltaTime;
            startupPromptText.alpha = Mathf.Lerp(0f, 1f, timer / startupFadeDuration);
            yield return null;
        }
        startupPromptText.alpha = 1f;

        // Wait for display duration
        yield return new WaitForSeconds(timeBetweenPrompts);

        // Fade out
        timer = 0f;
        while (timer < startupFadeDuration)
        {
            timer += Time.deltaTime;
            startupPromptText.alpha = Mathf.Lerp(1f, 0f, timer / startupFadeDuration);
            yield return null;
        }
        startupPromptText.alpha = 0f;
    }
 

    
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
            
            ShowInteractionFeedback(currentTarget);

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

   
private void ShowInteractionFeedback(IInteractable interactedObject)
{
    string feedbackText = GetInteractionFeedback(interactedObject);
    if (!string.IsNullOrEmpty(feedbackText) && startupPromptText != null)
    {
        StartCoroutine(ShowQuickFeedbackCoroutine(feedbackText));
    }
}

    private IEnumerator ShowQuickFeedbackCoroutine(string text)
    {
        if (startupPromptText == null) yield break;

        startupPromptText.text = text;
        startupPromptText.alpha = 1f;

        yield return new WaitForSeconds(1.5f); // Show for 1.5 seconds

        // Fade out
        float timer = 0f;
        while (timer < startupFadeDuration)
        {
            timer += Time.deltaTime;
            startupPromptText.alpha = Mathf.Lerp(1f, 0f, timer / startupFadeDuration);
            yield return null;
        }
        startupPromptText.alpha = 0f;
    }


    private string GetInteractionFeedback(IInteractable interactedObject)
    {
        // You can customize this based on the type of interaction
        if (interactedObject is PickableItem)
        {
            return "Item picked up";
        }
        else if (interactedObject is PlacePoint)
        {
            return "Item placed";
        }
        // Add more types as needed

        return "Interacted";
    }


    
}