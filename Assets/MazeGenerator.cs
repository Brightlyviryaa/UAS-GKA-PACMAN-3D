using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private MazeBlock _mazeBlockPrefab;

    [SerializeField]
    private int _mazeWidth;

    [SerializeField]
    private int _mazeDepth;

    private MazeBlock[,] _mazeGrid;
    // Start is called before the first frame update
    void Start()
    {
        _mazeGrid = new MazeBlock[_mazeWidth, _mazeDepth];

        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                _mazeGrid[x, z] = Instantiate(_mazeBlockPrefab, new Vector3(x, 0, z), Quaternion.identity);
            }
        }

        GenerateMaze(null, _mazeGrid[0, 0]);
    }

    private void GenerateMaze(MazeBlock previousBlock, MazeBlock currentBlock)
    {
        currentBlock.Visit();
        ClearWalls(previousBlock, currentBlock);

        MazeBlock nextBlock;

        do
        {
            nextBlock = GetNextUnvisitedBlock(currentBlock);

            if (nextBlock != null)
            {
                GenerateMaze(currentBlock, nextBlock);
            }
        } while (nextBlock != null);
    }

    private MazeBlock GetNextUnvisitedBlock(MazeBlock currentBlock)
    {
        var unvisitedBlocks = GetUnvisitedBlocks(currentBlock);

        return unvisitedBlocks.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeBlock> GetUnvisitedBlocks(MazeBlock currentBlock)
    {
        int x = (int)currentBlock.transform.position.x;
        int z = (int)currentBlock.transform.position.z;

        if (x + 1 < _mazeWidth)
        {
            var cellToRight = _mazeGrid[x + 1, z];

            if(cellToRight.IsVisited == false)
            {
                yield return cellToRight;
            }
        }

        if (x - 1 >= 0)
        {
            var cellToLeft = _mazeGrid[x - 1, z];

            if(cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;
            }
        }

        if (z + 1 < _mazeDepth)
        {
            var cellToFront = _mazeGrid[x, z + 1];

            if(cellToFront.IsVisited == false)
            {
                yield return cellToFront;
            }
        }

        if (z - 1 >= 0)
        {
            var cellToBack = _mazeGrid[x, z - 1];

            if(cellToBack.IsVisited == false)
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearWalls(MazeBlock previousBlock, MazeBlock currentBlock)
    {
        if (previousBlock == null)
        {
            return;
        }

        if (previousBlock.transform.position.x < currentBlock.transform.position.x)
        {
            previousBlock.ClearRightWall();
            currentBlock.ClearLeftWall();
            return;
        }
        
        if (previousBlock.transform.position.x > currentBlock.transform.position.x)
        {
            previousBlock.ClearLeftWall();
            currentBlock.ClearRightWall();
            return;
        }

        if (previousBlock.transform.position.z < currentBlock.transform.position.z)
        {
            previousBlock.ClearFrontWall();
            currentBlock.ClearRearWall();
            return;
        }

        if (previousBlock.transform.position.z > currentBlock.transform.position.z)
        {
            previousBlock.ClearRearWall();
            currentBlock.ClearFrontWall();
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
