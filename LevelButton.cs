using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [Header("Active Stuff")]
    public bool isActive;
    public Sprite activeSprite;
    public Sprite lockedSprite;
    private Image buttonImage;
    private Button myButton;
    private int starsActivate;


    [Header("Level UI")]
    public Image[] stars;
    public TMP_Text levelText;
    public int level;
    public GameObject confirmPanel;


    private GameData gameData;

    // Start is called before the first frame update
    void Start()
    {
        gameData = FindObjectOfType<GameData>();

        // Validate GameData and SaveData
        if (gameData == null || gameData.saveData == null)
        {
            Debug.LogError("GameData or SaveData is null. Ensure GameData is properly initialized.");
            return;
        }

        // Validate the level range
        if (level <= 0 || level > gameData.saveData.isActive.Length)
        {
            Debug.LogError($"Invalid level: {level}. Ensure it is within the range 1 to {gameData.saveData.isActive.Length}.");
            return;
        }

        buttonImage = GetComponent<Image>();
        myButton = GetComponent<Button>();
        LoadData();
        ActivateStars();
        showLevel();
        DecideSprite();
    }

    void LoadData()
    {
        if (gameData != null && gameData.saveData != null)
        {
            // Validate isActive array access
            if (level - 1 >= 0 && level - 1 < gameData.saveData.isActive.Length)
            {
                isActive = gameData.saveData.isActive[level - 1];
            }
            else
            {
                Debug.LogWarning($"Level index {level - 1} is out of bounds for isActive array.");
                isActive = false;
            }

            // Validate stars array access
            if (level - 1 >= 0 && level - 1 < gameData.saveData.stars.Length)
            {
                starsActivate = gameData.saveData.stars[level - 1];
            }
            else
            {
                Debug.LogWarning($"Level index {level - 1} is out of bounds for stars array.");
                starsActivate = 0;
            }
        }
        else
        {
            Debug.LogError("GameData or SaveData is null. Ensure GameData is properly initialized.");
        }
    }


    void ActivateStars()
    {

        for (int i = 0; i < starsActivate; i++)
        {
            stars[i].enabled = true;
        }
    }
    void DecideSprite()
    {
        if (isActive)
        {
            buttonImage.sprite = activeSprite;
            myButton.enabled = true;
            levelText.enabled = true;
        }
        else
        {
            buttonImage.sprite = lockedSprite;
            myButton.enabled = false;
            levelText.enabled = true;
        }
    }
    void showLevel()
    {
        levelText.text = "" + level;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ConfirmPanel(int level)
    {
        confirmPanel.GetComponent<ConfirmPanel>().level = level;
        confirmPanel.SetActive(true);
    }
}
