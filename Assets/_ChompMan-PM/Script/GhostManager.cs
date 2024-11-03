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
            return; // Exit the method if the index is invalid
        }

        // Toggle control for the current ghost
        if (currentGhostIndex < ghosts.Length)
        {
            switch (currentGhostIndex)
            {
                case 0: // Blue Ghost
                    ghosts[currentGhostIndex].GetComponent<BlueGhostController>().ToggleControl(false);
                    break;
                case 1: // Yellow Ghost
                    ghosts[currentGhostIndex].GetComponent<OrangeGhostController>().ToggleControl(false);
                    break;
                case 2: // Red Ghost
                    ghosts[currentGhostIndex].GetComponent<PinkGhostController>().ToggleControl(false);
                    break;
                case 3: // Pink Ghost
                    ghosts[currentGhostIndex].GetComponent<RedGhostController>().ToggleControl(false);
                    break;
            }
        }

        // Update to the new ghost index
        currentGhostIndex = newGhostIndex;

        // Toggle control for the new ghost
        if (currentGhostIndex < ghosts.Length)
        {
            switch (currentGhostIndex)
            {
                case 0: // Blue Ghost
                    ghosts[currentGhostIndex].GetComponent<BlueGhostController>().ToggleControl(true);
                    break;
                case 1: // Yellow Ghost
                    ghosts[currentGhostIndex].GetComponent<OrangeGhostController>().ToggleControl(true);
                    break;
                case 2: // Red Ghost
                    ghosts[currentGhostIndex].GetComponent<PinkGhostController>().ToggleControl(true);
                    break;
                case 3: // Pink Ghost
                    ghosts[currentGhostIndex].GetComponent<RedGhostController>().ToggleControl(true);
                    break;
            }
        }

        // Update the camera's target
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
