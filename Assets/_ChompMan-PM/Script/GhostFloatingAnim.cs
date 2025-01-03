using UnityEngine;

public class GhostFloatingAnim : MonoBehaviour
{
    public float floatSpeed = 1f;  // Speed of floating
    public float floatHeight = 0.5f;  // Height of floating
    
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
 