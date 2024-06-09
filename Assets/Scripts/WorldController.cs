using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
public class WorldController : MonoBehaviour
{
    public CesiumForUnity.CesiumGeoreference georeference;

    public bool isDynamicCameraSpawned = false;

    public GameObject droneTemplate;
    
    public MavlinkMessageProcessor mavlinkMessageProcessor;
    public double3 currentOriginECEF = new double3(0, 0, 0);

    public CameraController cameraController;
    public CameraListController cameraListController;

    public ConfigLoader configLoader;

    private void Start()
    {
        configLoader.worldController = this;
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
    public void SpawnDrone(int id, string type, int port)
    {
        if (type != "MAV_TYPE_FIXED_WING" && type != "MAV_TYPE_QUADROTOR")
        {
            Debug.Log("Unsupported drone type. " + type);
            return;
        }
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
            droneController.UpdateAircraftType(type);
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
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().enabled = cameraConfig.streamingEnabled;
            camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().streamAddress = cameraConfig.destination;
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