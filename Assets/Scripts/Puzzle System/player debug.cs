using UnityEngine;

public class PlayerPushDetector : MonoBehaviour
{
    [Header("Push Detection")]
    [SerializeField] private float checkInterval = 0.2f;
    [SerializeField] private float pushThreshold = 0.1f; // Minimum movement to count as "pushed"
    
    private Vector3 lastPosition;
    private float lastCheckTime;
    private Rigidbody playerRigidbody;
    private CharacterController playerController;
    
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerController = GetComponent<CharacterController>();
        lastPosition = transform.position;
        lastCheckTime = Time.time;
        
        Debug.Log("Player Push Detector started - will log what's pushing the player");
    }
    
    void Update()
    {
        if (Time.time - lastCheckTime >= checkInterval)
        {
            CheckForPushingObjects();
            lastCheckTime = Time.time;
        }
    }
    
    void CheckForPushingObjects()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;
        
        // Check if player is being moved by something other than their own input
        if (movement.magnitude > pushThreshold)
        {
            // Check what's colliding with the player
            CheckCollisions();
            
            Debug.Log($"Player is being moved! Movement: {movement.magnitude:F3}, Direction: {movement.normalized}");
        }
        
        lastPosition = currentPosition;
    }
    
    void CheckCollisions()
    {
        // Check for overlapping colliders
        Collider[] overlaps = Physics.OverlapSphere(transform.position, 2f); // 2 meter radius
        
        foreach (Collider collider in overlaps)
        {
            if (collider.gameObject != gameObject) // Don't detect self
            {
                // Check if this is a frame
                InteractableFrame frame = collider.GetComponent<InteractableFrame>();
                if (frame != null)
                {
                    Debug.Log($"🚨 PLAYER BEING PUSHED BY FRAME: {frame.name} at position {frame.transform.position}");
                    continue;
                }
                
                // Check if this is a frame position marker
                if (collider.name.Contains("Position") || collider.name.Contains("Frame"))
                {
                    Debug.Log($"🚨 Player near frame-related object: {collider.name} at {collider.transform.position}");
                    continue;
                }
                
                // Log any other close objects
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < 1.5f) // Only log very close objects
                {
                    Debug.Log($"Player near: {collider.name} (Distance: {distance:F2})");
                }
            }
        }
    }
    
    // Also check OnCollision events for more precise detection
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"🔴 COLLISION START with: {collision.gameObject.name}");
        LogCollisionDetails(collision);
    }
    
    void OnCollisionStay(Collision collision)
    {
        // Only log every second to avoid spam
        if (Time.time % 1f < 0.1f)
        {
            Debug.Log($"🟡 COLLISION CONTINUES with: {collision.gameObject.name}");
            LogCollisionDetails(collision);
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        Debug.Log($"🟢 COLLISION END with: {collision.gameObject.name}");
    }
    
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"🎮 CHARACTER CONTROLLER HIT: {hit.gameObject.name} with force");
        Debug.Log($"    Hit point: {hit.point}, Normal: {hit.normal}");
    }
    
    void LogCollisionDetails(Collision collision)
    {
        Debug.Log($"    Relative velocity: {collision.relativeVelocity.magnitude:F2}");
        Debug.Log($"    Contact points: {collision.contactCount}");
        
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.Log($"    Contact with {contact.otherCollider.name} at {contact.point}");
        }
        
        // Check if it's a frame
        InteractableFrame frame = collision.gameObject.GetComponent<InteractableFrame>();
        if (frame != null)
        {
            Debug.Log($"🚨🚨🚨 CONFIRMED: Frame {frame.name} is colliding with player!");
            Debug.Log($"    Frame position: {frame.transform.position}");
            Debug.Log($"    Frame scale: {frame.transform.lossyScale}");
            
            // Check collider size
            Collider collider = frame.GetComponent<Collider>();
            if (collider is BoxCollider box)
            {
                Debug.Log($"    Collider size: {box.size}");
                Debug.Log($"    Collider center: {box.center}");
            }
        }
    }
    
    // Visualize detection sphere in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}