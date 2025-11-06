// LightRaycast.cs  — drop-in replacement that supports Area lamps cleanly
using UnityEngine;

public class LightRaycast : MonoBehaviour
{
    [Header("Area Lamp Settings")]
    [SerializeField] private Transform lightSource;     // center of the lamp; defaults to this.transform
    [SerializeField] private LayerMask enemyLayer = ~0; // include the enemy's layer
    [SerializeField] private float areaRadius = 4f;     // how far the lamp reaches

    [Header("DarknessZone Painting (optional)")]
    [SerializeField] private bool paintDarkness = true;
    [SerializeField] private float injectStrength = 1f;      // 0..1
    [SerializeField] private float decayPerSecond = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private bool isRaycastActive = false;
    private DarknessZone[] zones;

    private void Awake()
    {
        if (!lightSource) lightSource = transform;
        zones = FindObjectsByType<DarknessZone>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        if (!isRaycastActive) return;

        // 1) Repel any enemies inside the lit area
        Vector3 origin = lightSource.position;
        var hits = Physics.OverlapSphere(origin, areaRadius, enemyLayer, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var enemy = hits[i].GetComponentInParent<EnemyAi>() ?? hits[i].GetComponent<EnemyAi>();
            if (enemy != null) enemy.TriggerRepel();
        }

        // 2) (Optional) brighten darkness zones so AI avoids this area
        if (paintDarkness && zones != null)
        {
            float r2 = areaRadius * areaRadius;
            for (int z = 0; z < zones.Length; z++)
            {
                var dz = zones[z];
                if (!dz) continue;
                if ((dz.transform.position - origin).sqrMagnitude <= r2)
                    dz.InjectLight(injectStrength, decayPerSecond);
            }
        }
    }

    public void SetActive(bool isActive)
    {
        isRaycastActive = isActive;
#if UNITY_EDITOR
        Debug.Log($"[LightRaycast] Area lamp {(isActive ? "ON" : "OFF")}");
#endif
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Vector3 origin = lightSource ? lightSource.position : transform.position;
        Gizmos.color = isRaycastActive ? Color.yellow : Color.gray;
        Gizmos.DrawWireSphere(origin, areaRadius);
    }
}
