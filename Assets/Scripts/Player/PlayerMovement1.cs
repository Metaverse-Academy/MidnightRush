using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform; // assign your Camera (make it a child of the player)

    [Header("Move")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float acceleration = 12f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDistanceCheck = 0.3f;
    [SerializeField] private float rayStartOffset = 0.06f;

    [Header("Crouch")]
    [SerializeField] private bool useToggleCrouch = true;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivityX = 0.12f; // increase if too slow
    [SerializeField] private float mouseSensitivityY = 0.12f; // increase if too slow
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    // inputs/state
    private Vector2 moveInput;   // WASD / stick
    private Vector2 lookInput;   // mouse delta / stick
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;

    // look state
    private float yaw;   // player body rotation around Y
    private float pitch; // camera pitch around X

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // We rotate player around Y ourselves; lock X/Z so physics don’t tip the body over
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Make sure the camera is parented to the player (so it follows position)
        if (cameraTransform != null && cameraTransform.parent != transform)
            cameraTransform.SetParent(transform);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize yaw from current player rotation so there’s no snap
        yaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        // Ground check
        Vector3 rayOrigin = transform.position + Vector3.up * rayStartOffset;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundDistanceCheck, groundLayer, QueryTriggerInteraction.Ignore);

        // Handle look here (use Update for input feel; no physics required)
        HandleLook();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleLook()
    {
        if (!cameraTransform) return;

        // Mouse deltas are per-frame from the Input System; no need to multiply by deltaTime
        yaw   += lookInput.x * mouseSensitivityX;
        pitch -= lookInput.y * mouseSensitivityY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply rotations
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);         // turn the player body (yaw)
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f); // tilt the camera (pitch)
    }

    private void HandleMovement()
    {
        // Movement relative to player yaw (transform.forward/right)
        Vector3 f = transform.forward; f.y = 0f; f.Normalize();
        Vector3 r = transform.right;   r.y = 0f; r.Normalize();

        Vector3 desiredPlanar = f * moveInput.y + r * moveInput.x;
        Vector3 planarMoveDir = desiredPlanar.sqrMagnitude > 1e-4f ? desiredPlanar.normalized : Vector3.zero;

        float targetSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 targetVelH = planarMoveDir * targetSpeed;

        // Smooth horizontal linearVelocity, preserve vertical
        Vector3 v = rb.linearVelocity;
        Vector3 vH = Vector3.Lerp(new Vector3(v.x, 0f, v.z), targetVelH, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(vH.x, v.y, vH.z);

        // IMPORTANT: Do NOT auto-rotate to movement direction (that causes “circling” with mouse look).
        // (We removed that block from your old script.)
    }

    private void ApplyCrouchState()
    {
        if (capsule)
            capsule.height = isCrouching ? crouchHeight : standingHeight;

        if (isCrouching)
            isSprinting = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 rayOrigin = transform.position + Vector3.up * rayStartOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundDistanceCheck);
    }

    // ===== Input System callbacks =====

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isGrounded)
        {
            Vector3 cur = rb.linearVelocity;
            if (cur.y < 0f) cur.y = 0f;
            rb.linearVelocity = cur;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) isSprinting = true;
        else if (ctx.canceled) isSprinting = false;
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (useToggleCrouch)
        {
            if (ctx.performed)
            {
                isCrouching = !isCrouching;
                ApplyCrouchState();
            }
        }
        else
        {
            if (ctx.performed)
            {
                isCrouching = true;
                ApplyCrouchState();
            }
            else if (ctx.canceled)
            {
                isCrouching = false;
                ApplyCrouchState();
            }
        }
    }
}
