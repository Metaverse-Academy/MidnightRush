using UnityEngine;

public class GroundPazzle : Interactable
{
    [Header("Pazzle Settings")]
    [SerializeField] private GameObject handPazzle;
    [SerializeField] private string promptMessage = "Press E to pick up Cube";
    private Collider pazzleCollider;
    public override string GetPrompt()
    {
        return promptMessage;
    }
    public override void Interact(GameObject interactor)
    {
        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();
        pazzleCollider = GetComponent<Collider>();
        pazzleCollider.isTrigger = true;
        if (player != null && !player.IsHoldingPazzle)
        {
            gameObject.SetActive(false);

            if (handPazzle != null)
            {
                handPazzle.SetActive(true);
            }

            player.IsHoldingPazzle = true;
        }
    }
}