using UnityEngine;

public class Pellet : MonoBehaviour
{
    // Tandai metode ini sebagai virtual untuk memungkinkan override
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PacMan"))
        {
            // Logika dasar: hancurkan pellet
            Destroy(gameObject);
        }
    }
}
