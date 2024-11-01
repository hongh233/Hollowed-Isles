using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class PlayerCharacter : MonoBehaviour
{
    public Image healthBarForeground;   // The foreground image of the health bar UI
    public TextMeshProUGUI healthText;  // The text component to display current health


    private float maxHealth = 200f; // The maximum health of the player
    private float currentHealth;    // The maximum health of the player
    private bool isDead = false;    // show that if the player is dead



    public float fallDamageStartHeight = 10f; // Minimum height for fall damage to start
    public float damagePerMeter = 5f;         // Damage taken per meter of falling
    private float fallStartHeight;            // The height where the fall started
    private bool isFalling = false;           // Indicates if the player is currently falling


    private CharacterController characterController;


    public Transform victoryArea;               // The location of the victory area
    public float victoryAreaRadius = 5f;        // Radius around the victory area to trigger victory
    private bool allEnemiesDefeated = false;    // Indicates if all enemies have been defeated
    private bool victoryAchieved = false;       // Indicates if victory conditions are met


    private int totalEnemies;           // Total number of enemies at the start
    private int defeatedEnemies = 0;    // Number of enemies defeated

    void Start()
    {
        currentHealth = maxHealth;      // Set current health to maximum (full health)
        UpdateHealthBar();              // Update the health bar UI
        characterController = GetComponent<CharacterController>();
        // Count the total number of enemies in the scene
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    //method to update the health bar UI
    private void UpdateHealthBar()
    {
        healthBarForeground.fillAmount = currentHealth / maxHealth; // Update health bar fill amount
        healthText.text = $"{currentHealth}/{maxHealth}";           // Update health text
    }

    // method to reduce health by a specified amount
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;                                    // Subtract damage from current health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);   // Clamp health to be between 0 and maxHealth
        UpdateHealthBar();                                          // Update the health bar UI

        // Check if the player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Handle player death
    private void Die()
    {
        // Change the color of all lights to red to indicate death
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            light.color = Color.red;
        }

        isDead = true;                          // Set the dead flag
        characterController.enabled = false;    // Disable character movement
        Time.timeScale = 0f;                    // Pause the game
    }

    // Handle on-screen GUI elements
    void OnGUI()
    {
        if (isDead)
        {
            // Set the style for the "You Died" message
            GUIStyle guiStyle = new GUIStyle();
            guiStyle.fontSize = 35;
            guiStyle.normal.textColor = Color.red;
            guiStyle.alignment = TextAnchor.MiddleCenter;

            // Display the "You Died" message
            Rect rect = new Rect(Screen.width / 2, Screen.height / 2, 0, 0);
            GUI.Label(rect, "You Died. Press R to Restart", guiStyle);

            // Check if the player presses the 'R' key to restart
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R)
            {
                Time.timeScale = 1f;    // Resume time
                RestartGame();          // Restart the game
            }
        }

        if (victoryAchieved)
        {
            // Set the style for the victory message
            GUIStyle guiStyle = new GUIStyle();
            guiStyle.fontSize = 35;
            guiStyle.normal.textColor = Color.yellow;
            guiStyle.alignment = TextAnchor.MiddleCenter;

            // Display the victory message
            Rect rect = new Rect(Screen.width / 2, Screen.height / 2, 0, 0);

            // Check if the player presses the 'N' key to go to the next level
            GUI.Label(rect, "Mission Accomplished. Press N to next level", guiStyle);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.N)
            {
                SceneManager.LoadScene("Level2_Scene"); // Load the next level
            }
        }

        DisplayTaskInfo();      // Show the task information
    }


    // Display task information on the screen
    private void DisplayTaskInfo()
    {
        // Style for the task title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 16;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        // Style for the task details
        GUIStyle taskStyle = new GUIStyle();
        taskStyle.fontSize = 14;
        taskStyle.normal.textColor = Color.white;
        taskStyle.alignment = TextAnchor.MiddleLeft;

        // Display the task title
        Rect titleRect = new Rect(Screen.width / 2 - 150, 10, 300, 30);
        GUI.Label(titleRect, "Task to success:", titleStyle);

        // Display task details
        float tasksStartX = Screen.width / 2 - 150;
        Rect taskRect1 = new Rect(tasksStartX, 30, 400, 30);
        GUI.Label(taskRect1, $"1. Defeat all enemies ({defeatedEnemies}/{totalEnemies})", taskStyle);

        Rect taskRect2 = new Rect(tasksStartX, 50, 400, 30);
        GUI.Label(taskRect2, "2. Find a new portal same as the starting portal.", taskStyle);
    }


    // Restart the game by reloading the current scene
    private void RestartGame()
    {
        Time.timeScale = 1f; // Resume game time

        // Reset player status
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthBar();
        isFalling = false;
        allEnemiesDefeated = false;
        victoryAchieved = false;
        defeatedEnemies = 0;

        // Reset player position and enable character movement
        characterController.enabled = false;
        transform.position = new Vector3(20.27f, 6f, -136.4f);
        characterController.enabled = true;

        // Reset player controls if available
        var playerControl = GetComponent<PlayerControl>();
        if (playerControl != null)
        {
            playerControl.enabled = false;
            playerControl.enabled = true;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
        Cursor.lockState = CursorLockMode.Locked;   // Lock the cursor
        Cursor.visible = false;                     // Hide the cursor
    }



    // Update is called once per frame
    void Update()
    {
        // Manually check if the player is grounded
        bool isGroundedManual = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // Check if the player starts falling
        if (!isGroundedManual && !isFalling)
        {
            isFalling = true;
            fallStartHeight = transform.position.y;     // Record the fall start height
        }

        // Check if the player lands
        if (isGroundedManual && isFalling)
        {
            float fallDistance = fallStartHeight - transform.position.y; // Calculate the fall distance

            if (fallDistance > fallDamageStartHeight)
            {
                // Calculate fall damage
                float fallDamage = (fallDistance - fallDamageStartHeight) * damagePerMeter;
                TakeDamage(fallDamage);     // Apply damage
            }

            isFalling = false; // Reset falling state
        }

        // Check if the player falls below a certain height
        if (transform.position.y < -100f)
        {
            TakeDamage(currentHealth);  // Take full damage and die
        }


        CheckVictoryConditions();       // Check for victory conditions
    }


    // Check if the victory conditions are met
    private void CheckVictoryConditions()
    {
        // Check if all enemies are defeated
        if (!allEnemiesDefeated)
        {
            int remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            defeatedEnemies = totalEnemies - remainingEnemies;

            if (remainingEnemies == 0)
            {
                allEnemiesDefeated = true; // Mark all enemies as defeated
                Debug.Log("All enemies defeated. Now find the victory area.");
            }
        }
        else
        {
            // Check if the player is within the victory area
            float distanceToVictoryArea = Vector3.Distance(transform.position, victoryArea.position);
            if (distanceToVictoryArea <= victoryAreaRadius)
            {
                Victory();  // Trigger victory
            }
        }
    }


    // Trigger victory and change the environment to indicate success
    private void Victory()
    {
       Debug.Log("Mission Accomplished! Press N to next level.");
        victoryAchieved = true; // Mark victory as achieved

        // Change the color of all lights to yellow to indicate victory
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            light.color = Color.yellow;
        }
    }


}
