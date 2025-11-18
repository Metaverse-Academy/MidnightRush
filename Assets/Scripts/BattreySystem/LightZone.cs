using UnityEngine;

public class LightZone : MonoBehaviour
{
    [Header("Light Settings")]
    public Light lightSource;
    public TheEnemyAI enemyAI;

    private Collider triggerCollider;
    private bool isLightActive = false;

    private void Start()
    {
        if (lightSource == null)
            lightSource = GetComponentInChildren<Light>();

        triggerCollider = GetComponent<Collider>();

        if (triggerCollider == null)
        {
            Debug.LogError("No Collider found on this GameObject!");
            return;
        }

        UpdateColliderState();
    }

    private void Update()
    {
        bool lightNowActive = (lightSource != null && lightSource.enabled && lightSource.intensity > 0.1f);

        if (lightNowActive != isLightActive)
        {
            isLightActive = lightNowActive;
            UpdateColliderState();
        }
    }

    private void UpdateColliderState()
    {
        if (triggerCollider == null)
            return;

        triggerCollider.enabled = isLightActive;

        if (isLightActive)
        {
            Debug.Log("Light turned ON → Collider ENABLED");
        }
        else
        {
            Debug.Log("Light turned OFF → Collider DISABLED");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLightActive || !other.CompareTag("Enemy"))
            return;

        TheEnemyAI ghost = other.GetComponent<TheEnemyAI>();
        if (ghost != null)
        {
            ghost.OnLightExposed();
            Debug.Log("Enemy entered light zone!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // if (!isLightActive || !other.CompareTag("Enemy"))
        //     return;

        // TheEnemyAI ghost = other.GetComponent<TheEnemyAI>();
        // if (ghost != null)
        // {
        //     ghost.OnLightExposed();
        // }

        if (other.CompareTag("Enemy"))
        {
            enemyAI.currentState = TheEnemyAI.State.Escape;
            enemyAI.OnLightExposed();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy exited the light!");
        }
    }
}