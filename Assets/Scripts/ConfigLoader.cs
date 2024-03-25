using System.IO;
using UnityEngine;

public class Config
{
    public string tileURL;
    public string mavlink2RestURL = "ws://127.0.0.1:6040";
}

public class ConfigLoader : MonoBehaviour
{
    public CesiumForUnity.Cesium3DTileset tileset;
    public MavlinkWS mavlinkWS;
    private void Start()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        // Path to the config file within the Assets folder.
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");

        if (File.Exists(filePath))
        {
            // Read the JSON from the file.
            string jsonContents = File.ReadAllText(filePath);
            
            // Deserialize the JSON into the Config object.
            Config config = JsonUtility.FromJson<Config>(jsonContents);

            // Apply the loaded settings.
            ApplySettings(config);
        }
        else
        {
            Debug.LogError("Cannot find config file.");
        }
    }

    private void ApplySettings(Config config)
    {
        tileset.url = config.tileURL;
        mavlinkWS.Connect(config.mavlink2RestURL);
    }
}
