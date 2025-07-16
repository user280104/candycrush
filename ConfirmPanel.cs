using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ConfirmPanel : MonoBehaviour
{
    [Header("Level Information")]
    public string levelToLoad;
    public int level;
    private GameData gameData;
    private int starsActive;
    private int highScore;

    [Header("UI stuff")]
    public Image[] stars;
    public TMP_Text highScoreText;
    public TMP_Text starText;

   
    void OnEnable()
    {
        gameData = FindObjectOfType<GameData>();

        if (gameData == null || gameData.saveData == null || gameData.saveData.stars == null)
        {
            Debug.LogError("GameData or SaveData is not initialized properly.");
            return;
        }

        // Validate level range
        if (level < 1 || level > gameData.saveData.stars.Length)
        {
            Debug.LogError($"Invalid level: {level}. It must be between 1 and {gameData.saveData.stars.Length}.");
            return;
        }

        LoadData();
        ActivateStars();
        SetText();
    }

    void LoadData()
    {
        if (gameData != null && level - 1 < gameData.saveData.stars.Length)
        {
            starsActive = gameData.saveData.stars[level - 1];
            highScore = gameData.saveData.highScores[level - 1];
        }
        else
        {
            Debug.LogError("Level data is out of bounds or GameData is null.");
        }
    }

    void SetText()
    {
        highScoreText.text = "" + highScore;
        starText.text = starsActive + "/3";
    }

    void ActivateStars()
    {
        for (int i = 0; i < starsActive && i < stars.Length; i++)
        {
            stars[i].enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Cancel()
    {
        this.gameObject.SetActive(false);
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level", level - 1);
        SceneManager.LoadScene(levelToLoad);
    }
}
