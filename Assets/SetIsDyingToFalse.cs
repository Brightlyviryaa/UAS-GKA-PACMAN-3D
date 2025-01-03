using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetIsDyingToFalse : StateMachineBehaviour
{
    // This is a reference to the Animator where the 'isDying' parameter is stored
    private Animator animator;

    // Called when the animation state is exited
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Set the 'isDying' parameter to false
        animator.SetBool("isDying", false);
    }
}