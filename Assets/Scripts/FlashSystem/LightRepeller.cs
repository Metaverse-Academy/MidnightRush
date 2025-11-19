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
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private bool showRayInGame = true;
    
    public bool isLightActive => flashlightController != null && flashlightController.IsOn;

    private void Awake()
    {
        if (lightSource == null)
        {
            lightSource = transform;
        }

        // Find the flashlight controller
        if (flashlightController == null)
        {
            flashlightController = FindAnyObjectByType<FlashlightController>();
        }
    }

    private void Update()
    {
        
        if (isLightActive)
        {
            PerformRepelCast();
        }
    }
    
    private void PerformRepelCast()
    {
        if (!isLightActive) 
        {
            return;
        }

        Vector3 origin = lightSource.position;
        Vector3 direction = lightSource.forward;

        // Use SphereCast for better detection
        bool hasHit = Physics.SphereCast(origin, repelRadius, direction, out RaycastHit hit, repelDistance);
        
        if (hasHit)
        {

            // Check if the hit object is on the enemy layer
            bool isOnEnemyLayer = ((1 << hit.collider.gameObject.layer) & enemyLayer) != 0;
            


            TheEnemyAI enemy = hit.collider.GetComponent<TheEnemyAI>();
            if (enemy != null)
            {
                
                enemy.OnLightExposed();
            }
            else
            {
                
                // Check if it has the component but it's disabled or missing
                enemy = hit.collider.GetComponentInParent<TheEnemyAI>();
                if (enemy != null)
                {
                    enemy.OnLightExposed();
                }
            }
        }
        else
        {
            if (enableDebug) Debug.Log("No hits detected with SphereCast");
        }

        // Additional debug: Check what objects are in the path
        if (enableDebug)
        {
            RaycastHit[] allHits = Physics.SphereCastAll(origin, repelRadius, direction, repelDistance);
            
        }
    }

    void OnDrawGizmos()
    {
        if (!showRayInGame) return;
        
        Vector3 origin = lightSource != null ? lightSource.position : transform.position;
        Vector3 direction = lightSource != null ? lightSource.forward : transform.forward;
        
        Gizmos.color = isLightActive ? Color.yellow : Color.gray;
        Gizmos.DrawRay(origin, direction * repelDistance);
        
        // Draw the sphere cast volume
        if (isLightActive)
        {
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(origin, repelRadius);
            Gizmos.DrawWireSphere(origin + direction * repelDistance, repelRadius);
            
            // Draw connecting lines
            Gizmos.DrawLine(origin + Vector3.right * repelRadius, origin + direction * repelDistance + Vector3.right * repelRadius);
            Gizmos.DrawLine(origin + Vector3.left * repelRadius, origin + direction * repelDistance + Vector3.left * repelRadius);
            Gizmos.DrawLine(origin + Vector3.up * repelRadius, origin + direction * repelDistance + Vector3.up * repelRadius);
            Gizmos.DrawLine(origin + Vector3.down * repelRadius, origin + direction * repelDistance + Vector3.down * repelRadius);
        }
    }
}