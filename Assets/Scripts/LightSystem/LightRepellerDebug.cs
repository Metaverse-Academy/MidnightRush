using UnityEngine;

public class LightRepellerDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDetailedDebug = true;
    [SerializeField] private bool logEveryFrame = false;
    [SerializeField] private KeyCode debugKey = KeyCode.L;
    
    private LightRepeller lightRepeller;
    private FlashlightController flashlightController;
    private int frameCount = 0;
    private int debugInterval = 60; // Log every 60 frames if logEveryFrame is true

    void Start()
    {
        lightRepeller = GetComponent<LightRepeller>();
        flashlightController = FindObjectOfType<FlashlightController>();
        
        Debug.Log("=== LIGHT REPELLER DEBUG STARTED ===");
        Debug.Log($"LightRepeller component: {lightRepeller != null}");
        Debug.Log($"FlashlightController: {flashlightController != null}");
        
        if (lightRepeller == null)
        {
            Debug.LogError("No LightRepeller component found on this GameObject!");
            return;
        }
        
        // Use reflection to access private fields
        System.Reflection.FieldInfo enemyLayerField = typeof(LightRepeller).GetField("enemyLayer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (enemyLayerField != null)
        {
            LayerMask enemyLayer = (LayerMask)enemyLayerField.GetValue(lightRepeller);
            Debug.Log($"Enemy Layer Mask value: {enemyLayer.value}");
            Debug.Log($"Enemy Layer names: {GetLayerNamesFromMask(enemyLayer.value)}");
        }
    }

    void Update()
    {
        frameCount++;
        
        if (Input.GetKeyDown(debugKey))
        {
            RunComprehensiveDebug();
        }
        
        if (logEveryFrame && frameCount % debugInterval == 0)
        {
            RunFrameDebug();
        }
    }

    private void RunComprehensiveDebug()
    {
        Debug.Log("=== COMPREHENSIVE LIGHT REPELLER DEBUG ===");
        
        // Check LightRepeller status
        if (lightRepeller == null)
        {
            Debug.LogError("LightRepeller is null!");
            return;
        }

        // Check flashlight status
        if (flashlightController == null)
        {
            Debug.LogError("FlashlightController is null!");
            return;
        }

        Debug.Log($"Flashlight IsOn: {flashlightController.IsOn}");
        Debug.Log($"LightRepeller isLightActive: {lightRepeller.isLightActive}");

        // Check for enemies in scene
        TheEnemyAI[] allEnemies = FindObjectsOfType<TheEnemyAI>();
        Debug.Log($"Enemies in scene: {allEnemies.Length}");

        foreach (TheEnemyAI enemy in allEnemies)
        {
            Debug.Log($"Enemy: {enemy.name}, Layer: {LayerMask.LayerToName(enemy.gameObject.layer)}, " +
                     $"Position: {enemy.transform.position}, State: {enemy.currentState}");
        }

        // Test raycast manually
        TestManualRaycast();
        
        Debug.Log("=== END COMPREHENSIVE DEBUG ===");
    }

    private void RunFrameDebug()
    {
        if (!enableDetailedDebug) return;
        
        Debug.Log($"Frame {frameCount}: LightActive: {lightRepeller.isLightActive}, " +
                 $"Flashlight: {flashlightController != null && flashlightController.IsOn}");
    }

    private void TestManualRaycast()
    {
        if (lightRepeller == null) return;

        // Use reflection to access private methods and fields
        System.Reflection.FieldInfo lightSourceField = typeof(LightRepeller).GetField("lightSource", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo enemyLayerField = typeof(LightRepeller).GetField("enemyLayer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo repelDistanceField = typeof(LightRepeller).GetField("repelDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo repelRadiusField = typeof(LightRepeller).GetField("repelRadius", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (lightSourceField == null || enemyLayerField == null || repelDistanceField == null || repelRadiusField == null)
        {
            Debug.LogError("Could not access LightRepeller private fields!");
            return;
        }

        Transform lightSource = (Transform)lightSourceField.GetValue(lightRepeller);
        LayerMask enemyLayer = (LayerMask)enemyLayerField.GetValue(lightRepeller);
        float repelDistance = (float)repelDistanceField.GetValue(lightRepeller);
        float repelRadius = (float)repelRadiusField.GetValue(lightRepeller);

        Vector3 origin = lightSource.position;
        Vector3 direction = lightSource.forward;

        Debug.Log($"Manual Raycast Test:");
        Debug.Log($"- Origin: {origin}");
        Debug.Log($"- Direction: {direction}");
        Debug.Log($"- Distance: {repelDistance}");
        Debug.Log($"- Radius: {repelRadius}");
        Debug.Log($"- Enemy Layer: {enemyLayer.value}");

        // Perform the raycast
        RaycastHit[] hits = Physics.SphereCastAll(origin, repelRadius, direction, repelDistance, enemyLayer);
        
        Debug.Log($"- Hits detected: {hits.Length}");

        foreach (RaycastHit hit in hits)
        {
            Debug.Log($"  - Hit: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            
            TheEnemyAI enemy = hit.collider.GetComponent<TheEnemyAI>();
            if (enemy != null)
            {
                Debug.Log($"    - HAS TheEnemyAI COMPONENT - Should be repelled!");
            }
            else
            {
                Debug.Log($"    - No TheEnemyAI component found");
            }
        }

        if (hits.Length == 0)
        {
            // Check what IS in the path
            RaycastHit[] allHits = Physics.SphereCastAll(origin, repelRadius, direction, repelDistance);
            Debug.Log($"- Total objects in path (any layer): {allHits.Length}");
            
            foreach (RaycastHit hit in allHits)
            {
                Debug.Log($"  - Object: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            }
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
        if (lightRepeller == null || !enableDetailedDebug) return;

        // Use reflection to get private fields
        System.Reflection.FieldInfo lightSourceField = typeof(LightRepeller).GetField("lightSource", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo repelDistanceField = typeof(LightRepeller).GetField("repelDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo repelRadiusField = typeof(LightRepeller).GetField("repelRadius", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (lightSourceField == null || repelDistanceField == null || repelRadiusField == null) return;

        Transform lightSource = (Transform)lightSourceField.GetValue(lightRepeller);
        float repelDistance = (float)repelDistanceField.GetValue(lightRepeller);
        float repelRadius = (float)repelRadiusField.GetValue(lightRepeller);

        Vector3 origin = lightSource.position;
        Vector3 direction = lightSource.forward;

        // Draw debug visualization
        Gizmos.color = lightRepeller.isLightActive ? Color.green : Color.red;
        Gizmos.DrawRay(origin, direction * repelDistance);
        
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(origin, repelRadius);
        Gizmos.DrawWireSphere(origin + direction * repelDistance, repelRadius);
    }
}