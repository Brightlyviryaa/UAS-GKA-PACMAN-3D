using UnityEngine;
using System.Collections;

public class RedGhostController : GhostControllerBase
{
    public float speedBoostMultiplier = 5f; // Multiplier for speed boost
    public float skillDuration = 5f; // Duration of the speed boost
    public float skillCooldown = 5f; // Cooldown period

    private bool isSkillActive = false;
    private bool isCooldownActive = false;

    void Update()
    {
        base.Update();

        // Change activation key to "E"
        if (isPlayerControlled && Input.GetKeyDown(KeyCode.E) && !isSkillActive && !isCooldownActive)
        {
            StartCoroutine(ActivateSpeedBoost());
        }
    }

    private IEnumerator ActivateSpeedBoost()
    {
        isSkillActive = true;
        float originalSpeed = ghostSpeed;
        ghostSpeed *= speedBoostMultiplier;
        agent.speed = ghostSpeed; // Update NavMeshAgent speed

        yield return new WaitForSeconds(skillDuration);

        ghostSpeed = originalSpeed;
        agent.speed = ghostSpeed;
        isSkillActive = false;
        isCooldownActive = true;

        yield return new WaitForSeconds(skillCooldown);
        isCooldownActive = false;
    }
}
