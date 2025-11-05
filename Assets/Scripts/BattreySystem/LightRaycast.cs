using UnityEngine;

public class LightRaycast : PlayerInteraction
{
    [Header("Light Source")]
    [SerializeField] private Transform lightSource;

    [Tooltip("Layer that the enemy belongs to")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("Distance the light ray will repel enemies")]
    [SerializeField] private float repelDistance = 20f;

    [Tooltip("Radius of the light ray")]
    [SerializeField] private float repelRadius = 0.5f;
    private bool isRaycastActive = false;

    private void Awake()
    {
        if (lightSource == null)
        {
            lightSource = transform;
        }
    }
    private void Update()
    {
        if (isRaycastActive)
        {
            PerformRepelCast();
        }
    }
    private void PerformRepelCast()
    {

        if (Physics.SphereCast(lightSource.position, repelRadius, lightSource.forward, out RaycastHit hit, repelDistance, enemyLayer))
        {
            EnemyAi enemy = hit.collider.GetComponent<EnemyAi>();
            if (enemy != null)
            {
                enemy.TriggerRepel();
                Debug.Log("Light repelled enemy: " + enemy.name);
            }
        }
    }
    public void SetActive(bool isActive)
    {
        isRaycastActive = isActive;
        Debug.Log("Light raycast " + (isActive ? "activated" : "deactivated"));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isRaycastActive ? Color.yellow : Color.gray;
        Vector3 origin = lightSource != null ? lightSource.position : transform.position;
        Vector3 direction = lightSource != null ? lightSource.forward : transform.forward;
        Gizmos.DrawSphere(origin + direction * repelDistance, repelRadius);
    }
}