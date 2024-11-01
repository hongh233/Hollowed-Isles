using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    public Transform player; // player's Transform, used to follow the player
    public float cameraHeight = 110f; // The fixed height at which the camera should stay

    // Update is called once per frame
    void Update()
    {
        // Check if the player reference is not null
        if (player != null)
        {
            // Create a new position for the camera based on the player's position
            Vector3 newPosition = player.position;

            // Keep the Y-coordinate (height) of the camera fixed at cameraHeight
            newPosition.y = cameraHeight;

            // Update the camera's position to the new position, following the player's X and Z coordinates
            transform.position = newPosition;
        }
        
    }
}
