using UnityEditor.EditorTools;
using UnityEngine;

public class LightRepeller : MonoBehaviour
{
    [Header("Light Source")]
    [Tooltip("The light source that will emit the ray. If left null, this object will be used as the source.")]
    [SerializeField] private Transform lightSource;

    [Header("Repel Settings")]
    private FlashlightController flashlightController;

    [Tooltip("Layer that the enemy belongs to")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("Distance the light ray will repel enemies")]
    [SerializeField] private float repelDistance = 20f;

    [Tooltip("Radius of the light ray")]
    [SerializeField] private float repelRadius = 0.5f;
    public bool isLightActive => flashlightController != null && flashlightController.IsOn;

    private void Awake()
    {
        if (lightSource == null)
        {
            lightSource = transform;
        }

        // --- الجزء المهم ---
        if (flashlightController == null)
        {
            flashlightController = FindAnyObjectByType<FlashlightController>();
        }

        // أضف هذا الشرط للتحقق
        if (flashlightController == null)
        {
            Debug.LogError("theFlashlightController Does not exist in the scene! Please ensure there is a FlashlightController component present.");
        }
        else
        {
            Debug.Log("Success: Found and linked FlashlightController successfully!", flashlightController.gameObject);
        }
        // -----------------
    }

    private void Update()
    {
        // Debug.Log("LightRepeller isLightActive: " + isLightActive);
        if (isLightActive)
        {
            PerformRepelCast();
        }
    }
    private void PerformRepelCast()
    {
        if (!isLightActive) return;

        if (Physics.Raycast(lightSource.position, lightSource.forward, out RaycastHit hit, repelDistance))
        {
            Debug.Log("Raycast hit: " + hit.collider.name + " on layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));

            TheEnemyAI enemy = hit.collider.GetComponent<TheEnemyAI>();
            if (enemy != null)
            {
                enemy.OnLightExposed();
                Debug.Log("Light repelled enemy: " + enemy.name);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isLightActive ? Color.yellow : Color.white;
        Vector3 origin = lightSource != null ? lightSource.position : transform.position;
        Vector3 direction = lightSource != null ? lightSource.forward : transform.forward;
        Gizmos.DrawRay(origin, direction * repelDistance);
    }
}
