using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour, IFlashable
{
    [Header("Flashlight Settings")]
    [SerializeField] private float workingDuration = 10f;
    [SerializeField] private float rechargeDuration = 20f;
    [SerializeField] private Light flashlightLight;
    [SerializeField] private GameObject flashlightModel;

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Slider batterySlider;
    private float currentBattery;
    private float rechargeTimer;
    private bool isRecharging = false;
    private Vector2 OnFlashInput;
    public bool IsWorking => !isRecharging && currentBattery > 0;
    private bool isFlashlightActive = false;
    public bool IsOn => isFlashlightActive && IsWorking;
    [Header("Audio")]
    [SerializeField] private AudioClip flashlightOnClip;
    [SerializeField] private AudioClip flashlightOffClip;

    private void Start()
    {
        currentBattery = workingDuration;
        if (batterySlider != null)
            batterySlider.maxValue = workingDuration;

        if (flashlightLight != null)
            flashlightLight.enabled = false;
    }

    private void Update()
    {
        HandleInput();
        HandleBattery();
        UpdateUI();
    }

    private void HandleInput()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (!isFlashlightActive)
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }
    }

    public void OnFlash(InputAction.CallbackContext ctx)
    {
        // تأكد من أن الزر تم ضغطه (وليس تحريره أو أي شيء آخر)
        if (ctx.performed)
        {
            // استخدم نفس المنطق البسيط والمباشر
            if (!isFlashlightActive)
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }
    }


    private void HandleBattery()
    {
        if (IsOn)
        {
            currentBattery -= Time.deltaTime;
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                TurnOff();
                StartRecharge();
            }
        }

        if (isRecharging)
        {
            rechargeTimer -= Time.deltaTime;
            if (rechargeTimer <= 0)
            {
                isRecharging = false;
                currentBattery = workingDuration;
            }
        }
    }
    public void TurnOn()
    {
        if (IsWorking)
        {
            isFlashlightActive = true; // <-- تغيير هنا
            flashlightLight.enabled = true;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(flashlightOnClip);
        }
    }

    public void TurnOff()
    {
        isFlashlightActive = false; // <-- تغيير هنا
        flashlightLight.enabled = false;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(flashlightOffClip);
        }
    }
    public void StartRecharge()
    {
        isRecharging = true;
        rechargeTimer = rechargeDuration;
        TurnOff();
    }
    private void UpdateUI()
    {
        if (batterySlider != null)
        {
            if (isRecharging)
            {
                batterySlider.value = (rechargeDuration - rechargeTimer) / rechargeDuration * workingDuration;
            }
            else
            {
                batterySlider.value = currentBattery;
            }
        }
    }
}
