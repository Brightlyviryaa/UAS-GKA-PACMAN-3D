using UnityEngine;
using UnityEditor;

public class PowerPelletConverter : MonoBehaviour
{
    [MenuItem("Tools/Convert PowerPellets to Empty GameObjects")]
    public static void ConvertPowerPellets()
    {
        // Cari Pellet Parents
        GameObject rightSideParent = GameObject.Find("Pellet Parent/Pellet for the Right Side");
        GameObject leftSideParent = GameObject.Find("Pellet Parent/Pellet for the Left Side");

        if (rightSideParent == null || leftSideParent == null)
        {
            Debug.LogError("Pellet Parent or its children are not found. Please check the hierarchy names.");
            return;
        }

        // Buat parent untuk menyimpan posisi baru
        GameObject positionsParent = new GameObject("PowerPelletPositions");

        // Konversi semua PowerPellets di Right Side
        ConvertChildObjectsToEmpty(rightSideParent, positionsParent);

        // Konversi semua PowerPellets di Left Side
        ConvertChildObjectsToEmpty(leftSideParent, positionsParent);

        Debug.Log("All PowerPellets have been converted to Empty GameObjects.");
    }

    private static void ConvertChildObjectsToEmpty(GameObject parent, GameObject positionsParent)
    {
        foreach (Transform child in parent.transform)
        {
            // Ambil posisi setiap child
            Vector3 position = child.position;

            // Buat Empty GameObject pada posisi tersebut
            GameObject emptyPellet = new GameObject("PowerPelletPosition");
            emptyPellet.transform.position = position;

            // Parent-kan Empty GameObject ini ke PowerPelletPositions
            emptyPellet.transform.parent = positionsParent.transform;

            // Hapus PowerPellet asli
            DestroyImmediate(child.gameObject);
        }
    }
}
