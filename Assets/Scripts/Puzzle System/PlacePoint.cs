using UnityEngine;
using System;

public class PlacePoint : Interactable
{
    [Tooltip("Optional: limit to specific objects (exact instances). Leave empty = allow any.")]
    [SerializeField] private GameObject[] allowedObjects;

    [Tooltip("Where to snap. If null, uses this transform.")]
    [SerializeField] private Transform snapPoint;

    [Tooltip("Allow taking back the placed object with Interact?")]
    [SerializeField] private bool allowTakeBack = true;

    public AudioClip placecubeSound;
    [SerializeField] private AudioSource audioSource;
    public bool AllowTakeBack 
    { 
        get => allowTakeBack; 
        set => allowTakeBack = value; 
    }

    public bool IsOccupied => current != null;
    public GameObject CurrentObject => current;

    public event Action<PlacePoint, GameObject> OnPlaced;
    public event Action<PlacePoint, GameObject> OnRemoved;

    private GameObject current;

    private void Awake()
    {
        if (!snapPoint) snapPoint = transform;

        if (snapPoint.lossyScale != Vector3.one)
        {
            Debug.LogWarning($"{name}: snapPoint or its parents have non-1 scale. This can cause size issues for placed objects.");
        }
    }

    public override string GetPrompt()
    {
        if (!IsOccupied) return "Place object";
        return allowTakeBack ? "Take object" : "Occupied";
    }

    public override void Interact(GameObject interactor)
    {
        var carry = interactor.GetComponent<PlayerCarry>();
        if (!carry) return;

        if (carry.IsHolding)
        {
            // PLACE
            if (IsOccupied) return;
            var rb = carry.HeldBody;
            var go = rb ? rb.gameObject : null;
            if (!go) return;
            if (!IsAllowed(go)) return;

            PlaceObject(go);

            current = go;

            // Tell the carry system it no longer holds this object
            carry.ForceReleaseWithoutDrop();

            OnPlaced?.Invoke(this, go);
        }
        else
        {
            // TAKE BACK
            if (!allowTakeBack || !IsOccupied) return;

            var rb = current.GetComponent<Rigidbody>();
            if (rb && carry.TryPickup(rb))
            {
                var old = current;
                current = null;
                OnRemoved?.Invoke(this, old);
            }
        }
    }

    private void PlaceObject(GameObject go)
{
    var rb = go.GetComponent<Rigidbody>();


    // 1) Save the current *world* scale (how big it actually looks)
    Vector3 desiredWorldScale = go.transform.lossyScale;

    // 2) Disable physics so it sits nicely in the slot
    if (rb)
    {
        rb.isKinematic      = true;
        rb.useGravity       = false;
        rb.detectCollisions = false;
    }

    // 3) Parent to the snapPoint and move/rotate it there
    go.transform.SetParent(snapPoint, worldPositionStays: false);
    go.transform.position = snapPoint.position;
    go.transform.rotation = snapPoint.rotation;

    // 4) Fix the scale so its *world* size stays the same as before
    Vector3 parentWorldScale = snapPoint.lossyScale;


    // Avoid division by zero just in case
    if (parentWorldScale.x == 0) parentWorldScale.x = 0.0001f;
    if (parentWorldScale.y == 0) parentWorldScale.y = 0.0001f;
    if (parentWorldScale.z == 0) parentWorldScale.z = 0.0001f;

    go.transform.localScale = new Vector3(
        desiredWorldScale.x / parentWorldScale.x,
        desiredWorldScale.y / parentWorldScale.y,
        desiredWorldScale.z / parentWorldScale.z
    );
        audioSource.PlayOneShot(placecubeSound);

    
    // Optional debug:
    // Debug.Log($"Placed {go.name}. World scale before: {desiredWorldScale}, after: {go.transform.lossyScale}");
}



    private bool IsAllowed(GameObject go)
    {
        if (allowedObjects == null || allowedObjects.Length == 0) return true;
        foreach (var a in allowedObjects) if (a == go) return true;
        return false;
    }

    private void OnDrawGizmos()
    {
        if (snapPoint)
        {
            Gizmos.color = IsOccupied ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(snapPoint.position, Vector3.one * 0.1f);
        }
    }
}