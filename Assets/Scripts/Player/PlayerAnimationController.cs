using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement2 playerMovement;
    [SerializeField] private Animator animator;

    // Animator parameter hashes
    private int speedHash;
    private int isGroundedHash;
    private int isCrouchingHash;
    private int isJumpingHash;
    private int isLookingHash;

    // Cached previous state
    private bool wasGrounded;

    private void Awake()
    {
        // Get references if not assigned in inspector
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement2>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (!animator)
                animator = GetComponentInChildren<Animator>();
        }

        // Initialize parameter hashes (MUST match Animator parameter names)
        speedHash       = Animator.StringToHash("Speed");
        isGroundedHash  = Animator.StringToHash("IsGrounded");
        isCrouchingHash = Animator.StringToHash("IsCrouching");
        isJumpingHash   = Animator.StringToHash("IsJumping");
        isLookingHash   = Animator.StringToHash("IsLooking");
    }

    private void Update()
    {
        if (playerMovement == null || animator == null) return;

        UpdateLocomotion();
        UpdateGroundedAndJump();
        UpdateCrouch();
    }

    private void UpdateLocomotion()
    {
        // Use horizontal velocity magnitude as Speed
        Vector3 v = playerMovement.Velocity;
        float horizontalSpeed = new Vector3(v.x, 0f, v.z).magnitude;

        animator.SetFloat(speedHash, horizontalSpeed);
    }

    private void UpdateGroundedAndJump()
    {
        bool isGrounded = playerMovement.IsGrounded;
        animator.SetBool(isGroundedHash, isGrounded);

        // Detect jump: was grounded, now not grounded, and moving upward
        if (wasGrounded && !isGrounded && playerMovement.Velocity.y > 0.1f)
        {
            animator.SetTrigger(isJumpingHash);
        }

        wasGrounded = isGrounded;
    }

    private void UpdateCrouch()
    {
        bool isCrouching = playerMovement.IsCrouching;
        animator.SetBool(isCrouchingHash, isCrouching);
    }

    // Call this from an input action or other script to play look-around animation
    public void TriggerLookAround()
    {
        if (!animator) return;
        animator.SetTrigger(isLookingHash);
    }
}
