using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST2_MazeGenerator : MonoBehaviour
{
    [SerializeField] private GameObject mazeBlockPrefab;  // Reference to the TEST_MazeBlock prefab
    [SerializeField] private int mazeWidth = 10;          // Width of the maze (number of blocks)
    [SerializeField] private int mazeDepth = 10;          // Depth of the maze (number of blocks)
    [SerializeField] private Vector2Int spawnPointSize = new Vector2Int(3, 3);  // The size of the ghost spawn point (3x3 area, for example)

    void Start()
    {
        // Ensure spawnPointSize.x is at least 4
        if (spawnPointSize.x < 4)
        {
            Debug.LogWarning("Spawn point width (x) is less than 4. Setting to minimum size of 4.");
            spawnPointSize.x = 4;
        }

        GenerateMazeBorder();  // Generate the maze border instantly
    }

    void GenerateMazeBorder()
    {
        // Calculate the middle area for ghost spawn (for example, the center spawnPointSize area)
        int spawnStartX = (mazeWidth - spawnPointSize.x) / 2;
        int spawnStartZ = (mazeDepth - spawnPointSize.y) / 2;
        int spawnEndX = spawnStartX + spawnPointSize.x - 1;
        int spawnEndZ = spawnStartZ + spawnPointSize.y - 1;

        // Determine the middle X positions for removing the _frontWall
        int middleX1 = spawnStartX + (spawnPointSize.x - 1) / 2;  // First middle index
        int middleX2 = spawnPointSize.x % 2 == 0 ? middleX1 + 1 : middleX1;  // Second middle index (only for even sizes)

        // Instantiate walls for the border of the maze
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeDepth; z++)
            {
                // Instantiate the maze block at the correct position
                Vector3 position = new Vector3(x, 0, z); // Position the blocks along x and z axis
                GameObject mazeBlock = Instantiate(mazeBlockPrefab, position, Quaternion.identity, transform);  // Instantiate and parent it to this object

                // Check if TEST_MazeBlock component exists on the instantiated block
                TEST_MazeBlock mazeBlockScript = mazeBlock.GetComponent<TEST_MazeBlock>();
                if (mazeBlockScript == null)
                {
                    Debug.LogError("TEST_MazeBlock script not found on the instantiated maze block at position: " + position);
                    continue;
                }

                // Disable all walls first
                mazeBlockScript.ClearAllWalls();
                mazeBlockScript.ClearCenterFill();

                // Check adjacency to the spawn point
                bool isAdjacentToSpawn =
                    (x >= spawnStartX - 1 && x <= spawnEndX + 1 && z >= spawnStartZ - 1 && z <= spawnEndZ + 1) &&
                    !(x >= spawnStartX && x <= spawnEndX && z >= spawnStartZ && z <= spawnEndZ);

                if (isAdjacentToSpawn)
                {
                    // If adjacent to spawn point, leave walls off (no walls)
                    continue;
                }

                // Enable specific walls based on the position
                if (x == 0) mazeBlockScript.SetLeftWall();  // Left side
                if (x == mazeWidth - 1) mazeBlockScript.SetRightWall();  // Right side
                if (z == 0) mazeBlockScript.SetRearWall();  // Rear side (top row, z == 0)
                if (z == mazeDepth - 1) mazeBlockScript.SetFrontWall();  // Front side (bottom row, z == mazeDepth - 1)

                // Randomize blocks inside the maze
                if (x > 0 && x < mazeWidth - 1 && z > 0 && z < mazeDepth - 1)
                {
                    // Check if the current position is within the spawn area
                    if (x >= spawnStartX && x <= spawnEndX && z >= spawnStartZ && z <= spawnEndZ)
                    {
                        // It's within the spawn point area
                        if (z == spawnEndZ) // Front wall logic for spawn area
                        {
                            if (x == middleX1 || x == middleX2)
                            {
                                // Leave the front wall open for middle prefabs
                                continue;
                            }
                        }

                        // Add walls for the spawn area's boundary
                        if (x == spawnStartX) mazeBlockScript.SetLeftWall();
                        if (x == spawnEndX) mazeBlockScript.SetRightWall();
                        if (z == spawnStartZ) mazeBlockScript.SetRearWall();
                        if (z == spawnEndZ) mazeBlockScript.SetFrontWall();
                    }
                    else
                    {
                        // Check if the block is NOT a border or adjacent to spawn point
                        if (!isAdjacentToSpawn && x != 0 && x != mazeWidth - 1 && z != 0 && z != mazeDepth - 1)
                        {
                            // Randomize between the two states
                            if (Random.value > 0.65f)
                            {
                                // Set all walls and centerFill
                                mazeBlockScript.ClearAllWalls();
                                mazeBlockScript.SetCenterFill();
                            }
                            else
                            {
                                // Clear all walls and centerFill
                                mazeBlockScript.ClearAllWalls();
                                mazeBlockScript.ClearCenterFill();
                            }
                        }
                    }
                }
            }
        }
    }
}
