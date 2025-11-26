using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float acceleration = 12f;
    [Header("Slope Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;

    // START MODIFICATION: Stair climbing settings
    [Header("Stair Settings")]
    [SerializeField] private GameObject stepRayUpper;
    [SerializeField] private GameObject stepRayLower;
    [SerializeField] private float stepHeight = 0.3f;
    [SerializeField] private float stepSmooth = 0.1f;
    // END MODIFICATION

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDistanceCheck = 0.3f;
    [SerializeField] private float rayStartOffset = 0.06f;

    [Header("Crouch Settings")]
    [SerializeField] private bool useToggleCrouch = true;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivityX = 0.12f;
    [SerializeField] private float mouseSensitivityY = 0.12f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Audio")]
    [SerializeField] private AudioSource footstepsSource;
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip[] footstepClips; // random footsteps
    [SerializeField] private float footstepIntervalWalk = 0.5f;
    [SerializeField] private float footstepIntervalSprint = 0.35f;

    [SerializeField] private AudioClip jumpClip;

    private float footstepTimer;


    // Components
    private Rigidbody rb;
    private CapsuleCollider capsule;

    // Input
    private Vector2 moveInput;
    private Vector2 lookInput;

    private Animator anim;
    public bool IsGrounded { get; private set; }
    public bool IsMoving => moveInput.magnitude > 0.1f;
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsJumping { get; private set; }
    public Vector3 Velocity => rb.linearVelocity;

    // Look state
    private float yaw;
    private float pitch;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;

        if (cameraTransform != null && cameraTransform.parent != transform)
            cameraTransform.SetParent(transform);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;

        // START MODIFICATION: Initialize stair raycast positions
        if (stepRayUpper != null)
        {
            stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepHeight, stepRayUpper.transform.position.z);
        }
        // END MODIFICATION
    }

    private void Update()
    {
        if (GameManager.Instance.GamePaused()) return;
        if (GameManager.Instance.IsGameEnded()) return;
        if (GameManager.Instance.HasGameWon()) return;
        CheckGrounded();
        HandleLook();

        anim.SetBool("IsCrouching", IsCrouching);
        anim.SetBool("IsJumping", !IsGrounded);
        anim.SetBool("IsRunning", IsSprinting);

        if (moveInput.magnitude > 0.1f)
        {
            anim.SetBool("IsLooking", false);
        }
        else
        {
            anim.SetBool("IsLooking", true);
        }

        HandleBreathing();
        HandleFootsteps();

    }

    private void FixedUpdate()
    {
        // START MODIFICATION: Call HandleStairs before HandleMovement
        HandleStairs();
        // END MODIFICATION
        HandleMovement();
    }

    // START MODIFICATION: New function to handle stair climbing
    private void HandleStairs()
    {
        if (!IsMoving || !IsGrounded) return;

        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, moveDirection, out hitLower, 0.5f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, moveDirection, out hitUpper, 0.6f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.fixedDeltaTime, 0f);
            }
        }
    }
    // END MODIFICATION

    private void CheckGrounded()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * rayStartOffset;
        IsGrounded = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundDistanceCheck, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private void HandleLook()
    {
        if (!cameraTransform) return;

        yaw += lookInput.x * mouseSensitivityX;
        pitch -= lookInput.y * mouseSensitivityY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

    }

    private void HandleMovement()
    {
        Vector3 f = transform.forward; f.y = 0f; f.Normalize();
        Vector3 r = transform.right; r.y = 0f; r.Normalize();

        Vector3 desiredPlanar = f * moveInput.y + r * moveInput.x;
        Vector3 planarMoveDir = desiredPlanar.sqrMagnitude > 1e-4f ? desiredPlanar.normalized : Vector3.zero;

        float targetSpeed = IsCrouching ? crouchSpeed : (IsSprinting ? sprintSpeed : walkSpeed);

        Vector3 targetVelH;

        if (OnSlope())
        {
            targetVelH = Vector3.ProjectOnPlane(planarMoveDir, slopeHit.normal).normalized * targetSpeed;
        }
        else
        {
            targetVelH = planarMoveDir * targetSpeed;
        }
        Vector3 v = rb.linearVelocity;
        Vector3 vH = Vector3.Lerp(new Vector3(v.x, 0f, v.z), targetVelH, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(vH.x, v.y, vH.z);

        if (moveInput.magnitude > 0.1f)
        {
            anim.SetBool("IsWalking", true);
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }

    }

    private void ApplyCrouchState()
    {
        if (capsule)
            capsule.height = IsCrouching ? crouchHeight : standingHeight;

        if (IsCrouching)
            IsSprinting = false;
    }

    // INPUT METHODS
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }


    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameManager.Instance.TogglePauseGame();
        }
    }
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsGrounded)
        {
            Vector3 cur = rb.linearVelocity;
            if (cur.y < 0f) cur.y = 0f;
            rb.linearVelocity = cur;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (sfxSource != null && jumpClip != null)
        {
            sfxSource.PlayOneShot(jumpClip);
        }
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) IsSprinting = true;
        else if (ctx.canceled) IsSprinting = false;
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (useToggleCrouch)
        {
            if (ctx.performed)
            {
                IsCrouching = !IsCrouching;
                ApplyCrouchState();
            }
        }
        else
        {
            if (ctx.performed)
            {
                IsCrouching = true;
                ApplyCrouchState();

            }
            else if (ctx.canceled)
            {
                IsCrouching = false;
                ApplyCrouchState();
            }
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, capsule.height * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private void HandleBreathing()
    {
        if (breathingSource == null) return;

        if (!breathingSource.isPlaying)
        {
            breathingSource.loop = true;
            breathingSource.Play();
        }
    }

    private void HandleFootsteps()
    {
        if (footstepsSource == null || footstepClips == null || footstepClips.Length == 0)
            return;

        if (IsGrounded && IsMoving && !IsCrouching)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                PlayRandomFootstep();

                float interval = IsSprinting ? footstepIntervalSprint : footstepIntervalWalk;
                footstepTimer = interval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        AudioClip clip = footstepClips[index];

        footstepsSource.pitch = Random.Range(0.95f, 1.05f);
        footstepsSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 rayOrigin = transform.position + Vector3.up * rayStartOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundDistanceCheck);

        // START MODIFICATION: Gizmos for stair detection
        if (stepRayLower != null && stepRayUpper != null)
        {
            Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(stepRayLower.transform.position, moveDirection * 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(stepRayUpper.transform.position, moveDirection * 0.6f);
        }
        // END MODIFICATION
    }
}
