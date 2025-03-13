using System.Collections.Generic;
using UnityEngine;

public class LandmarkDataLoader
{
    private static LandmarkDataLoader _instance;
    private Dictionary<string, LandmarkEntry> landmarkDict;

    // Public static property for singleton access
    public static LandmarkDataLoader Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = new LandmarkDataLoader();
            }
            return _instance;
        }
    }

    // Constructor loads data immediately
    public LandmarkDataLoader()
    {
        LoadLandmarkData();
    }

    private void LoadLandmarkData()
    {
        // Load 'landmark_data.json' from Resources
        TextAsset jsonFile = Resources.Load<TextAsset>("landmark_data");
        if (jsonFile == null)
        {
            Debug.LogError("LandmarkDataLoader: landmark_data.json not found in Resources!");
            return;
        }

        Debug.Log("JSON content: " + jsonFile.text);

        // Parse the JSON
        LandmarkData data = JsonUtility.FromJson<LandmarkData>(jsonFile.text);
        if (data == null || data.landmarks == null)
        {
            Debug.LogError("LandmarkDataLoader: Failed to parse JSON data!");
            return;
        }

        landmarkDict = new Dictionary<string, LandmarkEntry>();

        // Populate the dictionary
        foreach (var entry in data.landmarks)
        {
            Debug.Log("Adding landmark: " + entry.id + " - " + entry.name);
            landmarkDict[entry.id] = entry;
        }

        Debug.Log("LandmarkDataLoader: Loaded " + landmarkDict.Count + " landmarks from JSON.");
    }

    public LandmarkEntry GetLandmark(string id)
    {
        if (landmarkDict == null)
        {
            Debug.LogError("LandmarkDataLoader: Data not loaded or landmarkDict is null!");
            return null;
        }

        if (landmarkDict.ContainsKey(id))
        {
            return landmarkDict[id];
        }
        else
        {
            Debug.LogWarning("LandmarkDataLoader: No landmark found for id: " + id);
            return null;
        }
    }
}
