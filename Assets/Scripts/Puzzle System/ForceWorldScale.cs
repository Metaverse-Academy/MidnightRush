using UnityEngine;

public class ForceWorldScale : MonoBehaviour
{
    [Header("How big you want it to LOOK in world space")]
    public Vector3 targetWorldScale = new Vector3(4f, 4f, 4f);

    void LateUpdate()
    {
        // Avoid division by zero if parent is weird
        Vector3 parentWorldScale = Vector3.one;
        if (transform.parent != null)
            parentWorldScale = transform.parent.lossyScale;

        if (parentWorldScale.x == 0) parentWorldScale.x = 0.0001f;
        if (parentWorldScale.y == 0) parentWorldScale.y = 0.0001f;
        if (parentWorldScale.z == 0) parentWorldScale.z = 0.0001f;

        // Compute localScale so that:
        // local * parentWorld = targetWorld
        Vector3 newLocal = new Vector3(
            targetWorldScale.x / parentWorldScale.x,
            targetWorldScale.y / parentWorldScale.y,
            targetWorldScale.z / parentWorldScale.z
        );

        transform.localScale = newLocal;
    }
}
