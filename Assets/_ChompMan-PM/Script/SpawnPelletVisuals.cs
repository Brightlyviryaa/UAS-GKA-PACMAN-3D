using UnityEngine;

public class SpawnPelletVisuals : MonoBehaviour
{
    public GameObject pelletPrefab;        // Prefab untuk visual Pellet biasa
    public GameObject powerPelletPrefab;  // Prefab untuk Power Pellet (khusus)

    void Start()
    {
        // Cari semua PowerPelletPositions di scene
        GameObject[] pelletPositions = GameObject.FindGameObjectsWithTag("PowerPelletPosition");

        if (pelletPositions.Length == 0)
        {
            Debug.LogWarning("No PowerPelletPositions found. Ensure positions are correctly tagged.");
            return;
        }

        // Shuffle posisi agar pemilihan random lebih adil
        ShuffleArray(pelletPositions);

        // Spawn Power Pellets di 4 posisi pertama
        for (int i = 0; i < 4; i++)
        {
            SpawnPelletAtPosition(powerPelletPrefab, pelletPositions[i].transform.position);
        }

        // Spawn Pellet biasa di sisa posisi
        for (int i = 4; i < pelletPositions.Length; i++)
        {
            SpawnPelletAtPosition(pelletPrefab, pelletPositions[i].transform.position);
        }

        Debug.Log($"Spawned 4 Power Pellets and {pelletPositions.Length - 4} regular pellets.");
    }

    void SpawnPelletAtPosition(GameObject prefab, Vector3 position)
    {
        // Instansiasi pellet di posisi tertentu
        GameObject pellet = Instantiate(prefab, position, Quaternion.identity);

        // Pastikan layer diatur ke "IgnoreNavMesh"
        pellet.layer = LayerMask.NameToLayer("IgnoreNavMesh");

        // Set collider sebagai trigger
        Collider pelletCollider = pellet.GetComponent<Collider>();
        if (pelletCollider != null)
        {
            pelletCollider.isTrigger = true;
        }
    }

    // Fungsi untuk mengacak array (Fisher-Yates Shuffle)
    void ShuffleArray(GameObject[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // Swap elemen
            GameObject temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
