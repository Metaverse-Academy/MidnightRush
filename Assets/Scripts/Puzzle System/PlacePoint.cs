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

    // PUBLIC PROPERTY FOR EXTERNAL ACCESS
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
            // place
            if (IsOccupied) return;
            var rb = carry.HeldBody;
            var go = rb ? rb.gameObject : null;
            if (!go) return;
            if (!IsAllowed(go)) return;

            // FIXED: Use custom placement that doesn't mess with parenting
            PlaceObject(go);
            current = go;
            OnPlaced?.Invoke(this, go);
        }
        else
        {
            // take back
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
        
        // Disable physics but keep object visible
        if (rb) 
        { 
            rb.isKinematic = true; 
            rb.useGravity = false;
            rb.detectCollisions = false;
        }
        
        // FIXED: Set position/rotation directly without complex parenting
        go.transform.position = snapPoint.position;
        go.transform.rotation = snapPoint.rotation;
        
        // Optional: Parent to snap point but maintain scale
        // go.transform.SetParent(snapPoint, true);
    }

    private bool IsAllowed(GameObject go)
    {
        if (allowedObjects == null || allowedObjects.Length == 0) return true;
        foreach (var a in allowedObjects) if (a == go) return true;
        return false;
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (snapPoint)
        {
            Gizmos.color = IsOccupied ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(snapPoint.position, Vector3.one * 0.1f);
        }
    }
}