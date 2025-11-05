// using UnityEditor.EditorTools;
using UnityEngine;

public class LightRepeller : MonoBehaviour
{
    [Header("Light Source")]
    [Tooltip("The light source that will emit the ray. If left null, this object will be used as the source.")]
    [SerializeField] private Transform lightSource;

    [Header("Repel Settings")]
    public bool isLightActive = true;

    [Tooltip("Layer that the enemy belongs to")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("Distance the light ray will repel enemies")]
    [SerializeField] private float repelDistance = 20f;

    [Tooltip("Radius of the light ray")]
    [SerializeField] private float repelRadius = 0.5f;

    private void Awake()
    {
        if (lightSource == null)
        {
            lightSource = transform;
        }
    }

    private void Update()
    {
        if (!isLightActive) return;

        PerformRepelCast();
    }

    private void PerformRepelCast()
    {
        Vector3 origin = lightSource.position;
        Vector3 direction = lightSource.forward;

        if (Physics.CapsuleCast(origin, origin, repelRadius, direction, out RaycastHit hit, repelDistance, enemyLayer))
        {
            Debug.Log("Repeller hit: " + hit.collider.name + ". Deactivating.");
            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.Log("Repeller hit: " + hit.collider.name + ". Deactivating.");

                hit.collider.gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        Debug.DrawRay(origin, direction * repelDistance, Color.cyan);
#endif
    }
    public void ActivateLight()
    {
        isLightActive = true;
    }
    public void DeactivateLight()
    {
        isLightActive = false;
    }
}
