using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LandmarkDataLoader : MonoBehaviour
{
    public static LandmarkDataLoader Instance;

    private string url = "https://digitalworlds.github.io/CURE25_Test/models/Callithrix/Callithrix.json";
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

        StartCoroutine(DownloadJson());
    }

    IEnumerator DownloadJson()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonText = request.downloadHandler.text;
                Debug.Log("JSON Loaded: " + jsonText);

                // parse JSON
                LandmarkData data = JsonUtility.FromJson<LandmarkData>(jsonText);
                landmarkDict = new Dictionary<string, LandmarkEntry>();

                // add to dictionary 
                foreach (var entry in data.landmarks)
                {
                    landmarkDict[entry.id] = entry;
                }

                Debug.Log("LandmarkDataLoader: Loaded " + landmarkDict.Count + " landmarks from JSON.");
            }
            else
            {
                Debug.LogError("Failed to load JSON: " + request.error);
            }
        }
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