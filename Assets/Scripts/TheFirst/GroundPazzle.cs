using UnityEngine;

public class GroundPazzle : MonoBehaviour, IInteractable
{
    [Header("Pazzle Settings")]
    [SerializeField] private GameObject handPazzle;
    private Collider pazzleCollider;
    public string GetPrompt()
    {
        throw new System.NotImplementedException();
    }
    public void Interact(GameObject interactor)
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