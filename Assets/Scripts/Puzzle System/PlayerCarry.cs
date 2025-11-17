using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [Header("Where the item sits in front of the camera")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Vector3 defaultLocalOffset = new Vector3(0f, -0.15f, 0.7f);

    private Rigidbody held;
    private Transform prevParent;
    private int prevLayer;
    private Vector3 prevWorldScale;

    public bool IsHolding => held != null;
    public Rigidbody HeldBody => held;

    void Awake()
    {
        // WHY: Ensure we always have a holdPoint even if not assigned in inspector
        if (!holdPoint)
        {
            var cam = Camera.main ? Camera.main.transform : null;
            if (cam)
            {
                var hp = new GameObject("HoldPoint").transform;
                hp.SetParent(cam, false);
                hp.localPosition = defaultLocalOffset;
                hp.localRotation = Quaternion.identity;
                // hp.localScale    = Vector3.one;           // IMPORTANT: Prevent scale inheritance issues
                holdPoint = hp;
            }
            else
            {
                Debug.LogWarning("PlayerCarry: No holdPoint and no Camera.main. Assign a HoldPoint in the inspector!");
            }
        }
    }

    public bool TryPickup(Rigidbody rb)
    {
        // WHY: Check if we can pick up (not already holding, valid rigidbody, etc.)
        if (IsHolding || !rb || !holdPoint) return false;

        held       = rb;
        prevParent = held.transform.parent;
        prevLayer  = held.gameObject.layer;
        prevWorldScale = held.transform.lossyScale;  // remember world scale

        // WHY: Disable physics while holding to prevent collisions and gravity
        held.isKinematic      = true;
        held.useGravity       = false;
        held.detectCollisions = false;

        // WHY: Prevent the interaction ray from hitting the held object
        held.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // WHY: Parent to hold point and reset local transform
        held.transform.SetParent(holdPoint, false);
        held.transform.localPosition = Vector3.zero;
        held.transform.localRotation = Quaternion.identity;
        
        Debug.Log("Picked up " + rb.name);
        return true;
    }

    public void Drop()
    {
        if (!held) return;

        // WHY: Restore original parent while keeping world position
        held.transform.SetParent(prevParent, worldPositionStays: true);

        // WHY: Reset scale to avoid parent scale inheritance issues
        held.transform.localScale = Vector3.one;

        // WHY: Restore physics properties
        held.isKinematic      = false;
        held.useGravity       = true;
        held.detectCollisions = true;
        held.gameObject.layer = prevLayer;

        // WHY: Apply a small forward force when dropping for more natural feel
        held.linearVelocity = Vector3.zero;
        held.angularVelocity = Vector3.zero;
        held.AddForce(transform.forward * 2f, ForceMode.Impulse);

        held = null;
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

    // NEW: Toggle method for single-button pickup/drop
    public void TogglePickupDrop(Rigidbody targetRb = null)
    {
        if (IsHolding)
        {
            // If holding something, drop it
            Drop();
        }
        else if (targetRb != null)
        {
            // If not holding and target is provided, try to pick it up
            TryPickup(targetRb);
        }
    }
}