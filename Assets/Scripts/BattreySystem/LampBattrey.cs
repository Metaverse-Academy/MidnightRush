using UnityEngine;
using System.Collections;
public class LampBattrey : Interactable
{
    [Header("Lamp Settings")]
    [SerializeField] private GameObject handBattery;
    [SerializeField] private GameObject lampBattery;
    [SerializeField] private Light lampLight;

    [Header("Battery Lifetime")]
    [Tooltip("Time in seconds before the battery is destroyed after being placed in the lamp")]
    [SerializeField] private float batteryLifetime = 10f;
    private bool hasBattery = false;

    public override void Interact(GameObject interactor)
    {
        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();
        if (player != null && player.IsHoldingBattery && !hasBattery)
        {
            PlaceBattery(player);
            StartCoroutine(DestroyBatteryAfterDelay());
        }
    }
    private void PlaceBattery(PlayerInteraction player)
    {
        if (handBattery != null) handBattery.SetActive(false);
        if (lampBattery != null) lampBattery.SetActive(true);
        if (lampLight != null) lampLight.enabled = true;

        player.IsHoldingBattery = false;
        hasBattery = true;

        Debug.Log("Battery placed. It will be destroyed after " + batteryLifetime + " seconds.");
    }
    private IEnumerator DestroyBatteryAfterDelay()
    {
        yield return new WaitForSeconds(batteryLifetime);

        if (lampLight != null)
        {
            lampLight.enabled = false;
        }

        if (lampBattery != null)
        {
            Destroy(lampBattery);
        }
        hasBattery = false;
    }
}
