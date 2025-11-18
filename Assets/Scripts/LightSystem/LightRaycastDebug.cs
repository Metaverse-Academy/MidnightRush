using UnityEngine;
using System.Collections.Generic;

public class LightRaycastDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private KeyCode debugKey = KeyCode.K;
    
    private LightRaycast lightRaycast;
    private HashSet<TheEnemyAI> lastFrameEnemies = new HashSet<TheEnemyAI>();

    void Start()
    {
        lightRaycast = GetComponent<LightRaycast>();
        
        Debug.Log("=== LIGHT RAYCAST DEBUG STARTED ===");
        Debug.Log($"LightRaycast component: {lightRaycast != null}");
        
        if (lightRaycast == null)
        {
            Debug.LogError("No LightRaycast component found on this GameObject!");
            return;
        }

        // Log initial settings
        System.Reflection.FieldInfo enemyLayerField = typeof(LightRaycast).GetField("enemyLayer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (enemyLayerField != null)
        {
            LayerMask enemyLayer = (LayerMask)enemyLayerField.GetValue(lightRaycast);
            Debug.Log($"Enemy Layer: {GetLayerNamesFromMask(enemyLayer.value)}");
        }

        Debug.Log($"LightRaycast isLightActive: {lightRaycast.isLightActive}");
    }

    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            RunLightRaycastDebug();
        }

        // Monitor enemy tracking changes
        if (enableDebug && lightRaycast != null && lightRaycast.isLightActive)
        {
            MonitorEnemyTracking();
        }
    }

    private void RunLightRaycastDebug()
    {
        Debug.Log("=== LIGHT RAYCAST COMPREHENSIVE DEBUG ===");
        
        if (lightRaycast == null)
        {
            Debug.LogError("LightRaycast is null!");
            return;
        }

        // Access private fields via reflection
        System.Reflection.FieldInfo enemiesInBarrierField = typeof(LightRaycast).GetField("enemiesInBarrier", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo enemyLayerField = typeof(LightRaycast).GetField("enemyLayer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo barrierDistanceField = typeof(LightRaycast).GetField("barrierDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (enemiesInBarrierField == null || enemyLayerField == null || barrierDistanceField == null)
        {
            Debug.LogError("Could not access LightRaycast private fields!");
            return;
        }

        HashSet<TheEnemyAI> enemiesInBarrier = (HashSet<TheEnemyAI>)enemiesInBarrierField.GetValue(lightRaycast);
        LayerMask enemyLayer = (LayerMask)enemyLayerField.GetValue(lightRaycast);
        float barrierDistance = (float)barrierDistanceField.GetValue(lightRaycast);

        Debug.Log($"Light Active: {lightRaycast.isLightActive}");
        Debug.Log($"Enemies in barrier: {enemiesInBarrier.Count}");
        Debug.Log($"Barrier distance: {barrierDistance}");
        Debug.Log($"Enemy Layer: {GetLayerNamesFromMask(enemyLayer.value)}");

        // Log each enemy in barrier
        foreach (TheEnemyAI enemy in enemiesInBarrier)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                Debug.Log($"  - {enemy.name} (Distance: {distance:F2}, State: {enemy.currentState})");
            }
        }

        // Test barrier volume manually
        TestBarrierVolume();
        
        Debug.Log("=== END LIGHT RAYCAST DEBUG ===");
    }

    private void MonitorEnemyTracking()
    {
        System.Reflection.FieldInfo enemiesInBarrierField = typeof(LightRaycast).GetField("enemiesInBarrier", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (enemiesInBarrierField == null) return;

        HashSet<TheEnemyAI> currentEnemies = (HashSet<TheEnemyAI>)enemiesInBarrierField.GetValue(lightRaycast);

        // Check for new enemies
        foreach (TheEnemyAI enemy in currentEnemies)
        {
            if (enemy != null && !lastFrameEnemies.Contains(enemy))
            {
                Debug.Log($"NEW ENTER: {enemy.name} entered light barrier!");
            }
        }

        // Check for exited enemies
        foreach (TheEnemyAI enemy in lastFrameEnemies)
        {
            if (enemy != null && !currentEnemies.Contains(enemy))
            {
                Debug.Log($"EXIT: {enemy.name} left light barrier!");
            }
        }

        lastFrameEnemies = new HashSet<TheEnemyAI>(currentEnemies);
    }

    private void TestBarrierVolume()
    {
        System.Reflection.FieldInfo lightSourceField = typeof(LightRaycast).GetField("lightSource", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo barrierDistanceField = typeof(LightRaycast).GetField("barrierDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo barrierWidthField = typeof(LightRaycast).GetField("barrierWidth", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo barrierHeightField = typeof(LightRaycast).GetField("barrierHeight", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo enemyLayerField = typeof(LightRaycast).GetField("enemyLayer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (lightSourceField == null || barrierDistanceField == null || 
            barrierWidthField == null || barrierHeightField == null || enemyLayerField == null) return;

        Transform lightSource = (Transform)lightSourceField.GetValue(lightRaycast);
        float barrierDistance = (float)barrierDistanceField.GetValue(lightRaycast);
        float barrierWidth = (float)barrierWidthField.GetValue(lightRaycast);
        float barrierHeight = (float)barrierHeightField.GetValue(lightRaycast);
        LayerMask enemyLayer = (LayerMask)enemyLayerField.GetValue(lightRaycast);

        Vector3 barrierCenter = lightSource.position + lightSource.forward * (barrierDistance * 0.5f);
        Vector3 barrierSize = new Vector3(barrierWidth, barrierHeight, barrierDistance);

        Collider[] enemiesInVolume = Physics.OverlapBox(barrierCenter, barrierSize * 0.5f, lightSource.rotation, enemyLayer);

        Debug.Log($"Barrier Volume Test:");
        Debug.Log($"- Center: {barrierCenter}");
        Debug.Log($"- Size: {barrierSize}");
        Debug.Log($"- Rotation: {lightSource.rotation.eulerAngles}");
        Debug.Log($"- Enemies in volume: {enemiesInVolume.Length}");

        foreach (Collider collider in enemiesInVolume)
        {
            TheEnemyAI enemy = collider.GetComponent<TheEnemyAI>();
            Debug.Log($"  - {collider.name} (Has TheEnemyAI: {enemy != null})");
        }
    }

    private string GetLayerNamesFromMask(int layerMask)
    {
        string layers = "";
        for (int i = 0; i < 32; i++)
        {
            if ((layerMask & (1 << i)) != 0)
            {
                layers += LayerMask.LayerToName(i) + ", ";
            }
        }
        return layers.Length > 0 ? layers.Substring(0, layers.Length - 2) : "None";
    }

    void OnDrawGizmos()
    {
        if (lightRaycast == null || !enableDebug) return;

        // Draw barrier volume
        System.Reflection.FieldInfo lightSourceField = typeof(LightRaycast).GetField("lightSource", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo barrierDistanceField = typeof(LightRaycast).GetField("barrierDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo barrierWidthField = typeof(LightRaycast).GetField("barrierWidth", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo barrierHeightField = typeof(LightRaycast).GetField("barrierHeight", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (lightSourceField == null || barrierDistanceField == null || 
            barrierWidthField == null || barrierHeightField == null) return;

        Transform lightSource = (Transform)lightSourceField.GetValue(lightRaycast);
        float barrierDistance = (float)barrierDistanceField.GetValue(lightRaycast);
        float barrierWidth = (float)barrierWidthField.GetValue(lightRaycast);
        float barrierHeight = (float)barrierHeightField.GetValue(lightRaycast);

        Vector3 barrierCenter = lightSource.position + lightSource.forward * (barrierDistance * 0.5f);
        Vector3 barrierSize = new Vector3(barrierWidth, barrierHeight, barrierDistance);

        Gizmos.color = lightRaycast.isLightActive ? new Color(0, 1, 1, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.1f);
        Gizmos.matrix = Matrix4x4.TRS(barrierCenter, lightSource.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, barrierSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}