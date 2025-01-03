using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnfreezeGame : StateMachineBehaviour
{
    // Reference to PacManController
    private PacManController pacManController;

    // Called when the animation state is entered (when the "died.anim" state starts)
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get reference to the PacManController (ensure PacMan has the script attached)
        pacManController = animator.GetComponent<PacManController>();

        // Call the PacManRespawn function if PacManController is found
        if (pacManController != null)
        {
            pacManController.UnfreezeGame();
        }
    }
}
