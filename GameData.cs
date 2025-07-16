using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
// Binary format means game will be able to read and write from it
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;

    // Constructor to initialize arrays with the correct size
    public SaveData(int totalLevels)
    {
        isActive = new bool[totalLevels];
        highScores = new int[totalLevels];
        stars = new int[totalLevels];

        // Set the first level as active
        if (totalLevels > 0)
        {
            isActive[0] = true;
        }
    }
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;

    // Set the total number of levels in your game
    public int totalLevels = 9; // Adjust this to match your actual game levels

    void Awake()
    {
        if (gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;

            // Initialize or load SaveData
            LoadOrInitializeSaveData();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Save()
    {
        // Create binary formatter which can read binary files
        BinaryFormatter formatter = new BinaryFormatter();

        // Create a route from the program to the file
        FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Create);

        // Save the data to the file
        formatter.Serialize(file, saveData);

        // Close the data stream
        file.Close();

        Debug.Log("Saved");
    }

    public void Load()
    {
        // Check if save game file exists
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            // Create a binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
            Debug.Log("Loaded");

            // Ensure the loaded data arrays match the total levels
            ValidateSaveData();
        }
        else
        {
            Debug.LogWarning("No save file found. Initializing new SaveData.");
            saveData = new SaveData(totalLevels);
        }
    }
    private void OnApplicationQuit()
    {
        Save();
    }
    private void OnDisable()
    {
        Save();
    }

    // Ensures saveData is initialized or validated properly
    private void LoadOrInitializeSaveData()
    {
        Load();

        // If saveData is null or arrays are not properly initialized, create new SaveData
        if (saveData == null || saveData.isActive == null || saveData.highScores == null || saveData.stars == null)
        {
            Debug.LogWarning("SaveData was null or invalid. Initializing new SaveData.");
            saveData = new SaveData(totalLevels);
        }
    }

    // Validates and fixes loaded SaveData to match the total levels
    private void ValidateSaveData()
    {
        if (saveData.isActive.Length != totalLevels)
        {
            Array.Resize(ref saveData.isActive, totalLevels);
            Debug.Log("Resized isActive array to match total levels.");
        }

        if (saveData.highScores.Length != totalLevels)
        {
            Array.Resize(ref saveData.highScores, totalLevels);
            Debug.Log("Resized highScores array to match total levels.");
        }

        if (saveData.stars.Length != totalLevels)
        {
            Array.Resize(ref saveData.stars, totalLevels);
            Debug.Log("Resized stars array to match total levels.");
        }

        // Ensure the first level is active
        if (totalLevels > 0 && !saveData.isActive[0])
        {
            saveData.isActive[0] = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
