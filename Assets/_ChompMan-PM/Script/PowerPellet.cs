using UnityEngine;

public class PowerPellet : Pellet
{
    public float effectDuration = 5f; // Durasi efek Power Pellet

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other.CompareTag("PacMan"))
        {
            PacManController pacMan = other.GetComponent<PacManController>();
            if (pacMan != null)
            {
                pacMan.ActivatePowerMode(effectDuration);
                Debug.Log("Power Pellet activated! PacMan can eat ghosts now.");
            }
            else
            {
                Debug.LogError("PacManController script is missing on PacMan!");
            }
        }
    }
}
