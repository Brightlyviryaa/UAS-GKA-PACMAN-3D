using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUiScript : MonoBehaviour
{
    public PacManController pacMan; // Reference to Pac-Man
    public Text livesText; // Reference to the UI Text

    void Start()
    {
        livesText.text = "Pacman Lives: " + pacMan.lives;
    }

    void Update()
    {
        livesText.text = "Pacman Lives: " + pacMan.lives; // Update the text
    }
}
