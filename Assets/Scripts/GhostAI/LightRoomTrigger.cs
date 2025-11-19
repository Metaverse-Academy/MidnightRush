using UnityEngine;
using System.Collections; // لإضافة الكوروتين

public class LightRoomTrigger : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("اسحب هنا اللمبة (Light Component) التي تتحكم في هذه المنطقة")]
    [SerializeField] private Light roomLight;

    [Tooltip("المدة التي تبقى فيها اللمبة مضاءة (بالثواني)")]
    [SerializeField] private float lightOnDuration = 30f;

    [Tooltip("المدة التي تبقى فيها اللمبة مطفأة (بالثواني)")]
    [SerializeField] private float lightOffDuration = 20f;

    private Collider triggerCollider; // الكولايدر الخاص بمنطقة الضوء

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null || !triggerCollider.isTrigger)
        {
            Debug.LogError("LightRoomTrigger needs a Collider set to 'Is Trigger' on the same GameObject.", this);
            this.enabled = false;
            return;
        }

        if (roomLight == null)
        {
            Debug.LogError("Room Light has not been assigned in the Inspector!", this);
            this.enabled = false;
        }
    }

    void Start()
    {
        // ابدأ دورة الإضاءة والإطفاء التلقائية
        StartCoroutine(LightCycleRoutine());
    }

    // هذا الكوروتين يدير دورة حياة اللمبة
    private IEnumerator LightCycleRoutine()
    {
        // اللعبة تبدأ والضوء مطفأ (يمكنك عكس هذا إذا أردت)
        while (true) // حلقة لا نهائية لتبديل الضوء
        {
            // --- مرحلة الإطفاء ---
            SetLightState(false);
            yield return new WaitForSeconds(lightOffDuration);

            // --- مرحلة الإضاءة ---
            SetLightState(true);
            yield return new WaitForSeconds(lightOnDuration);
        }
    }

    // دالة مركزية لتغيير حالة الضوء والـ Trigger
    public void SetLightState(bool isLightOn)
    {
        // 1. تحكم في اللمبة الفعلية
        roomLight.enabled = isLightOn;

        // 2. تحكم في الـ Trigger Collider (هذا هو الجزء الأهم)
        // إذا كان الضوء شغالاً، يجب أن يكون الـ Trigger فعالاً ليتمكن اللاعب من اكتشافه.
        // إذا كان الضوء مطفأ، نعطل الـ Trigger، وبالتالي لن يكتشفه اللاعب ويعتبر في الظلام.
        triggerCollider.enabled = isLightOn;
    }
}
