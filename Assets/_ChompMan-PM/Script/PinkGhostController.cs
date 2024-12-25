using UnityEngine;
using System.Collections;

public class PinkGhostController : GhostControllerBase
{
    public float phaseDuration = 5f; // Duration of the wall-phase
    public float skillCooldown = 10f; // Cooldown period
    private bool isPhasing = false;
    private bool isCooldownActive = false;

    private float originalSpeed = 1f; // Store the original speed to prevent interference

    // Maze boundaries (adjust these values based on your maze)
    private Vector3 mazeMinBounds = new Vector3(0, 0, 0); // Minimum bounds of the maze
    private Vector3 mazeMaxBounds = new Vector3(19, 0, 19); // Maximum bounds of the maze

    void Update()
    {
        base.Update();

        if (isPlayerControlled && Input.GetKeyDown(KeyCode.E) && !isPhasing && !isCooldownActive)
        {
            StartCoroutine(ActivateWallPhase());
        }
    }

    private IEnumerator ActivateWallPhase()
    {
        isPhasing = true;
        agent.enabled = false; // Disable the NavMeshAgent

        // You could use a simple movement system here
        float startTime = Time.time;
        while (Time.time - startTime < phaseDuration)
        {
            // Move the ghost freely, without NavMesh constraints (example: move it forward)
            transform.Translate(Vector3.forward * originalSpeed * Time.deltaTime);

            // Clamp the position to keep the ghost within the maze bounds
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, mazeMinBounds.x, mazeMaxBounds.x),
                transform.position.y,
                Mathf.Clamp(transform.position.z, mazeMinBounds.z, mazeMaxBounds.z)
            );

            yield return null;
        }

        agent.enabled = true; // Re-enable the NavMeshAgent after phase ends
        isPhasing = false;

        isCooldownActive = true;
        yield return new WaitForSeconds(skillCooldown);
        isCooldownActive = false;
    }
}
