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
    public float[] position; // Assuming these could be Vector3, but represented as arrays in JSON
    public float[] orientation; // Same assumption as above
    public string encoding;
    public string destination;
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
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");
        if (File.Exists(filePath))
        {
            string jsonContents = File.ReadAllText(filePath);
            Config config = JsonUtility.FromJson<Config>(jsonContents);
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

        Debug.Log("vehicle 1 " + config.vehicles[0].id);
        // Example of how to apply vehicle and camera settings.
        // You will need to adapt this to your specific needs, such as instantiating vehicles or configuring cameras.
        foreach (var vehicle in config.vehicles)
        {
            Debug.Log($"Vehicle ID: {vehicle.id}");
            foreach (var camera in vehicle.cameras)
            {
                // Here you would configure each camera.
                // This could involve setting camera positions, orientations, and streaming destinations.
                Debug.Log($"Camera destination: {camera.destination}");
                // Example: Find your camera object in the scene and apply settings.
                // var myCamera = FindCameraInScene(camera.id); // You need to implement FindCameraInScene.
                // myCamera.SetPosition(camera.position);
                // myCamera.SetOrientation(camera.orientation);
                // myCamera.SetDestination(camera.destination);
            }
        }
    }
}
