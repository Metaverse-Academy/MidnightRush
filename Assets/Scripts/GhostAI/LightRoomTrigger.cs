using UnityEngine;
using System.Collections;

public class LightRoomTrigger : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light roomLight;
    [SerializeField] private float lightOnDuration = 30f;
    [SerializeField] private float lightOffDuration = 20f;

    private Collider triggerCollider;
    private bool isLightOn = false;
    public bool IsLightOn() => isLightOn; // دالة ليقرأها الشبح

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true; // تأكد دائمًا أنه Trigger
    }

    void Start()
    {
        StartCoroutine(LightCycleRoutine());
    }

    private IEnumerator LightCycleRoutine()
    {
        while (true)
        {
            SetLightState(false);
            yield return new WaitForSeconds(lightOffDuration);

            SetLightState(true);
            yield return new WaitForSeconds(lightOnDuration);
        }
    }

    private void SetLightState(bool state)
    {
        isLightOn = state;
        roomLight.enabled = state;

        // إذا اشتغل الضوء، تحقق مما إذا كان الشبح بالداخل لطرده
        if (isLightOn)
        {
            CheckForGhostInside();
        }
    }

    // عند دخول أي مجسم للـ Trigger
    void OnTriggerEnter(Collider other)
    {
        // إذا كان لاعباً، حدث حالته
        var playerState = other.GetComponent<PlayerLightState>();
        if (playerState != null)
        {
            playerState.SetInDark(false);
        }
    }

    // عند خروج أي مجسم من الـ Trigger
    void OnTriggerExit(Collider other)
    {
        // إذا كان لاعباً، حدث حالته
        var playerState = other.GetComponent<PlayerLightState>();
        if (playerState != null)
        {
            playerState.SetInDark(true);
        }
    }

    // دالة للتحقق من وجود الشبح عند إضاءة الغرفة
    private void CheckForGhostInside()
    {
        // قم بتوسيع نطاق التحقق قليلاً ليشمل حدود الـ Trigger
        Collider[] objectsInside = Physics.OverlapBox(triggerCollider.bounds.center, triggerCollider.bounds.extents, transform.rotation);

        foreach (var objCollider in objectsInside)
        {
            var ghost = objCollider.GetComponent<GhostAI>();
            if (ghost != null)
            {
                // وجدنا الشبح! أمره بالاختفاء
                ghost.VanishFromLitRoom();
            }
        }
    }
}
