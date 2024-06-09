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
    public bool jsbsim = false;
    public string type = "NONE"; 
    public int port = 12345;
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

    public WorldController worldController;

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


//     {
//     "tileURL": "https://tile.googleapis.com/v1/3dtiles/root.json?key=AIzaSyCLQrJ7iJvSyCcs5MCQwINWxLrtu9tlnfA",
//     "mavlink2RestURL" : "wss://sim.intelligentquads.com/60e8797ec8e541c2b50b191c4fda7d9c",
//     "vehicles" : [
//         {
//             "jsbsim" : false
//             "id" : 1,
//             "cameras" : [
//                 {
//                     "id": 1,
//                     "position" : [3, 0, 0],
//                     "orientation" : [0, 0, 0],
//                     "vFOV" : 26,
//                     "streamingEnabled" : "false",
//                     "encoding" : "H264Nvidia",
//                     "destination" : "udp://127.0.0.1:5600"
//                 }
//             ]
//         }
//     ]

// }
    private void ApplySettings(Config config)
    {
        // Apply settings to tileset and mavlinkWS as before.
        tileset.url = config.tileURL;
        mavlinkWS.Connect(config.mavlink2RestURL);

        for (int i = 0; i < config.vehicles.Length; i++)
        {
            VehicleConfig vehicleConfig = config.vehicles[i];
            if (vehicleConfig.jsbsim)
            {
                Debug.Log("JSBSim vehicle detected. config: " + JsonUtility.ToJson(vehicleConfig, true));
                worldController.SpawnDrone(vehicleConfig.id, vehicleConfig.type, vehicleConfig.port);
            }
            


        }   
    }
}