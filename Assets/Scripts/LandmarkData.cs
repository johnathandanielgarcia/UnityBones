using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LandmarkEntry
{
    public string id;   // "jaw"
    public string name; // "Jaw"
    public string fact; // "fact about jaw"
}

[System.Serializable]
public class LandmarkData
{
    public List<LandmarkEntry> landmarks;
}