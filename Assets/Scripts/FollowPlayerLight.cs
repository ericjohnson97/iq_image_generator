using UnityEngine;

public class FollowPlayerLight : MonoBehaviour
{
    public Transform player; // Assign your player's transform in the inspector
    public float heightAbovePlayer = 1000f; // Height above the player where the light should stay

    void Update()
    {
        if (player != null)
        {
            // Update the light's position to be directly above the player at the specified height
            transform.position = player.position + Vector3.up * heightAbovePlayer;

            // Orient the light to face directly downwards.
            // Since a directional light in Unity doesn't use its position (only rotation), pointing it downwards simulates a sun-like effect.
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
