using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour, IFlashable
{
    [Header("Flashlight Settings")]
    [SerializeField] private float workingDuration = 10f;
    [SerializeField] private float rechargeDuration = 20f;
    [SerializeField] private Light flashlightLight;
    [SerializeField] private GameObject flashlightModel;

    [Header("UI Elements")]
    [SerializeField] private Slider batterySlider;
    [SerializeField] private Image sliderFillImage;
    [SerializeField] private Text batteryText;
    [SerializeField] private GameObject rechargeIndicator;
    [SerializeField] private Text rechargeText;

    [Header("UI Colors")]
    [SerializeField] private Color fullBatteryColor = Color.green;
    [SerializeField] private Color mediumBatteryColor = Color.yellow;
    [SerializeField] private Color lowBatteryColor = Color.red;
    [SerializeField] private Color rechargingColor = Color.blue;

    private float currentBattery;
    private float rechargeTimer;
    private bool isRecharging = false;
    private Vector2 OnFlashInput;
    private bool isFlashlightActive = false;

    public bool IsWorking => !isRecharging && currentBattery > 0;
    public bool IsOn => isFlashlightActive && IsWorking;

    [Header("Audio")]
    [SerializeField] private AudioClip flashlightOnClip;
    [SerializeField] private AudioClip flashlightOffClip;

    private void Start()
    {
        currentBattery = workingDuration;
        InitializeUI();
        
        if (flashlightLight != null)
            flashlightLight.enabled = false;
    }

    private void InitializeUI()
    {
        // Initialize battery slider
        if (batterySlider != null)
        {
            batterySlider.maxValue = workingDuration;
            batterySlider.value = currentBattery;
        }

        // Initialize recharge indicator
        if (rechargeIndicator != null)
        {
            rechargeIndicator.SetActive(false);
        }

        // Initialize text elements
        UpdateBatteryText();
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
        if (ctx.performed)
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
                UpdateUI(); // Force UI update
            }
        }
    }

    public void TurnOn()
    {
        if (IsWorking)
        {
            isFlashlightActive = true;
            if (flashlightLight != null)
                flashlightLight.enabled = true;
            
            if (AudioManager.Instance != null && flashlightOnClip != null)
            {
                AudioManager.Instance.PlaySound(flashlightOnClip);
            }
        }
    }

    public void TurnOff()
    {
        isFlashlightActive = false;
        if (flashlightLight != null)
            flashlightLight.enabled = false;
        
        if (AudioManager.Instance != null && flashlightOffClip != null)
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
        UpdateBatterySlider();
        UpdateBatteryText();
        UpdateRechargeIndicator();
        UpdateSliderColors();
    }

    private void UpdateBatterySlider()
    {
        if (batterySlider != null)
        {
            if (isRecharging)
            {
                // Show recharge progress (how much time has passed)
                float rechargeProgress = (rechargeDuration - rechargeTimer) / rechargeDuration;
                batterySlider.value = rechargeProgress * workingDuration;
            }
            else
            {
                batterySlider.value = currentBattery;
            }
        }
    }

    private void UpdateBatteryText()
    {
        if (batteryText != null)
        {
            if (isRecharging)
            {
                float secondsLeft = Mathf.Ceil(rechargeTimer);
                batteryText.text = $"Recharging: {secondsLeft:F0}s";
            }
            else
            {
                float batteryPercent = (currentBattery / workingDuration) * 100f;
                batteryText.text = $"Battery: {batteryPercent:F0}%";
            }
        }
    }

    private void UpdateRechargeIndicator()
    {
        if (rechargeIndicator != null)
        {
            rechargeIndicator.SetActive(isRecharging);
            
            if (isRecharging && rechargeText != null)
            {
                float secondsLeft = Mathf.Ceil(rechargeTimer);
                rechargeText.text = $"Recharging... {secondsLeft:F0}s";
            }
        }
    }

    private void UpdateSliderColors()
    {
        if (sliderFillImage != null)
        {
            if (isRecharging)
            {
                sliderFillImage.color = rechargingColor;
            }
            else
            {
                float batteryPercent = currentBattery / workingDuration;
                
                if (batteryPercent > 0.6f)
                    sliderFillImage.color = fullBatteryColor;
                else if (batteryPercent > 0.3f)
                    sliderFillImage.color = mediumBatteryColor;
                else
                    sliderFillImage.color = lowBatteryColor;
            }
        }
    }

    // Public methods to get battery info (useful for other scripts)
    public float GetBatteryPercentage()
    {
        return currentBattery / workingDuration;
    }

    public float GetRemainingBatteryTime()
    {
        return currentBattery;
    }

    public float GetRechargeTimeLeft()
    {
        return isRecharging ? rechargeTimer : 0f;
    }

    public bool IsRecharging()
    {
        return isRecharging;
    }

    // Method to manually add battery (for power-ups)
    public void AddBattery(float amount)
    {
        if (!isRecharging)
        {
            currentBattery = Mathf.Clamp(currentBattery + amount, 0f, workingDuration);
        }
    }

    // Method to instantly recharge (for cheats or power-ups)
    public void InstantRecharge()
    {
        isRecharging = false;
        currentBattery = workingDuration;
        UpdateUI();
    }
}