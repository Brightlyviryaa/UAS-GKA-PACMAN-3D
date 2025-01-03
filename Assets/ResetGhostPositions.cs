using UnityEngine;

public class ResetGhostPositions : StateMachineBehaviour
{
    private GhostManager ghostManager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find the GhostManager in the scene
        ghostManager = GameObject.FindObjectOfType<GhostManager>();

        if (ghostManager != null)
        {
            ghostManager.RespawnAllGhosts();
        }
        else
        {
            Debug.LogError("GhostManager not found in the scene!");
        }
    }
}
