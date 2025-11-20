using UnityEngine;

public class PickableItem : Interactable
{
    private Rigidbody rb;
    [SerializeField] private AudioSource audioSource;
    public AudioClip pickUpSound;

    void Awake() 
    { 
        rb = GetComponent<Rigidbody>(); 
    }

    public override string GetPrompt()
    {
        var carry = GameObject.FindObjectOfType<PlayerCarry>();
        if (carry && carry.IsHolding)
        {
            return "Drop Item";
        }
        return "Pick Up";
    }

    public override void Interact(GameObject interactor)
    {
        var carry = interactor.GetComponent<PlayerCarry>();
        if (!carry) return;
        audioSource.PlayOneShot(pickUpSound);   

        // WHY: Use the toggle method - picks up if empty hands, drops if holding
        carry.TogglePickupDrop(rb);
    }
}