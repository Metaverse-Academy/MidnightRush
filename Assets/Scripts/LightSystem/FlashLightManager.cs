using UnityEngine;

public class FlashLightManager : MonoBehaviour
{
    [Header("Light References")]
    [SerializeField] private FlashlightController flashlight;
    [SerializeField] private CeilingLight[] ceilingLights;

    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;

    private void Update()
    {
        CheckInteractions();
    }

    private void CheckInteractions()
    {
        bool canInteract = false;
        string promptMessage = "";

        foreach (var light in ceilingLights)
        {
            if (IsNearLight(light.transform))
            {
                canInteract = true;
                if (light.IsWorking)
                {
                    promptMessage = light.IsOn ? "اضغط E لإطفاء اللمبة" : "اضغط E لتشغيل اللمبة";
                }
                else
                {
                    promptMessage = "اللمبة تحتاج وقت للشحن";
                }
                break;
            }
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(canInteract);
            if (canInteract && promptText != null)
            {
                promptText.text = promptMessage;
            }
        }
    }

    private bool IsNearLight(Transform lightTransform)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(lightTransform.position, player.transform.position);
            return distance < 3f;
        }
        return false;
    }

    public void ActivateFlashlight()
    {
        if (flashlight != null && flashlight.IsWorking)
        {
            flashlight.TurnOn();
        }
    }

    public void DeactivateFlashlight()
    {
        if (flashlight != null)
        {
            flashlight.TurnOff();
        }
    }
}