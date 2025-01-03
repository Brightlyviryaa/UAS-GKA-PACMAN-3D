using UnityEngine;
using UnityEngine.SceneManagement; // Import to work with scene loading
using UnityEngine.UI; // Import for button interactions

public class GameOverManager : MonoBehaviour
{
    public Button tryAgainButton;
    public Button mainMenuButton;

    void Start()
    {
        Time.timeScale = 1f; // Unfreeze the game
        // Attach button listeners
        tryAgainButton.onClick.AddListener(OnTryAgainPressed);
        mainMenuButton.onClick.AddListener(OnMainMenuPressed);
    }

    // Method called when Try Again button is pressed
    void OnTryAgainPressed()
    {
        // Reload the current scene (fresh start)
        SceneManager.LoadScene("Game_Randomized_Maze_Test");
    }

    // Method called when Main Menu button is pressed
    void OnMainMenuPressed()
    {
        // Load the Main Menu scene
        SceneManager.LoadScene("Main Menu");
    }
}
