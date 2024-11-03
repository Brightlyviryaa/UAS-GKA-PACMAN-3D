using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The target (ghost) for the camera to follow
    public Vector3 offset; // Offset from the target's position
    public float smoothSpeed = 0.125f; // Speed of the camera's movement

    private Quaternion staticRotation; // Store the initial rotation

    void Start()
    {
        // Store the initial rotation of the camera
        staticRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Calculate the desired position with the offset
            Vector3 desiredPosition = target.position + offset;
            // Smoothly interpolate to the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            // Update camera position
            transform.position = smoothedPosition;

            // Apply the static rotation to the camera
            transform.rotation = staticRotation;
        }
    }
}
