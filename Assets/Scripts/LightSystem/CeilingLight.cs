using UnityEngine;

public class CeilingLight : MonoBehaviour
{
    [Header("Ceiling Light Settings")]
    [SerializeField] private float workingDuration = 10f; // 10 ثواني تشغيل
    [SerializeField] private float rechargeDuration = 20f; // 20 ثانية كول داون
    [SerializeField] private Light ceilingLight;
    [SerializeField] private Material lightOnMaterial;
    [SerializeField] private Material lightOffMaterial;
    [SerializeField] private Renderer lightRenderer;

    private float currentPower;
    private float rechargeTimer;
    private bool isRecharging = false;
    private bool isOn = false;

    public bool IsWorking => !isRecharging && currentPower > 0;
    public bool IsOn => isOn && IsWorking;

    private void Start()
    {
        currentPower = workingDuration;
        if (ceilingLight != null)
            ceilingLight.enabled = false;

        UpdateLightAppearance();
    }

    private void Update()
    {
        HandlePower();
        HandleInput();
    }

    private void HandleInput()
    {
        // يمكن تشغيل اللمبة بالضغط على E مثلاً
        if (Input.GetKeyDown(KeyCode.E) && IsPlayerNear())
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

    private void HandlePower()
    {
        if (IsOn)
        {
            currentPower -= Time.deltaTime;
            if (currentPower <= 0)
            {
                currentPower = 0;
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
                currentPower = workingDuration;
            }
        }
    }

    public void TurnOn()
    {
        if (IsWorking)
        {
            isOn = true;
            ceilingLight.enabled = true;
            UpdateLightAppearance();
        }
    }

    public void TurnOff()
    {
        isOn = false;
        ceilingLight.enabled = false;
        UpdateLightAppearance();
    }

    public void StartRecharge()
    {
        isRecharging = true;
        rechargeTimer = rechargeDuration;
        TurnOff();
    }

    private void UpdateLightAppearance()
    {
        if (lightRenderer != null)
        {
            if (IsOn)
                lightRenderer.material = lightOnMaterial;
            else
                lightRenderer.material = lightOffMaterial;
        }
    }

    private bool IsPlayerNear()
    {
        // تحقق من وجود اللاعب في نطاق معين
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            return distance < 3f; // 3 وحدات مسافة
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // رسم دائرة لإظهار نطاق التفاعل
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}