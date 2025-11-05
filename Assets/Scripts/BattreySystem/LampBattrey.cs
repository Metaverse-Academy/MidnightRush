using UnityEngine;
using System.Collections;

public class LampBattrey : Interactable
{
    [Header("Lamp Settings")]
    [SerializeField] private GameObject handBattery;
    [SerializeField] private GameObject lampBattery;
    [SerializeField] private Light lampLight;
    [Header("Dependencies")]
    [SerializeField] private LightRaycast lightRaycastController;

    [Header("Battery Lifetime")]
    [Tooltip("Lifetime of the battery in seconds")]
    [SerializeField] private float batteryLifetime = 10f;

    [Header("UI Feedback")]
    [SerializeField] private string placeBatteryPrompt = "To place the battery, press 'E'";
    [SerializeField] private string noBatteryPrompt = "You need a battery to power the lamp.";

    private bool hasBattery = false;
    private Coroutine batteryDestroyCoroutine;

    public override void Interact(GameObject interactor)
    {
        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();

        if (player != null && player.IsHoldingBattery && !hasBattery)
        {
            PlaceBattery(player);
        }
        else if (player != null && !player.IsHoldingBattery && !hasBattery)
        {
            Debug.Log(noBatteryPrompt);
        }
    }

    public new string GetPrompt()
    {
        if (hasBattery)
        {
            return "The lamp has a battery installed.";
        }
        else
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
                if (playerInteraction != null && playerInteraction.IsHoldingBattery)
                {
                    return placeBatteryPrompt;
                }
            }
            return noBatteryPrompt;
        }
    }
    private void PlaceBattery(PlayerInteraction player)
    {
        if (handBattery != null) handBattery.SetActive(false);

        if (lampBattery != null) lampBattery.SetActive(true);

        if (lampLight != null) lampLight.enabled = true;
        if (lightRaycastController != null)
    {
        lightRaycastController.enabled = true;
    }

        player.IsHoldingBattery = false;
        hasBattery = true;

        Debug.Log("You have placed the battery in the lamp. It will last for " + batteryLifetime + " seconds.");

        batteryDestroyCoroutine = StartCoroutine(DestroyBatteryAfterDelay());
    }

    private IEnumerator DestroyBatteryAfterDelay()
    {
        yield return new WaitForSeconds(batteryLifetime);

        Debug.Log("The battery has expired and has been destroyed. You can now place a new battery.");

        if (lampLight != null)
        {
            lampLight.enabled = false;
        }

        if (lampBattery != null)
        {
            lampBattery.SetActive(false);
        }
        if (lightRaycastController != null)
    {
        lightRaycastController.enabled = false;
    }

        hasBattery = false;
        batteryDestroyCoroutine = null;
    }

    public bool HasBattery()
    {
        return hasBattery;
    }

    public float GetRemainingBatteryTime()
    {
        if (!hasBattery || batteryDestroyCoroutine == null)
            return 0f;

        return batteryLifetime;
    }
    private void OnDestroy()
    {
        if (batteryDestroyCoroutine != null)
        {
            StopCoroutine(batteryDestroyCoroutine);
        }
    }
}