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
    public bool IsOn => flashlightLight.enabled && IsWorking;

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
            if (IsWorking && !IsOn)
            {
                TurnOn();
            }
            else if (IsOn)
            {
                TurnOff();
            }
        }
    }
    public void OnFlash(InputAction.CallbackContext ctx)
{
    if (ctx.performed) // fires when button is pressed
    {
        if (IsWorking && !IsOn)
        {
            TurnOn();
        }
        else if (IsOn)
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
            flashlightLight.enabled = true;
        }
    }

    public void TurnOff()
    {
        flashlightLight.enabled = false;
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
