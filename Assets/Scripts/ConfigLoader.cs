using System.IO;
using UnityEngine;

[System.Serializable]
public class Config
{
    public string tileURL;
    public string mavlink2RestURL;
    public VehicleConfig[] vehicles;
    public StaticObject[] staticObjects;
}
[System.Serializable]
public class StaticObject
{
    public string model = "";
    public double[] latlonalt = {0,0,0};
    public bool clipToGround = true;
    public float altOffset = 0; 
}

[System.Serializable]
public class VehicleConfig
{
    public bool jsbsim = false;
    public string type = "NONE"; 
    public int port = 12345;
    public int id;
    public CameraConfig[] cameras;
    public string model = "red drone";
}

[System.Serializable]
public class CameraConfig
{
    public int id;
    public float[] position; 
    public float[] orientation;
    public int[] resolution = {1280, 720};
    public bool isGreyScale = false;
    public float fps = 15;
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

    public void LoadConfig()
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

        for (int i = 0; i < config.vehicles.Length; i++)
        {
            VehicleConfig vehicleConfig = config.vehicles[i];
            if (vehicleConfig.jsbsim)
            {
                Debug.Log("JSBSim vehicle detected. config: " + JsonUtility.ToJson(vehicleConfig, true));
                worldController.SpawnDrone(vehicleConfig.id, vehicleConfig.model, vehicleConfig.port);
            }
            


        }   

        for (int i = 0; i < config.staticObjects.Length; i++)
        {
            StaticObject staticObject = config.staticObjects[i];
            
            worldController.SpawnStaticObject(staticObject.model, staticObject.latlonalt, staticObject.clipToGround, staticObject.altOffset);
            
        }
    }
}