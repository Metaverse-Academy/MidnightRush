using UnityEngine;
using System.Collections;
using System;

public class LampBattrey : Interactable
{
    [Header("Lamp Settings")]
    [SerializeField] private GameObject handBattery;
    [SerializeField] private GameObject lampBattery;
    [SerializeField] private Light lampLight;

    [Header("Battery Lifetime")]
    [SerializeField] private float batteryLifetime = 10f;

    [Header("Enemy Repel Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float repelDistance = 20f;
    [SerializeField] private float repelCapsuleRadius = 0.5f;
    public Vector3 direction;

    private bool isLightOn = false;
    private bool hasBattery = false;

    private void Update()
    {
        if (!isLightOn) return;

        PerformRepelCast();
    }

    private void PerformRepelCast()
    {
        if (lampLight == null) return;

        Vector3 origin = lampLight.transform.position;
        Vector3 direction = lampLight.transform.forward;

        if (Physics.SphereCast(origin, repelCapsuleRadius, direction, out RaycastHit hit, repelDistance, enemyLayer))
        {
            if (hit.collider.TryGetComponent<EnemyAi>(out EnemyAi enemy))
            {
                enemy.TriggerRepel();
            }
        }

        Debug.DrawRay(origin, direction * repelDistance, Color.yellow);
    }

    public override void Interact(GameObject interactor)
    {
        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();
        if (player != null && player.IsHoldingBattery && !hasBattery)
        {
            PlaceBattery(player);
            StartCoroutine(DestroyBatteryAfterDelay());
        }
    }

    private void PlaceBattery(PlayerInteraction player)
    {
        if (handBattery != null) handBattery.SetActive(false);
        if (lampBattery != null) lampBattery.SetActive(true);
        if (lampLight != null) lampLight.enabled = true;

        player.IsHoldingBattery = false;
        hasBattery = true;
        isLightOn = true;

        Debug.Log("Battery placed. It will be destroyed after " + batteryLifetime + " seconds.");
    }

    private IEnumerator DestroyBatteryAfterDelay()
    {
        yield return new WaitForSeconds(batteryLifetime);

        if (lampLight != null)
        {
            lampLight.enabled = false;
        }

        if (lampBattery != null)
        {
            Destroy(lampBattery);
        }
        hasBattery = false;
        isLightOn = false;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = lampLight.transform.position;
        Vector3 endpoint = origin + lampLight.transform.forward * repelDistance;
        // Gizmos.DrawLine(origin, endpoint);
        // Gizmos.DrawWireSphere(endpoint, repelCapsuleRadius);
        Gizmos.DrawWireSphere(origin, repelCapsuleRadius);
    }
}
