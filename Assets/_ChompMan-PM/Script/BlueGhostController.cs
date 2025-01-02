using UnityEngine;
using System.Collections;

public class BlueGhostController : GhostControllerBase
{
    public float skillDuration = 5f; // Duration of the invisibility
    public float skillCooldown = 5f; // Cooldown period
    private bool isSkillActive = false;
    private bool isCooldownActive = false;

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

        // Change activation key to "E"
        if (isPlayerControlled && Input.GetKeyDown(KeyCode.E) && !isSkillActive && !isCooldownActive)
        {
            StartCoroutine(ActivateInvisibility());
        }
    }

    private IEnumerator ActivateInvisibility()
    {
        isSkillActive = true;
        isInvisible = true;
        Debug.Log($"Blue Ghost is now Invisible!!");

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

        yield return new WaitForSeconds(skillDuration); // Wait for skill duration

        // Revert to Opaque mode
        ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        ghostMaterial.SetInt("_ZWrite", 1); // Enable writing to the depth buffer
        ghostMaterial.DisableKeyword("_ALPHABLEND_ON");
        ghostMaterial.EnableKeyword("_ALPHATEST_ON");
        ghostMaterial.renderQueue = -1;

        // Revert to original opacity
        ghostMaterial.color = originalColor;

        isInvisible = false;
        isSkillActive = false;
        isCooldownActive = true;

        yield return new WaitForSeconds(skillCooldown); // Wait for cooldown
        isCooldownActive = false;
    }
}
