using UnityEngine;

public class GroundBattery : Interactable
{
    [Header("Battery Settings")]
    [SerializeField] private GameObject handBattery;
    public override void Interact(GameObject interactor)
    {
        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();

        if (player != null && !player.IsHoldingBattery)
        {
            gameObject.SetActive(false);

            if (handBattery != null)
            {
                handBattery.SetActive(true);
            }

            player.IsHoldingBattery = true;

            Debug.Log("Battery picked up!");
        }
    }
}
