using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownButton : MonoBehaviour
{
    [SerializeField] private Button skillButton; // Reference to the button
    [SerializeField] private Text buttonText;    // Reference to the text component

    private int[] cooldownTimes = { 45, 30, 15, 10 }; // Cooldown times for each ghost
    private float[] cooldownTimers = { 0, 0, 0, 0 }; // Tracks remaining cooldown time for each ghost
    private int currentGhost = 0; // Current selected ghost (0-based index)

    private void Start()
    {
        if (skillButton == null || buttonText == null)
        {
            Debug.LogError("Button or Text reference is missing.");
            return;
        }

        // Set button initial state
        skillButton.onClick.AddListener(UseSkill);
        UpdateButtonText();
    }

    private void Update()
    {
        // Handle ghost selection
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchGhost(0, "Invisibility");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchGhost(1, "Decoy");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchGhost(2, "Wall-Phasing");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchGhost(3, "Movement Speed Buff");
        }

        // Handle skill use
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers[currentGhost] <= 0)
        {
            UseSkill();
        }

        // Update cooldown timers
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0)
            {
                cooldownTimers[i] -= Time.deltaTime;
                if (i == currentGhost)
                {
                    UpdateButtonText();
                }
            }
        }
    }

    private void SwitchGhost(int ghostIndex, string ghostName)
    {
        currentGhost = ghostIndex;
        UpdateButtonText(ghostName);
    }

    private void UseSkill()
    {
        // Start cooldown for the current ghost
        cooldownTimers[currentGhost] = cooldownTimes[currentGhost];
        skillButton.interactable = false;
        StartCoroutine(Cooldown(currentGhost));
    }

    private IEnumerator Cooldown(int ghostIndex)
    {
        // Wait for the cooldown to complete
        while (cooldownTimers[ghostIndex] > 0)
        {
            yield return null;
        }

        if (ghostIndex == currentGhost)
        {
            skillButton.interactable = true;
            UpdateButtonText();
        }
    }

    private void UpdateButtonText(string ghostName = null)
    {
        if (ghostName != null)
        {
            buttonText.text = ghostName;
        }
        else if (cooldownTimers[currentGhost] > 0)
        {
            buttonText.text = Mathf.CeilToInt(cooldownTimers[currentGhost]).ToString();
        }
        else
        {
            buttonText.text = "Skill Available";
        }
    }
}
