using UnityEngine;
using System.Collections;

public class PinkGhostController : GhostControllerBase
{
    public float phaseDuration = 5f; // Duration of the wall-phase
    public float skillCooldown = 10f; // Cooldown period
    public float pulseSpeed = 2f; // Speed of the pulsing effect
    public float pulseAmount = 0.5f; // How much the intensity will increase
    public float minPulseIntensity = 0.2f; // Minimum pulse intensity (lower than original)
    
    private bool isPhasing = false;
    private bool isCooldownActive = false;

    private float originalSpeed = 1f; // Store the original speed to prevent interference

    // Maze boundaries (adjust these values based on your maze)
    private Vector3 mazeMinBounds = new Vector3(0, 0, 0); // Minimum bounds of the maze
    private Vector3 mazeMaxBounds = new Vector3(19, 0, 19); // Maximum bounds of the maze

    private Renderer ghostRenderer; // Renderer of the Blue Ghost
    private Color originalColor; // Store original color
    private Material ghostMaterial; // Material to modify opacity

    void Start()
    {
        base.Start();
        ghostRenderer = GetComponent<Renderer>(); // Access the Renderer component
        ghostMaterial = ghostRenderer.material; // Access the material of the Blue Ghost
        originalColor = ghostMaterial.color; // Store the original color
    }

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

        // Start pulsing the color intensity
        float elapsedTime = 0f;
        while (elapsedTime < phaseDuration)
        {
            // Switch to Transparent mode
            ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ghostMaterial.SetInt("_ZWrite", 0); // Disable writing to the depth buffer
            ghostMaterial.DisableKeyword("_ALPHATEST_ON");
            ghostMaterial.EnableKeyword("_ALPHABLEND_ON");
            ghostMaterial.renderQueue = 3000;

            // Set the opacity to 50% (semi-transparent)
            Color newColor = originalColor;
            newColor.a = 0.5f; // Adjust the opacity (0 is fully transparent, 1 is fully opaque)
            ghostMaterial.color = newColor;

            // You could use a simple movement system here to move the ghost freely
            transform.Translate(Vector3.forward * originalSpeed * Time.deltaTime);

            // Clamp the position to keep the ghost within the maze bounds
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, mazeMinBounds.x, mazeMaxBounds.x),
                transform.position.y,
                Mathf.Clamp(transform.position.z, mazeMinBounds.z, mazeMaxBounds.z)
            );

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        agent.enabled = true; // Re-enable the NavMeshAgent after phase ends
        isPhasing = false;

        // Revert to Opaque mode
        ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        ghostMaterial.SetInt("_ZWrite", 1); // Enable writing to the depth buffer
        ghostMaterial.DisableKeyword("_ALPHABLEND_ON");
        ghostMaterial.EnableKeyword("_ALPHATEST_ON");
        ghostMaterial.renderQueue = -1;

        ghostMaterial.color = originalColor; // Revert back to the original color after skill ends

        isCooldownActive = true;
        yield return new WaitForSeconds(skillCooldown);
        isCooldownActive = false;
    }

    public void StopWallPhase()
    {
        if (isPhasing)
        {
            // Revert to Opaque mode
            ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            ghostMaterial.SetInt("_ZWrite", 1); // Enable writing to the depth buffer
            ghostMaterial.DisableKeyword("_ALPHABLEND_ON");
            ghostMaterial.EnableKeyword("_ALPHATEST_ON");
            ghostMaterial.renderQueue = -1;

            ghostMaterial.color = originalColor; // Revert back to the original color after skill ends
            // Immediately end the phasing
            StopCoroutine(ActivateWallPhase()); // Stop the wall-phase coroutine
            agent.enabled = true; // Re-enable the NavMeshAgent
            isPhasing = false; // Set the phasing status to false
        }
    }

    public bool IsPhasing()
    {
        return isPhasing;
    }
}
