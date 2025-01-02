using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayGameButton : MonoBehaviour
{
    [SerializeField] private string Game_Randomized_Maze_Test;

    public void OnPlayGameButtonPressed()
    {
        if (string.IsNullOrEmpty(Game_Randomized_Maze_Test))
        {
            Debug.LogError("Scene name is not set in the PlayGameButton script.");
            return;
        }

        if (SceneExists(Game_Randomized_Maze_Test))
        {
            Debug.Log($"Loading scene: {Game_Randomized_Maze_Test}");
            SceneManager.LoadScene(Game_Randomized_Maze_Test);
        }
        else
        {
            Debug.LogError($"Scene \"{Game_Randomized_Maze_Test}\" is not added to the build settings or doesn't exist.");
        }
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneFileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneFileName == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}
