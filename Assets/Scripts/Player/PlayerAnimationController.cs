using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement2 playerMovement;
    [SerializeField] private Animator animator;

    // Animation parameter IDs (for performance)
    private int isWalkingHash;
    private int isRunningHash;
    private int isJumpingHash;
    private int isCrouchingHash;
    private int isCrouchWalkingHash;
    private int isGroundedHash;

    // Cache previous states to avoid unnecessary Animator calls
    private bool wasGrounded;
    private bool wasMoving;
    private bool wasSprinting;
    private bool wasCrouching;

    private void Awake()
    {
        // Get references if not assigned in inspector
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement2>();
        
        if (animator == null)
            animator = GetComponent<Animator>();

        // Initialize parameter hashes
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        isCrouchingHash = Animator.StringToHash("isCrouching");
        isCrouchWalkingHash = Animator.StringToHash("isCrouchWalking");
        isGroundedHash = Animator.StringToHash("isGrounded");
    }

    private void Update()
    {
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (playerMovement == null || animator == null) return;

        // Get current state from player movement
        bool isMoving = playerMovement.IsMoving;
        bool isGrounded = playerMovement.IsGrounded;
        bool isSprinting = playerMovement.IsSprinting;
        bool isCrouching = playerMovement.IsCrouching;
        bool isJumping = playerMovement.IsJumping;

        // Update animations only when state changes (optimization)
        if (isGrounded != wasGrounded)
        {
            animator.SetBool(isGroundedHash, isGrounded);
            wasGrounded = isGrounded;
        }

        if (isMoving != wasMoving || isSprinting != wasSprinting || isCrouching != wasCrouching)
        {
            // Walking & Running
            animator.SetBool(isWalkingHash, isMoving && !isSprinting && !isCrouching);
            animator.SetBool(isRunningHash, isMoving && isSprinting && !isCrouching);
            
            // Crouching
            animator.SetBool(isCrouchingHash, isCrouching);
            animator.SetBool(isCrouchWalkingHash, isMoving && isCrouching);

            wasMoving = isMoving;
            wasSprinting = isSprinting;
            wasCrouching = isCrouching;
        }

        // Jumping (usually triggered by events, but can be state-based)
        if (isJumping)
        {
            animator.SetBool(isJumpingHash, true);
        }
        else if (isGrounded && animator.GetBool(isJumpingHash))
        {
            animator.SetBool(isJumpingHash, false);
        }
    }

    // Animation event methods (called from animation clips)
    public void OnJumpAnimationEnd()
    {
        animator.SetBool(isJumpingHash, false);
    }

    public void OnLandAnimationComplete()
    {
        // Reset any landing states if needed
    }
}