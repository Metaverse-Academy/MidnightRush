using UnityEngine;
using UnityEngine.InputSystem; // <- new Input System

public class PowerSwitch : MonoBehaviour , IInteractable
{
    [Header("Puzzle link")]
    public PowerPuzzleManager puzzleManager;   // assign in inspector

    // Implement IInteractable.Interact

    [Header("Visuals")]
    public GameObject indicatorLight;          // small light / emissive mesh
    public AudioSource audioSource;            // click sound
    public Animator animator;            // switch animation
    private Light lightComponent;
    [HideInInspector] public bool isOn;

    private bool playerInZone;

    void Awake()
    {
        lightComponent = indicatorLight.GetComponent<Light>();
    }

     public void Interact(GameObject interactor)
    {
        Toggle();
    }

    // Implement IInteractable.GetPrompt
    public string GetPrompt()
    {
        return isOn ? "Turn Off" : "Turn On";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            // optional: show "Press □ to toggle"
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }

    private void Update()
    {
        // if (!playerInZone) return;

        

        // Make sure we have a gamepad, then check SQUARE (buttonWest)
        var pad = Gamepad.current;
        if (pad != null && pad.buttonWest.wasPressedThisFrame)
        {
            // Toggle();
        }

        if (pad != null && pad.buttonWest.wasPressedThisFrame) 
        {
        Debug.Log("Square pressed");
        // Toggle();   
        }   


    }

    private void Toggle()
    {

        isOn = !isOn;
        if (lightComponent)
            lightComponent.enabled = isOn;


        if (audioSource)
            audioSource.Play();

        if (animator)
        {
            // Either use a bool parameter or just play an animation
            animator.SetBool("On", isOn);
            animator.Play("Switch"); // change to your state name
        }

        // Tell the puzzle manager something changed
        if (puzzleManager != null)
        {
            puzzleManager.OnSwitchChanged();
        }
    }
}
