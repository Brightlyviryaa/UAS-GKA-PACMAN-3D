using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private TEST_MazeBlock _mazeBlockPrefab;

    [SerializeField]
    private int _mazeWidth;

    [SerializeField]
    private int _mazeDepth;

    private TEST_MazeBlock[,] _mazeGrid;

    [SerializeField, Range(0, 1)]
    private float openAreaChance = 0.2f; // Chance for open spaces

    private void GenerateGrid()
    {
        // Create the base grid
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                _mazeGrid[x, z] = Instantiate(_mazeBlockPrefab, new Vector3(x, 0, z), Quaternion.identity);

                // Default: Set all walls
                _mazeGrid[x, z].SetAllWalls();
            }
        }
    }

    private void GenerateMaze()
    {
        bool[,] visited = new bool[_mazeWidth, _mazeDepth];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // Start at a random position in the maze
        Vector2Int startPosition = new Vector2Int(Random.Range(0, _mazeWidth), Random.Range(0, _mazeDepth));
        stack.Push(startPosition);
        visited[startPosition.x, startPosition.y] = true;

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current, visited);

            if (neighbors.Count > 0)
            {
                // Pick a random neighbor
                Vector2Int chosenNeighbor = neighbors[Random.Range(0, neighbors.Count)];

                // Remove the wall between the current block and the chosen neighbor
                RemoveWallBetween(current, chosenNeighbor);

                // Mark the neighbor as visited and push it to the stack
                visited[chosenNeighbor.x, chosenNeighbor.y] = true;
                stack.Push(chosenNeighbor);
            }
            else
            {
                // Backtrack only if there are no unvisited neighbors
                stack.Pop();
            }
        }

        // After maze generation, ensure no dead-ends
        EnsureNoDeadEnds();
    }

    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int current, bool[,] visited)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = current + dir;

            if (neighbor.x >= 0 && neighbor.x < _mazeWidth &&
                neighbor.y >= 0 && neighbor.y < _mazeDepth &&
                !visited[neighbor.x, neighbor.y])
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private void RemoveWallBetween(Vector2Int current, Vector2Int neighbor)
    {
        TEST_MazeBlock currentBlock = _mazeGrid[current.x, current.y];
        TEST_MazeBlock neighborBlock = _mazeGrid[neighbor.x, neighbor.y];

        if (current.x < neighbor.x)
        {
            // Neighbor is to the right
            currentBlock.ClearRightWall();
            neighborBlock.ClearLeftWall();
        }
        else if (current.x > neighbor.x)
        {
            // Neighbor is to the left
            currentBlock.ClearLeftWall();
            neighborBlock.ClearRightWall();
        }
        else if (current.y < neighbor.y)
        {
            // Neighbor is in front
            currentBlock.ClearFrontWall();
            neighborBlock.ClearRearWall();
        }
        else if (current.y > neighbor.y)
        {
            // Neighbor is behind
            currentBlock.ClearRearWall();
            neighborBlock.ClearFrontWall();
        }
    }

    // Ensure no dead-ends in the maze after generation
    private void EnsureNoDeadEnds()
    {
        // Loop through the grid and identify dead-ends
        for (int x = 1; x < _mazeWidth - 1; x++)
        {
            for (int z = 1; z < _mazeDepth - 1; z++)
            {
                int wallCount = CountAdjacentWalls(x, z);
                // If a block has more than 3 walls (it could be a dead-end), connect it to an adjacent block
                if (wallCount >= 3)
                {
                    // Remove a wall to connect the block to the path
                    _mazeGrid[x, z].ClearRandomWall();
                }
            }
        }
    }

    private int CountAdjacentWalls(int x, int z)
    {
        int wallCount = 0;
        if (_mazeGrid[x, z].HasFrontWall()) wallCount++;
        if (_mazeGrid[x, z].HasRearWall()) wallCount++;
        if (_mazeGrid[x, z].HasLeftWall()) wallCount++;
        if (_mazeGrid[x, z].HasRightWall()) wallCount++;

        return wallCount;
    }

    // Call GenerateMaze in Start
    void Start()
    {
        _mazeGrid = new TEST_MazeBlock[_mazeWidth, _mazeDepth];

        GenerateGrid();
        GenerateMaze(); // Create the maze
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
