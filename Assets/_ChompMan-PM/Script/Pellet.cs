using UnityEngine;

public class Pellet : MonoBehaviour
{
    // This function is called when another collider enters the trigger collider attached to the pellet
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to Pac-Man (or the specific AI controlling Pac-Man)
        if (other.CompareTag("PacMan"))
        {
            // Destroy the pellet, making it disappear
            Destroy(gameObject);
        }
    }
}
