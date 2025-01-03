using UnityEngine;
using UnityEditor;

public class TagPowerPelletPositions : MonoBehaviour
{
    [MenuItem("Tools/Tag All PowerPelletPositions")]
    public static void TagAllPowerPelletPositions()
    {
        // Pastikan tag "PowerPelletPosition" ada di project
        if (!TagExists("PowerPelletPosition"))
        {
            Debug.LogError("Tag 'PowerPelletPosition' does not exist. Please create it in the Tag Manager.");
            return;
        }

        // Cari semua GameObjects bernama "PowerPelletPosition"
        GameObject[] powerPelletPositions = GameObject.FindObjectsOfType<GameObject>();

        int taggedCount = 0;
        foreach (GameObject obj in powerPelletPositions)
        {
            if (obj.name == "PowerPelletPosition") // Hanya beri tag pada objek dengan nama ini
            {
                obj.tag = "PowerPelletPosition";
                taggedCount++;
            }
        }

        Debug.Log($"Tagged {taggedCount} objects as 'PowerPelletPosition'.");
    }

    private static bool TagExists(string tag)
    {
        // Cek apakah tag sudah ada di Tag Manager
        foreach (string existingTag in UnityEditorInternal.InternalEditorUtility.tags)
        {
            if (existingTag == tag)
                return true;
        }
        return false;
    }
}
