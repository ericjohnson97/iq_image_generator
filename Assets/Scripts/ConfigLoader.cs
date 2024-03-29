using System.IO;
using UnityEngine;

[System.Serializable]
public class Config
{
    public string tileURL;
    public string mavlink2RestURL;
    public VehicleConfig[] vehicles;
}

[System.Serializable]
public class VehicleConfig
{
    public int id;
    public CameraConfig[] cameras;
}

[System.Serializable]
public class CameraConfig
{
    public int id;
    public float[] position; 
    public float[] orientation;
    public float vFOV;
    public bool streamingEnabled;
    public string encoding;
    public string destination;
}

public class ConfigLoader : MonoBehaviour
{
    public CesiumForUnity.Cesium3DTileset tileset;
    public MavlinkWS mavlinkWS;

    public Config config;

    private void Start()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");
        if (File.Exists(filePath))
        {
            string jsonContents = File.ReadAllText(filePath);
            config = JsonUtility.FromJson<Config>(jsonContents);
            Debug.Log($"Deserialized config: {JsonUtility.ToJson(config, true)}");

            ApplySettings(config);
        }
        else
        {
            Debug.LogError("Cannot find config file.");
        }
    }

    private void ApplySettings(Config config)
    {
        // Apply settings to tileset and mavlinkWS as before.
        tileset.url = config.tileURL;
        mavlinkWS.Connect(config.mavlink2RestURL);
    }
}
