using UnityEngine;

public class Pellet : MonoBehaviour
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PacMan"))
        {
            // Saat PacMan kena pellet biasa, hanya hancurkan pellet
            Destroy(gameObject);
        }
    }
}
