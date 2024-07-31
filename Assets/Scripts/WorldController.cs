using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using FFmpegOut.LiveStream;
using FFmpegOut;
using CesiumForUnity;

public class WorldController : MonoBehaviour
{
    public GameObject gameObjectGeoReference;
    public CesiumForUnity.CesiumGeoreference georeference;

    public GameObject defaultModel;

    public bool isDynamicCameraSpawned = false;

    public GameObject droneTemplate;
    
    public MavlinkMessageProcessor mavlinkMessageProcessor;
    public double3 currentOriginECEF = new double3(0, 0, 0);

    public CameraController cameraController;
    public CameraListController cameraListController;

    public ConfigLoader configLoader;
    public AssetBundleLoader assetBundleLoader;

    private void Start()
    {
        configLoader.worldController = this;
    }

    public void SpawnStaticObject(string objectName, double[] latlonalt, bool clipToGround, float altOffset)
    {
        GameObject objInstance = new GameObject();
        if (assetBundleLoader.loadedModels.TryGetValue(objectName, out GameObject prefab))
        {
            // Instantiate the GameObject
            objInstance = Instantiate(prefab);
        }
        else
        {
            Debug.LogWarning($"Object '{objectName}' not found in loaded models. Using default model.");
            if (defaultModel != null)
            {
                objInstance = Instantiate(defaultModel);
                
            }
            else
            {
                Debug.LogError("Default model is not assigned.");
            }
        }
        objInstance.transform.parent = gameObjectGeoReference.transform;

        // Add Cesium Globe Anchor
        CesiumGlobeAnchor anchor = objInstance.AddComponent<CesiumGlobeAnchor>();
        anchor.longitudeLatitudeHeight = new double3(latlonalt[1], latlonalt[0], latlonalt[2]);

        if (clipToGround)
        {
            GroundClipping groundClipping = objInstance.AddComponent<GroundClipping>();
            groundClipping.georeference = georeference;
            groundClipping.anchor = anchor;
            groundClipping.altOffset = altOffset;
        }
    }

// mavlink spawning of drone
    public void SpawnDrone(MavlinkMessages.Heartbeat heartbeat)
    {
        Debug.Log($"Spawned a new drone ");
        if (heartbeat.message.mavtype.type != "MAV_TYPE_FIXED_WING" && heartbeat.message.mavtype.type != "MAV_TYPE_QUADROTOR")
        {
            Debug.Log("Unsupported drone type. " + heartbeat.message.mavtype.type);
            return;
        }
        if (droneTemplate != null)
        {
            // Instantiate the drone at the position and rotation of the georeference
            GameObject newDrone = Instantiate(droneTemplate, georeference.transform.position, Quaternion.identity, georeference.transform); // Parent set here

            // Rename the drone object
            newDrone.name = $"Drone_{heartbeat.header.system_id}";
            var droneController = newDrone.GetComponent<DroneController>();
            droneController.systemId = heartbeat.header.system_id;
            droneController.georeference = georeference;
            droneController.mavlinkMessageProcessor = mavlinkMessageProcessor;
            droneController.drone = newDrone;
            droneController.enabled = true;

            if (heartbeat.header.system_id == 1)
            {
                droneController.setAsDynamicCameraController();
            }

            newDrone.SetActive(true);
            
            // Hack to make sure there is only one Camera Named Dynamic Camera Active
            if (isDynamicCameraSpawned == false)
            {
                isDynamicCameraSpawned = true;
                GameObject dynamicCamera = GameObject.Find("DynamicCamera");

                if (dynamicCamera == null)
                {
                    Debug.LogError("DynamicCamera GameObject not found in the scene.");
                }
                else
                {
                    dynamicCamera.transform.SetParent(newDrone.transform, false);
                }
            }

            CommonSpawnDrone(heartbeat.header.system_id, newDrone);
        }
    }

// JSB spawning code
    public void SpawnDrone(int id, string ModelName, int port)
    {
        if (droneTemplate != null)
        {
            // Instantiate the drone at the position and rotation of the georeference
            GameObject newDrone = Instantiate(droneTemplate, georeference.transform.position, Quaternion.identity, georeference.transform); // Parent set here

            // Rename the drone object
            newDrone.name = $"Drone_{id}";
            var droneController = newDrone.GetComponent<JSBSimDroneController>();
            droneController.systemId = id;
            droneController.georeference = georeference;
            droneController.drone = newDrone;
            droneController.enabled = true;
            droneController.UpdateAircraftType(ModelName);
            Debug.Log("Setting up JSB receiver on port " + port);
            droneController.setUpJSBReceiver( newDrone.GetComponent<JSBUDPReceiver>(), port);
            if (id == 1)
            {
                droneController.setAsDynamicCameraController();
            }

            newDrone.SetActive(true);


            // Hack to make sure there is only one Camera Named Dynamic Camera Active
            if (isDynamicCameraSpawned == false)
            {
                isDynamicCameraSpawned = true;
                GameObject dynamicCamera = GameObject.Find("DynamicCamera");

                if (dynamicCamera == null)
                {
                    Debug.LogError("DynamicCamera GameObject not found in the scene.");
                }
                else
                {
                    dynamicCamera.transform.SetParent(newDrone.transform, false);
                }
            }


            CommonSpawnDrone(id, newDrone);
        }
    }

    private void CommonSpawnDrone(int systemId, GameObject newDrone )
    {
        
            VehicleConfig vehicleConfig = findVehicleConfig(systemId);
            if (vehicleConfig != null)
            {
                setUpCameras(vehicleConfig, newDrone);
            }
            else
            {
                Debug.LogError("Vehicle config not found for system ID: " + systemId);
            }

            Camera FollowCamera = newDrone.transform.Find("FollowCam").GetComponent<Camera>();
            if (FollowCamera != null)
            {
                FollowCamera.enabled = true;
                cameraController.AddCamera(FollowCamera);
            }
            else
            {
                Debug.LogError("FollowCam component not found on the drone.");
            }

            cameraListController.CreateEntry(newDrone);

            Debug.Log($"Spawned a new drone for system ID: {systemId}.");
        
    }


    public VehicleConfig findVehicleConfig(int systemId)
    {
        foreach (var vehicleConfig in configLoader.config.vehicles)
        {
            if (vehicleConfig.id == systemId)
            {
                return vehicleConfig;
            }
        }
        return null;
    }

    public void setUpCameras(VehicleConfig vehicleConfig, GameObject newDrone)
    {
        foreach (var cameraConfig in vehicleConfig.cameras)
        {
            // copy camera game object from DynamicCameraTemplate and rename it
            GameObject camera = Instantiate(newDrone.transform.Find("DynamicCameraTemplate").gameObject, newDrone.transform);
            camera.name = $"CameraID_{cameraConfig.id}";
            camera.SetActive(true);
            camera.transform.position = convertNEDToUnity(cameraConfig.position[0], cameraConfig.position[1], cameraConfig.position[2]);
            // read in euler angle orientation and convert to quaternion
            camera.transform.rotation = convertEulerNEDToUnity(cameraConfig.orientation[0], cameraConfig.orientation[1], cameraConfig.orientation[2]);
            camera.GetComponent<Camera>().fieldOfView = cameraConfig.vFOV;
            
            // streaming setup
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().enabled = false;
            if (Enum.TryParse(cameraConfig.encoding, out FFmpegOut.FFmpegPreset preset))
            {
                camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().preset = preset;
                if( cameraConfig.encoding == "MJPEG")
                {
                    Enum.TryParse("UdpMJPEG", out FFmpegOut.LiveStream.StreamPreset streamPreset);
                    camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>()._streamPreset = streamPreset;
                }
            }
            else
            {
                Debug.LogError("Invalid stream encoding value: " + cameraConfig.encoding);
            }


            
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().streamAddress = cameraConfig.destination;
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().width = cameraConfig.resolution[0];
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().height = cameraConfig.resolution[1];
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().frameRate = cameraConfig.fps;
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().isGreyScale = cameraConfig.isGreyScale;
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().enabled = cameraConfig.streamingEnabled;
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().enabled = true;
        }
    }

    private Vector3 convertNEDToUnity(float x, float y, float z)
    {
        return new Vector3( y, -z, x );
    }

    private Quaternion convertEulerNEDToUnity(float roll, float pitch, float yaw)
    {
        return Quaternion.Euler(-pitch, yaw, -roll);
    }



    // private void FixedUpdate()
    // {   
    //     if (droneController != null)
    //     {
    //         updateWorldOriginIfNeeded(droneController.latLonAlt.y, droneController.latLonAlt.x, droneController.latLonAlt.z);
    //     }
    // }
}