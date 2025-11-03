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

        // تحقق من اللمبات القريبة
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

        // عرض رسالة التفاعل
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

    // دالة لتشغيل الفلاش من سكربت خارجي
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