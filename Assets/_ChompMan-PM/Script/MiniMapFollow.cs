using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player; // Drag the player object here in Inspector

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y; // Keep the camera height constant
            transform.position = newPosition;
        }
    }
}
