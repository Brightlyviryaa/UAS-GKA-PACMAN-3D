using UnityEngine;
using System.Collections;

public class RedGhostController : GhostControllerBase
{
    public float speedBoostMultiplier = 5f; // Multiplier for speed boost
    public float skillDuration = 5f; // Duration of the speed boost
    public float skillCooldown = 5f; // Cooldown period
    public float pulseSpeed = 0.5f; // Speed of the pulsing effect
    public float pulseAmount = 5f; // How much the intensity will increase
    public float minPulseIntensity = 0.0f; // Minimum pulse intensity (lower than original)

    private bool isSkillActive = false;
    private bool isCooldownActive = false;

    private Renderer ghostRenderer; // Renderer of the Red Ghost
    private Material ghostMaterial; // Material to modify color intensity
    private Color originalColor; // Store original color

    void Start()
    {
        base.Start();
        ghostRenderer = GetComponent<Renderer>(); // Access the Renderer component
        ghostMaterial = ghostRenderer.material; // Access the material of the Red Ghost
        originalColor = ghostMaterial.color; // Store the original color
    }

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

        // Start pulsing the color intensity
        float elapsedTime = 0f;
        while (elapsedTime < skillDuration)
        {
            // Calculate the pulse effect using Mathf.PingPong or Mathf.Sin
            float pulseFactor = Mathf.PingPong(Time.time * pulseSpeed, pulseAmount); // Pulsing effect
            float intensityFactor = Mathf.Lerp(minPulseIntensity, 5f, pulseFactor); // Ensure the intensity doesn't drop too low

            Color newColor = originalColor * intensityFactor; // Increase intensity

            ghostMaterial.color = newColor; // Apply the new color to the ghost material

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Revert back to the original color after skill duration ends
        ghostMaterial.color = originalColor;

        ghostSpeed = originalSpeed;
        agent.speed = ghostSpeed;
        isSkillActive = false;
        isCooldownActive = true;

        yield return new WaitForSeconds(skillCooldown);
        isCooldownActive = false;
    }
}
