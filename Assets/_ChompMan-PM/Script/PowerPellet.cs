using UnityEngine;

public class PowerPellet : MonoBehaviour
{
    public float effectDuration = 5f; // Durasi efek Power Pellet

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PacMan"))
        {
            // Saat PacMan kena pellet biasa, hanya hancurkan pellet
            Destroy(gameObject);

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
