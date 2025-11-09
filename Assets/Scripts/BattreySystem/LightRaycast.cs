using UnityEngine;

public class LightRaycast : MonoBehaviour // تم تغيير الوراثة إلى MonoBehaviour فقط
{
    [Header("Light Source")]
    [SerializeField] private Transform lightSource;
    [SerializeField] private Light lampLight; // المكون Light الخاص بالأباجورة

    [Header("Repel Settings")]
    [Tooltip("Layer that the enemy belongs to")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("Distance the light ray will repel enemies")]
    [SerializeField] private float repelDistance = 20f;

    [Tooltip("Radius of the light ray")]
    [SerializeField] private float repelRadius = 0.5f;

    // خاصية للتحقق من حالة الضوء
    // الضوء يكون نشطًا إذا كان مكون Light موجودًا ومفعلًا
    public bool isLightActive => lampLight != null && lampLight.enabled;

    private void Awake()
    {
        // إذا لم يتم تعيين lightSource، استخدم Transform الخاص بهذا الكائن
        if (lightSource == null)
        {
            lightSource = transform;
        }
        // إذا لم يتم تعيين lampLight، حاول إيجاده في هذا الكائن
        if (lampLight == null)
        {
            lampLight = GetComponent<Light>();
        }
    }

    private void Update()
    {
        // نفذ الـ Raycast فقط إذا كان الضوء نشطًا
        if (isLightActive)
        {
            PerformRepelCast();
        }
    }

    private void PerformRepelCast()
    {
        // SphereCast ينطلق من مصدر الضوء وفي اتجاهه
        if (Physics.SphereCast(lightSource.position, repelRadius, lightSource.forward, out RaycastHit hit, repelDistance, enemyLayer))
        {
            // محاولة الحصول على سكربت TheEnemyAI
            TheEnemyAI enemy = hit.collider.GetComponent<TheEnemyAI>();

            if (enemy != null)
            {
                // استدعاء دالة العمى والهروب
                enemy.TriggerBlindness();
                Debug.Log("Lamp Light blinded enemy: " + enemy.name);
            }
        }
    }

    void OnDrawGizmos()
    {
        // رسم Gizmos لتوضيح نطاق الـ Raycast في محرر Unity
        Gizmos.color = isLightActive ? Color.yellow : Color.white;

        Vector3 origin = lightSource != null ? lightSource.position : transform.position;
        Vector3 direction = lightSource != null ? lightSource.forward : transform.forward;

        // رسم كرة في نهاية نطاق الـ Raycast
        Gizmos.DrawWireSphere(origin + direction * repelDistance, repelRadius);

        // رسم خط يمثل اتجاه الـ Raycast
        Gizmos.DrawLine(origin, origin + direction * repelDistance);
    }
}
