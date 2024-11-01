using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{

    // Player movement and control components
    private CharacterController characterController;    // the CharacterController component
    private Animator animator;                          // the Animator component
    private float runSpeed = 5f;                        // Running speed of the player
    private float gravity = 9.8f;                       // Gravity constant
    private float verticalVelocity;                     // Vertical speed for jumping and falling
    private Vector3 moveDirection;                      // Direction in which the player is moving

    // Jumping-related variables
    private float jumpForce = 7f;           // Upward force applied when jumping
    private bool isJumping = false;         // Indicates whether the player is currently jumping
    private float jumpDuration = 0.7f;      // Duration of the jump
    private float jumpTimer = 0f;           // Timer to keep track of the jump duration

    // Rotation-related variables
    private float rotationX = 0f;           // Rotation angle on the X-axis for camera control

    // Camera and mouse sensitivity
    public Transform cameraTransform;       // Reference to the player's camera transform
    public float mouseSensitivity;          // Sensitivity for mouse movement

    // Attack-related variables
    private bool isAttacking = false;       // Indicates whether the player is currently attacking
    private float attackDuration = 0.5f;    // Duration of the attack animation
    private float attackTimer = 0f;         // Timer to keep track of the attack duration
    public float attackDamage;              // Damage dealt by the player's attack
    public float attackLength;              // Range of the attack

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();   
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;   // Lock the cursor to the center of the screen
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();       // Handle the player's rotation based on mouse input
        HandleMovement();       // Handle the player's movement based on keyboard input
        HandleJump();           // Handle the player's jumping behavior
        ApplyGravity();         // Apply gravity to the player

        HandleAttack();         // Handle the player's attack input
        UpdateAttackTimer();    // Update the attack timer to control attack duration
    }

    // Handles the player's rotation based on mouse movement
    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; // Get horizontal mouse input
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; // Get vertical mouse input
        
        // Rotate the player horizontally when the mouse is moved
        if (mouseX != 0)
        {
            transform.Rotate(Vector3.up * mouseX);
        }

        // Adjust the vertical rotation and clamp it to a range to prevent over-rotation
        rotationX -= mouseY;  
        // Limit vertical rotation between -90 and 90 degrees
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
    
        // Apply the vertical rotation to the camera
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    // Handles the player's movement based on keyboard input
    void HandleMovement()
    {
        if (!characterController.enabled) return; // If the CharacterController is disabled, do not move
        
        float horizontalInput = Input.GetAxis("Horizontal");// Get horizontal input (A/D or left/right arrow)
        float verticalInput = Input.GetAxis("Vertical");    // Get vertical input (W/S or up/down arrow)

        // Calculate forward and right directions based on the camera's orientation
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f; // Keep the forward direction horizontal
        right.y = 0f;   // Keep the right direction horizontal
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction based on input
        Vector3 direction = forward * verticalInput + right * horizontalInput;

        if (direction != Vector3.zero)
        {
            // Move the player in the specified direction
            MoveCharacter(direction, horizontalInput, verticalInput);
        }
        else
        {
            // Stop movement if there is no input
            SetAnimationState(false, false, false, false);
            moveDirection = Vector3.zero;
        }

        ApplyGravity();
        // Move the player character
        characterController.Move(moveDirection * Time.deltaTime);
    }

    // Moves the player character in the specified direction
    private void MoveCharacter(Vector3 direction, float horizontalInput, float verticalInput)
    {
        // Set animation state based on the movement direction
        if (verticalInput > 0) // Moving forward
        {
            SetAnimationState(true, false, false, false);
        }
        else if (verticalInput < 0) // Moving backward
        {
            SetAnimationState(false, true, false, false);
        }
        else if (horizontalInput > 0) // Moving right
        {
            SetAnimationState(false, false, false, true);
        }
        else if (horizontalInput < 0) // Moving left
        {
            SetAnimationState(false, false, true, false);
        }

        moveDirection = direction * runSpeed;   // Update the movement direction
    }

    // Handles the jumping behavior
    private void HandleJump()
    {
        // If the jump button (space) is pressed and the player is not already jumping
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            verticalVelocity = jumpForce;   // Apply upward force
            isJumping = true;               // Set jumping state
            jumpTimer = 0f;                 // Reset jump timer
            animator.SetTrigger("isJump");  // Trigger jump animation
        }

        // Update the jump timer if the player is jumping
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer >= jumpDuration)          // End the jump after the duration
            {
                isJumping = false;
                animator.ResetTrigger("isJump");    // Reset jump animation trigger
            }
        }
    }


    // Applies gravity to the player
    private void ApplyGravity()
    {
        if (isJumping)
        {
            // Apply upward force if jumping
            moveDirection.y = verticalVelocity;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;// Apply gravity when not jumping
            moveDirection.y = verticalVelocity;// Update vertical movement direction
        }
    }

    // Sets the player's animation state
    void SetAnimationState(bool runFWD, bool runBWD, bool runLFT, bool runRGT)
    {
        animator.SetBool("isRunFWD", runFWD); // Forward running animation
        animator.SetBool("isRunBWD", runBWD); // Backward running animation
        animator.SetBool("isRunLFT", runLFT); // Left running animation
        animator.SetBool("isRunRGT", runRGT); // Right running animation
    }

    // Handles the player's attack behavior
    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking) // Left mouse button pressed and not currently attacking
        {
            animator.SetTrigger("isAttack"); // Trigger attack animation
            isAttacking = true;
            attackTimer = 0f; // Reset attack timer

            // Perform a spherical overlap to detect hits in the attack range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 1f, attackLength);

            foreach (var hitCollider in hitColliders)
            {
                Debug.Log("Hit detected: " + hitCollider.transform.name);

                if (hitCollider.CompareTag("Enemy"))
                {
                    // Inflict damage to the enemy
                    hitCollider.GetComponent<EnemyAI>().TakeDamage(attackDamage);
                }
            }
        }
    }
    
    // Updates the attack timer
    private void UpdateAttackTimer()
    {
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;          // Increment the attack timer
            if (attackTimer >= attackDuration)      // Stop attacking after the duration
            {
                animator.ResetTrigger("isAttack");  // Reset the attack animation trigger
                isAttacking = false;                // End the attack state
            }
        }
    }



    // Triggers the hit animation
    public void HandleHit()
    {
        animator.SetTrigger("isHit"); // Trigger hit animation
    }

    // Triggers the death animation
    public void HandleDeath()
    {
        animator.SetTrigger("isDead"); // Trigger death animation
    }

}
