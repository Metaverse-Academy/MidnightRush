using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [Header("Where the item sits in front of the camera")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Vector3 defaultLocalOffset = new Vector3(0f, -0.15f, 0.7f);

    private Rigidbody held;
    private Transform prevParent;
    private int prevLayer;

    public bool IsHolding => held != null;
    public Rigidbody HeldBody => held;

    void Awake()
    {
        if (!holdPoint)
        {
            var cam = Camera.main ? Camera.main.transform : null;
            if (cam)
            {
                var hp = new GameObject("HoldPoint").transform;
                hp.SetParent(cam, false);
                hp.localPosition = defaultLocalOffset;
                hp.localRotation = Quaternion.identity;
                hp.localScale    = Vector3.one; // important: no weird scale
                holdPoint        = hp;
            }
            else
            {
                Debug.LogWarning("PlayerCarry: No holdPoint and no Camera.main. Assign a HoldPoint in the inspector!");
            }
        }

        // Also make sure THIS object and the camera are scale (1,1,1)
        if (transform.lossyScale != Vector3.one)
        {
            Debug.LogWarning("PlayerCarry object has non-1 scale. This can cause pickup size issues.");
        }
    }

    public bool TryPickup(Rigidbody rb)
    {
        if (IsHolding || !rb || !holdPoint) return false;

        held       = rb;
        prevParent = held.transform.parent;
        prevLayer  = held.gameObject.layer;

        // Disable physics while held
        held.isKinematic      = true;
        held.useGravity       = false;
        held.detectCollisions = false;

        // Ignore raycast while held
        held.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // Parent to hold point, but DO NOT touch scale
        held.transform.SetParent(holdPoint, worldPositionStays: false);
        held.transform.localPosition = Vector3.zero;
        held.transform.localRotation = Quaternion.identity;

        return true;
    }

    public void Drop()
    {
        if (!held) return;

        // Unparent back to its original parent while keeping world transform
        held.transform.SetParent(prevParent, worldPositionStays: true);

        // Restore physics and layer
        held.isKinematic      = false;
        held.useGravity       = true;
        held.detectCollisions = true;
        held.gameObject.layer = prevLayer;

        held.linearVelocity   = Vector3.zero;
        held.angularVelocity  = Vector3.zero;
        held.AddForce(transform.forward * 2f, ForceMode.Impulse);

        held      = null;
        prevParent = null;
    }

    // Used by PlacePoint to say "you no longer hold this object"
    public void ForceReleaseWithoutDrop()
    {
        if (!held) return;

        // Just restore layer. PlacePoint already handled transform & physics
        held.gameObject.layer = prevLayer;

        held       = null;
        prevParent = null;
    }

    public GameObject DropTo(Vector3 worldPos, Quaternion worldRot)
    {
        if (!held) return null;
        var go = held.gameObject;

        held.transform.SetParent(prevParent, worldPositionStays: true);
        held.transform.SetPositionAndRotation(worldPos, worldRot);

        held.isKinematic      = false;
        held.useGravity       = true;
        held.detectCollisions = true;
        held.gameObject.layer = prevLayer;

        held = null;
        prevParent = null;
        return go;
    }

    // Toggle method for single-button pickup/drop
    public void TogglePickupDrop(Rigidbody targetRb = null)
    {
        if (IsHolding)
        {
            Drop();
        }
        else if (targetRb != null)
        {
            TryPickup(targetRb);
        }
    }
}
