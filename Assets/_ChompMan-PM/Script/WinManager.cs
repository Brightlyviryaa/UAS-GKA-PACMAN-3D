using UnityEngine;
using UnityEngine.SceneManagement; // Import to work with scene loading
using UnityEngine.UI; // Import for button interactions

public class WinManager : MonoBehaviour
{
    public Button playAgainButton;
    public Button mainMenuButton;

    void Start()
    {
        Time.timeScale = 1f; // Unfreeze the game
        // Attach button listeners
        playAgainButton.onClick.AddListener(OnPlayAgainPressed);
        mainMenuButton.onClick.AddListener(OnMainMenuPressed);
    }

    // Method called when Try Again button is pressed
    void OnPlayAgainPressed()
    {
        // Load the Game_Randomized_Maze_Test scene
        SceneManager.LoadScene("Game_Randomized_Maze_Test");
    }

    // Method called when Main Menu button is pressed
    void OnMainMenuPressed()
    {
        // Load the Main Menu scene
        SceneManager.LoadScene("Main Menu");
    }
}
