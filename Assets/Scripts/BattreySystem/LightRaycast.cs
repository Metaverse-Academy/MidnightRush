using UnityEngine;
using System.Collections.Generic;

public class LightRaycast : MonoBehaviour
{
    [Header("Light Source")]
    [SerializeField] private Transform lightSource;
    [SerializeField] private Light lampLight;

    [Header("Barrier Settings")]
    [Tooltip("Layer that the enemy belongs to")]
    [SerializeField] private LayerMask enemyLayer;
    
    [Tooltip("Distance the light barrier extends")]
    [SerializeField] private float barrierDistance = 20f;
    
    [Tooltip("Width of the light barrier")]
    [SerializeField] private float barrierWidth = 2f;
    
    [Tooltip("Height of the light barrier")]
    [SerializeField] private float barrierHeight = 3f;

    [Header("Force Field Settings")]
    [Tooltip("How strongly enemies are pushed away")]
    [SerializeField] private float repelForce = 10f;
    
    [Tooltip("How close enemies can get before being repelled")]
    [SerializeField] private float minSafeDistance = 3f;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private bool showForceField = true;

    // Track enemies in the barrier area
    private HashSet<TheEnemyAI> enemiesInBarrier = new HashSet<TheEnemyAI>();
    private Dictionary<TheEnemyAI, float> enemyRepelCooldown = new Dictionary<TheEnemyAI, float>();

    public bool isLightActive => lampLight != null && lampLight.enabled && lampLight.intensity > 0.1f;

    private void Awake()
    {
        if (lightSource == null) lightSource = transform;
        if (lampLight == null) 
        {
            lampLight = GetComponent<Light>();
            if (lampLight == null) lampLight = GetComponentInChildren<Light>();
        }
    }

    private void Update()
    {
        if (isLightActive)
        {
            UpdateBarrier();
            RepelEnemiesFromBarrier();
        }
        else
        {
            // Clear tracked enemies when light is off
            enemiesInBarrier.Clear();
        }
    }

    private void UpdateBarrier()
    {
        // Clear previous frame's tracking
        HashSet<TheEnemyAI> currentFrameEnemies = new HashSet<TheEnemyAI>();

        // Create a box-shaped barrier area
        Vector3 barrierCenter = lightSource.position + lightSource.forward * (barrierDistance * 0.5f);
        Vector3 barrierSize = new Vector3(barrierWidth, barrierHeight, barrierDistance);

        // Check for enemies in the barrier volume
        Collider[] enemiesInVolume = Physics.OverlapBox(barrierCenter, barrierSize * 0.5f, lightSource.rotation, enemyLayer);

        foreach (Collider collider in enemiesInVolume)
        {
            TheEnemyAI enemy = collider.GetComponent<TheEnemyAI>();
            if (enemy != null)
            {
                currentFrameEnemies.Add(enemy);
                
                // If enemy just entered the barrier, trigger immediate reaction
                if (!enemiesInBarrier.Contains(enemy))
                {
                    OnEnemyEnterBarrier(enemy);
                }
            }
        }

        // Handle enemies that left the barrier
        foreach (TheEnemyAI enemy in enemiesInBarrier)
        {
            if (!currentFrameEnemies.Contains(enemy))
            {
                OnEnemyExitBarrier(enemy);
            }
        }

        // Update tracking
        enemiesInBarrier = currentFrameEnemies;
    }

    private void OnEnemyEnterBarrier(TheEnemyAI enemy)
    {
        Debug.Log("Enemy entered light barrier: " + enemy.name);
        
        // Immediate blindness effect
        enemy.TriggerBlindness();
        
        // Add to repel cooldown
        enemyRepelCooldown[enemy] = Time.time;
    }

    private void OnEnemyExitBarrier(TheEnemyAI enemy)
    {
        Debug.Log("Enemy exited light barrier: " + enemy.name);
        enemyRepelCooldown.Remove(enemy);
    }

    private void RepelEnemiesFromBarrier()
    {
        foreach (TheEnemyAI enemy in enemiesInBarrier)
        {
            // Cooldown check for continuous repelling
            if (enemyRepelCooldown.ContainsKey(enemy) && Time.time - enemyRepelCooldown[enemy] < 0.5f)
                continue;

            Vector3 toEnemy = enemy.transform.position - lightSource.position;
            float distanceToEnemy = toEnemy.magnitude;

            // Only repel if enemy is too close
            if (distanceToEnemy < minSafeDistance)
            {
                Vector3 repelDirection = -toEnemy.normalized;
                ApplyRepelForce(enemy, repelDirection);
                enemyRepelCooldown[enemy] = Time.time;
            }
        }
    }

    private void ApplyRepelForce(TheEnemyAI enemy, Vector3 direction)
    {
        // Use NavMeshAgent to push the enemy away
        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled)
        {
            // Calculate a safe position away from the light
            Vector3 safePosition = enemy.transform.position + direction * repelForce;
            
            // Sample NavMesh to ensure the position is valid
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(safePosition, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                
                if (showDebug)
                {
                    Debug.Log("Repelling enemy to safe position: " + hit.position);
                }
            }
        }
    }

    // Alternative: Simple sphere barrier around the light
    private void UpdateSphereBarrier()
    {
        float barrierRadius = barrierDistance;
        Collider[] enemiesNearLight = Physics.OverlapSphere(lightSource.position, barrierRadius, enemyLayer);

        foreach (Collider collider in enemiesNearLight)
        {
            TheEnemyAI enemy = collider.GetComponent<TheEnemyAI>();
            if (enemy != null)
            {
                Vector3 toEnemy = enemy.transform.position - lightSource.position;
                float distance = toEnemy.magnitude;

                // If enemy gets too close, push them back
                if (distance < minSafeDistance)
                {
                    Vector3 repelDirection = -toEnemy.normalized;
                    ApplyRepelForce(enemy, repelDirection);
                    
                    // Also trigger blindness if they're very close
                    if (distance < minSafeDistance * 0.5f)
                    {
                        enemy.TriggerBlindness();
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        Gizmos.color = isLightActive ? new Color(1, 1, 0, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.1f);

        // Draw barrier volume
        Vector3 barrierCenter = lightSource != null ? 
            lightSource.position + lightSource.forward * (barrierDistance * 0.5f) : 
            transform.position + transform.forward * (barrierDistance * 0.5f);
        
        Vector3 barrierSize = new Vector3(barrierWidth, barrierHeight, barrierDistance);
        
        Gizmos.matrix = lightSource != null ? 
            Matrix4x4.TRS(barrierCenter, lightSource.rotation, Vector3.one) :
            Matrix4x4.TRS(barrierCenter, transform.rotation, Vector3.one);
        
        Gizmos.DrawWireCube(Vector3.zero, barrierSize);

        // Draw force field radius
        if (showForceField)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawWireSphere(lightSource != null ? lightSource.position : transform.position, minSafeDistance);
        }
    }

    // Public method to check if a position is inside the barrier
    public bool IsPositionInBarrier(Vector3 position)
    {
        if (!isLightActive) return false;

        // Transform position to barrier's local space
        Vector3 localPos = lightSource.InverseTransformPoint(position);
        
        // Check if position is within barrier bounds
        return Mathf.Abs(localPos.x) < barrierWidth * 0.5f &&
               Mathf.Abs(localPos.y) < barrierHeight * 0.5f &&
               localPos.z > 0 && localPos.z < barrierDistance;
    }

    // Method that enemies can call to check if they can approach
    public bool CanApproachPosition(Vector3 position)
    {
        return !isLightActive || !IsPositionInBarrier(position);
    }
}