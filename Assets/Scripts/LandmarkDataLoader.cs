using System.Collections.Generic;
using UnityEngine;

public class LandmarkDataLoader : MonoBehaviour
{
    public static LandmarkDataLoader Instance;

    private Dictionary<string, LandmarkEntry> landmarkDict;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadLandmarkData();
    }

    void LoadLandmarkData()
    {
        // load 'landmark_data.json' from Resources (must be in 'Resources' folder)
        TextAsset jsonFile = Resources.Load<TextAsset>("landmark_data");
        if (jsonFile == null)
        {
            Debug.LogError("LandmarkDataLoader: landmark_data.json not found in Resources!");
            return;
        }

        // parse JSON
        LandmarkData data = JsonUtility.FromJson<LandmarkData>(jsonFile.text);
        landmarkDict = new Dictionary<string, LandmarkEntry>();

        // add to dictionary 
        foreach (var entry in data.landmarks)
        {
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