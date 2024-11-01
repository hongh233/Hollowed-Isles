using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;            // The Enemy reference to the player
    public float detectionRange = 10f;  // Range within which the enemy detects the player
    public float moveSpeed = 2f;        // Movement speed of the enemy
    private CharacterController characterController; // Reference to the character controller component
    private Animation enemyAnimation;   // Reference to the animation component


    public float gravity = 9.8f;            // Gravity applied to the enemy, so that they will fall
    private float verticalVelocity = 0f;    // Vertical movement speed


    public string idleAnimation;    
    public string walkAnimation;    
    public string attackAnimation;


    // Configurable attack parameters
    public float attackPower;       // Attack damage
    public float attackFrequency;   // Attack rate (attacks per second)
    public float attackRange;       // Range within which the enemy can attack


    public Image healthBarForeground;   // Reference to the health bar UI image
    public TextMeshProUGUI healthText;  // Reference to the text displaying health
    public float maxHealth;             // The amount of the Maximum health
    private float currentHealth;        // The amount of the current health


    private bool isAttacking = false;   // whether the enemy is currently attacking
    private float nextAttackTime = 0f;  // Time when the next attack is allowed

    void Start()
    {
        // Initialize components and health
        characterController = GetComponent<CharacterController>();
        enemyAnimation = GetComponent<Animation>();

        // Set attack animation to loop
        enemyAnimation[attackAnimation].wrapMode = WrapMode.Loop;

        // Initialize enemy health to be full
        currentHealth = maxHealth;
        UpdateHealthBar();
    }


    void Update()
    {
        // Calculate the distance between the enemy and the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If the player is within detection range
        if (distanceToPlayer < detectionRange)
        {
            // If the player is within the attack range
            if (distanceToPlayer <= attackRange)
            {
                if (!isAttacking)
                {
                    isAttacking = true; // Start attacking
                    PlayAttackAnimation();
                }

                // Control attack frequency
                if (Time.time >= nextAttackTime)
                {
                    AttackPlayer(); // Perform attack action
                    nextAttackTime = Time.time + 1f / attackFrequency; // Set next allowed attack time
                }
            }
            else
            {
                // If the player is in detection range but not in attack range
                if (isAttacking)
                {
                    isAttacking = false; // Stop attacking
                    enemyAnimation.Stop(); // Stop attack animation
                }

                // Always play walk animation to ensure smooth movement
                PlayWalkAnimation();

                // Move towards the player if the character controller is enabled
                if (characterController.enabled) 
                {
                    MoveTowardsPlayer();
                    RotateTowardsPlayer();
                }
            }
        }
        else
        {
            // Play idle animation if the player is out of detection range
            PlayIdleAnimation();
        }

        ApplyGravity(); // Apply gravity to the enemy, so that enemy will fall when they are in the sky

        // If the enemy falls below a certain height, trigger death
        if (transform.position.y < -5f)
        {
            Die();
        }
    }


    private void UpdateHealthBar()
    {
        // Update the fill amount of the health bar based on current health
        healthBarForeground.fillAmount = currentHealth / maxHealth;
        // Update the health text display
        healthText.text = $"{currentHealth}/{maxHealth}";
    }


    public void TakeDamage(float damage)
    {
        // Decrease current health and clamp it between 0 and maxHealth
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the health bar after taking damage
        UpdateHealthBar();

        // Check if the enemy's health has reached zero
        if (currentHealth <= 0)
        {
            Die();  // Trigger death
        }
    }
    private void Die()
    {
        // Stop all animations
        enemyAnimation.Stop();

        // Disable the character controller to prevent movement
        characterController.enabled = false;

        // Destroy the enemy game object
        Destroy(gameObject);
    }


    void MoveTowardsPlayer()
    {
        // Calculate the direction towards the player
        Vector3 direction = (player.position - transform.position).normalized;
        // Move horizontally
        Vector3 move = direction * moveSpeed * Time.deltaTime;
         // Apply vertical velocity (gravity)
        move.y = verticalVelocity * Time.deltaTime;
        // Use the character controller to move the enemy
        characterController.Move(move);
    }

    void RotateTowardsPlayer()
    {
        // Calculate the direction towards the player
        Vector3 direction = (player.position - transform.position).normalized;

        // Ignore vertical direction when rotating (keep rotation on the horizontal plane)
        direction.y = 0;

        // Calculate the target rotation to face the player
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate the enemy towards the player
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
    }


    void ApplyGravity()
    {
        // Check if the enemy is on the ground
        if (characterController.isGrounded)
        {
            // Reset vertical velocity when on the ground
            verticalVelocity = -1f;
        }
        else
        {
            // Apply gravity if the enemy is not grounded
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    void PlayWalkAnimation()
    {
        // Play walking animation if it's not already playing
        if (!enemyAnimation.IsPlaying(walkAnimation))
        {
            enemyAnimation.Play(walkAnimation);
        }
    }

    void PlayIdleAnimation()
    {
        // Play idle animation if it's not already playing
        if (!enemyAnimation.IsPlaying(idleAnimation))
        {
            enemyAnimation.Play(idleAnimation);
        }
    }

    void PlayAttackAnimation()
    {
        // Play attack animation if it's not already playing
        if (!enemyAnimation.IsPlaying(attackAnimation))
        {
            enemyAnimation.Play(attackAnimation);
        }
    }

    void AttackPlayer()
    {
        // Call the player's TakeDamage method to deal damage
        player.GetComponent<PlayerCharacter>().TakeDamage(attackPower);
    }


}
