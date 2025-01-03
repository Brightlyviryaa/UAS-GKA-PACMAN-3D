using UnityEngine;

public class ResetCameraToGhost : StateMachineBehaviour
{
    private GhostManager ghostManager; // Reference to the GhostManager script

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find the GhostManager in the scene
        ghostManager = GameObject.FindObjectOfType<GhostManager>();

        if (ghostManager != null)
        {
            // Call the UpdateCameraTarget method in GhostManager
            ghostManager.UpdateCameraTarget();
        }
        else
        {
            Debug.LogError("GhostManager not found in the scene!");
        }
    }
}