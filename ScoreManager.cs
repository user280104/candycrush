using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private Board board;
    public TMP_Text scoreText; // Reference to the TextMeshPro text component for displaying the score
    public int score; // Current score
    public Image scoreBar; // Reference to the score bar UI element
    private GameData gameData; // Reference to the GameData for saving/loading scores
    private int numberStars; // Number of stars earned

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        gameData = FindObjectOfType<GameData>();
        UpdateBar(); // Update the score bar based on the initial score
        if (gameData != null)
        {
            gameData.Load(); // Load saved game data
        }
        UpdateScoreText(); // Initialize the score text display
    }

    // Method to increase the score
    public void IncreaseScore(int amountToIncrease)
    {
        score += amountToIncrease; // Increase the score by the specified amount
        UpdateScoreText(); // Update the score text display

        // Check for star achievements
        for (int i = 0; i < board.scoreGoals.Length; i++)
        {
            if (score > board.scoreGoals[i] && numberStars < i + 1)
            {
                numberStars++;
            }
        }

        // Save high scores and stars if applicable
        if (gameData != null)
        {
            int highScore = gameData.saveData.highScores[board.level];
            if (score > highScore)
            {
                gameData.saveData.highScores[board.level] = score; // Update high score
            }
            int currentStars = gameData.saveData.stars[board.level];
            if (numberStars > currentStars)
            {
                gameData.saveData.stars[board.level] = numberStars; // Update stars
            }
            gameData.Save(); // Save the game data
        }
        UpdateBar(); // Update the score bar
    }

    // Method to update the score text display
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString(); // Set the score text to the current score
        }
        else
        {
            Debug.LogWarning("scoreText is not assigned!"); // Log a warning if scoreText is not assigned
        }
    }

    // Method to update the score bar based on the current score
    private void UpdateBar()
    {
        if (board != null && scoreBar != null)
        {
            int length = board.scoreGoals.Length;
            scoreBar.fillAmount = (float)score / (float)board.scoreGoals[length - 1]; // Update the fill amount of the score bar
        }
    }
}