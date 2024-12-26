using UnityEngine;

public class GhostManager : MonoBehaviour
{
    public GameObject[] ghosts; // Array to hold references to the ghost GameObjects
    private int currentGhostIndex = 0; // Index of the currently controlled ghost

    void Start()
    {
        // Set all ghosts to active initially
        foreach (var ghost in ghosts)
        {
            ghost.SetActive(true);
        }

        // Set the camera target to the first ghost
        UpdateCameraTarget();
    }

    void Update()
    {
        // Check for input to switch ghosts
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchGhost(0); // Blue ghost
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchGhost(1); // Yellow ghost
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchGhost(2); // Red ghost
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchGhost(3); // Pink ghost
        }
    }

    void SwitchGhost(int newGhostIndex)
    {
        if (newGhostIndex < 0 || newGhostIndex >= ghosts.Length)
        {
            Debug.LogError("Invalid ghost index: " + newGhostIndex);
            return;
        }

        // Disable control for the current ghost
        if (currentGhostIndex < ghosts.Length)
        {
            // Check if the current ghost is the Pink Ghost and it's phasing
            PinkGhostController pinkGhostController = ghosts[currentGhostIndex].GetComponent<PinkGhostController>();
            if (pinkGhostController != null && pinkGhostController.IsPhasing()) // Use the getter
            {
                pinkGhostController.StopWallPhase(); // Stop the wall phase if it's active
            }
            
            ghosts[currentGhostIndex].GetComponent<GhostControllerBase>().ToggleControl(false);
        }

        // Enable control for the new ghost
        currentGhostIndex = newGhostIndex;
        ghosts[currentGhostIndex].GetComponent<GhostControllerBase>().ToggleControl(true);

        UpdateCameraTarget();
    }

    private void UpdateCameraTarget()
    {
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = ghosts[currentGhostIndex].transform; // Set the camera target to the currently controlled ghost
        }
    }
}
