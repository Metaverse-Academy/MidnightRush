using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CameraRotate2 : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Smoothly rotate toward the target yaw/direction.")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float turnSpeed = 10f;
    [Tooltip("If your mesh doesn't face +Z, add an offset here.")]
    [SerializeField] private float yawOffset = 0f;

    [Header("Choose a yaw source (pick one)")]
    [SerializeField] private bool useMouseLook = true;
    [SerializeField] private bool faceVelocity = false;

    [Header("Mouse look")]
    [SerializeField] private float mouseSensitivity = 0.1f; // scale for Input System delta

    private Rigidbody rb;
    private float yaw;               // accumulated yaw for mouse-look
    private Vector2 lookDelta;       // latest look delta from input

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // cursor lock is optional; keep if you want mouse-look
        if (useMouseLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // accumulate yaw from mouse/look input if using mouse look
        if (useMouseLook && !faceVelocity)
        {
            // lookDelta is set in OnLook()
            yaw += lookDelta.x * mouseSensitivity;
            // reset delta so we only apply once per frame
            lookDelta = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        Quaternion targetRot;

        if (faceVelocity && rb)
        {
            // face horizontal velocity direction if moving
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            if (v.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = v.normalized;
                targetRot = Quaternion.LookRotation(Quaternion.Euler(0f, yawOffset, 0f) * dir, Vector3.up);
                ApplyRotation(targetRot);
                return;
            }
            // if not moving, fall through to keep current rotation (or mouse yaw if enabled)
        }

        // default: use accumulated mouse yaw (no camera needed)
        targetRot = Quaternion.Euler(0f, yaw + yawOffset, 0f);
        ApplyRotation(targetRot);
    }

    private void ApplyRotation(Quaternion targetRot)
    {
        if (smoothFollow)
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        else
            rb.MoveRotation(targetRot);
    }

    // ---------- Input System ----------
    // Bind this to your "Look" action (Vector2, usually Mouse delta / Right stick).
    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (!useMouseLook) return;
        // read once per frame in Update; store raw delta here
        if (ctx.performed || ctx.canceled)
            lookDelta = ctx.ReadValue<Vector2>();
    }
}
