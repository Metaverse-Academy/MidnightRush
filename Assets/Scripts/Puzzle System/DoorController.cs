using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private string closedState = "Closed";
    [SerializeField] private string openState = "Open";
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dooropenClip;
    
    private Animator animator;
    private bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        // Ensure door starts closed
        if (animator != null)
        {
            // Force the door to play closed animation at start
            animator.Play(closedState);
            isOpen = false;
        }
    }

    public void OpenDoor()
    {
        if (!isOpen && animator != null)
        {
            if (HasAnimationParameter(animator, "Open"))
            {
                animator.SetTrigger("Open");
                isOpen = true;
                Debug.Log("Door opened!");
                audioSource.PlayOneShot(dooropenClip);

            }
            else
            {
                Debug.LogWarning("Open parameter not found in animator!");
            }
        }

    }

    private bool HasAnimationParameter(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}