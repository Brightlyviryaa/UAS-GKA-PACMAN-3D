using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class OrangeGhostController : GhostControllerBase
{
    [SerializeField]
    public GameObject decoyPrefab; // The prefab for the decoy
    public float decoyDuration = 5f; // Duration of the decoy
    public float skillCooldown = 10f; // Cooldown period

    private bool isDecoyActive = false;
    private bool isCooldownActive = false;

    void Update()
    {
        base.Update();

        if (isPlayerControlled && Input.GetKeyDown(KeyCode.E) && !isDecoyActive && !isCooldownActive)
        {
            StartCoroutine(ActivateDecoy());
        }
    }

    private IEnumerator ActivateDecoy()
    {
        isDecoyActive = true;

        // Instantiate the decoy at the ghost's current position with modified Y value
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = 0f;  // Set the Y value to the desired height (example: 1.0f)

        // Instantiate the decoy at the ghost's current position
        GameObject decoy = Instantiate(decoyPrefab, spawnPosition, Quaternion.identity);

        // Set the decoy's tag and layer
        decoy.tag = "PlayerGhost"; // Set the tag to PlayerGhost
        decoy.layer = LayerMask.NameToLayer("Ghost"); // Set the layer to Ghost

        decoy.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        // Add GhostControllerBase component to the decoy to inherit its behavior
        GhostControllerBase decoyController = decoy.GetComponent<GhostControllerBase>();
        if (decoyController == null)
        {
            decoyController = decoy.AddComponent<GhostControllerBase>();
        }
    
        // Adjust NavMeshAgent properties
        NavMeshAgent decoyAgent = decoy.GetComponent<NavMeshAgent>();
        if (decoyAgent != null)
        {
            decoyAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            decoyAgent.height = 1.1f;
        }

        decoyController.ghostSpeed = ghostSpeed;

        decoyController.wanderRadius = 50f; // Set this to a value suitable for your decoy
        decoyController.detectionRadius = 8f; // Set this to a value suitable for your decoy

        // Optionally, make the decoy move around (here's a simple random movement example)
        float startTime = Time.time;
        while (Time.time - startTime < decoyDuration)
        {
            yield return null;
        }

        // Destroy the decoy after the duration
        Destroy(decoy);

        // Start cooldown
        isCooldownActive = true;
        yield return new WaitForSeconds(skillCooldown);
        isCooldownActive = false;
        isDecoyActive = false;
    }
}
