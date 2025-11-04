using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement2 : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float sprintSpeed = 9f;

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

    private Vector3 planarMoveDir;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void Update()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * rayStartOffset;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundDistanceCheck, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private void HandleMovement()
    {
        // Player-relative axes (NOT camera-relative anymore)
        Vector3 f = transform.forward; f.y = 0f; f.Normalize();
        Vector3 r = transform.right;   r.y = 0f; r.Normalize();

        Vector3 desiredPlanar = f * moveInput.y + r * moveInput.x;
        planarMoveDir = desiredPlanar.sqrMagnitude > 1e-4f ? desiredPlanar.normalized : Vector3.zero;

        float targetSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 targetVelH = planarMoveDir * targetSpeed;

        // Use Rigidbody.velocity (not linearVelocity)
        Vector3 v = rb.linearVelocity;
        Vector3 vH = Vector3.Lerp(new Vector3(v.x, 0f, v.z), targetVelH, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(vH.x, v.y, vH.z);

        // Optional: rotate player to face move direction
        if (planarMoveDir.sqrMagnitude > 1e-4f)
        {
            Quaternion look = Quaternion.LookRotation(planarMoveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.fixedDeltaTime);
        }
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

    #region Input System Callbacks

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
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

    #endregion
}
