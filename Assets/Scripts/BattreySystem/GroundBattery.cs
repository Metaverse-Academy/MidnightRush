using UnityEngine;

public class GroundBattery : Interactable
{
    [Header("Battery Settings")]
    [SerializeField] private GameObject handBattery;

    private Collider batteryCollider;
    public override void Interact(GameObject interactor)
    {
        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();
        batteryCollider = GetComponent<Collider>();
        batteryCollider.isTrigger = true;
        if (player != null && !player.IsHoldingBattery)
        {
            gameObject.SetActive(false);

            if (handBattery != null)
            {
                handBattery.SetActive(true);
            }

            player.IsHoldingBattery = true;
        }
    }
}
