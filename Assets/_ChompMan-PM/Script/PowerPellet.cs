using UnityEngine;

public class PowerPellet : Pellet
{
    public float effectDuration = 5f; // Durasi efek Power Pellet

    // Override metode induk
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other); // Panggil logika dasar dari Pellet

        if (other.CompareTag("PacMan"))
        {
            ActivatePowerEffect(); // Tambahkan efek khusus PowerPellet
        }
    }

    private void ActivatePowerEffect()
    {
        Debug.Log("Power Pellet activated! Applying special effects.");
        // Tambahkan logika efek PowerPellet di sini
    }
}
