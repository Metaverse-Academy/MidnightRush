using UnityEngine;
using UnityEngine.InputSystem;
public class FlashlightRaycast : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference flashAction; // Right click action

    [Header("Flash Settings")]
    public Transform flashOrigin; // Flashlight/camera
    public float maxDistance = 20f;
    public float coneAngle = 60f;
    public LayerMask enemyMask;
    public LayerMask obstructionMask;

    private void OnEnable()
    {
        flashAction.action.started += Flash;
        Debug.Log("FlashlightCone enabled and flash action subscribed.");
    }

    private void OnDisable()
    {
        flashAction.action.started -= Flash;
        Debug.Log("FlashlightCone disabled and flash action unsubscribed.");
    }

    private void Flash(InputAction.CallbackContext ctx)
    {
        // 1. Find enemies near flash
        Collider[] hits = Physics.OverlapSphere(flashOrigin.position, maxDistance, enemyMask);
        Debug.Log($"Flash activated. Found {hits.Length} potential targets.");

        foreach (var hit in hits)
        {
            Transform enemy = hit.transform;

            // 2. Check angle
            Vector3 dir = (enemy.position - flashOrigin.position).normalized;
            float angle = Vector3.Angle(flashOrigin.forward, dir);
            if (angle > coneAngle * 0.5f)
                continue;

            // 3. Raycast to ensure visible (no wall blocking)
            if (Physics.Raycast(flashOrigin.position, dir, out RaycastHit info, maxDistance, obstructionMask))
            {
                // Enemy e = info.collider.GetComponentInParent<Enemy>();
                // if (e != null)
                //     e.Fear();
                // e.destroyOnFlash = true;

                if (((1 << info.collider.gameObject.layer) & enemyMask) != 0)
                {
                    // Enemy e = info.collider.GetComponentInParent<Enemy>();
                    // if (e != null)
                    //     e.Fear();
                }
            }
        }
    }

    // Gizmo for debugging cone
    private void OnDrawGizmosSelected()
    {
        if (flashOrigin == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(flashOrigin.position, maxDistance);

        Vector3 leftLimit = Quaternion.Euler(0, -coneAngle / 2f, 0) * flashOrigin.forward;
        Vector3 rightLimit = Quaternion.Euler(0, coneAngle / 2f, 0) * flashOrigin.forward;

        Gizmos.DrawRay(flashOrigin.position, leftLimit * maxDistance);
        Gizmos.DrawRay(flashOrigin.position, rightLimit * maxDistance);
        Gizmos.DrawRay(flashOrigin.position, flashOrigin.forward * maxDistance);
    }
}