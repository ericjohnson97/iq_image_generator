using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
public class WorldController : MonoBehaviour
{
    public CesiumForUnity.CesiumGeoreference georeference;

    public DroneController droneController;

    public GameObject droneTemplate;
    
    public MavlinkMessageProcessor mavlinkMessageProcessor;
    public double3 currentOriginECEF = new double3(0, 0, 0);

    public CameraController cameraController;
    public CameraListController cameraListController;

    public ConfigLoader configLoader;

    private void updateWorldOriginIfNeeded(double newLongitude, double newLatitude, double newAltitude)
    {
        double3 newPositionECEF = CesiumForUnity.CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(newLongitude, newLatitude, newAltitude));
        double distance = math.distance(currentOriginECEF, newPositionECEF);

        if (distance > 100000)
        {
            georeference.SetOriginLongitudeLatitudeHeight(newLongitude, newLatitude, newAltitude);
            currentOriginECEF = newPositionECEF;
            Debug.Log("World origin updated to drone's current location.");
        }
    }

    public void SpawnDrone(MavlinkMessages.Heartbeat heartbeat)
    {
        Debug.Log($"Spawned a new drone ");
        // Optionally use systemId to customize the drone or its initialization
        if (droneTemplate != null)
        {
            if (heartbeat.message.mavtype.type != "MAV_TYPE_FIXED_WING" && heartbeat.message.mavtype.type != "MAV_TYPE_QUADROTOR")
            {
                Debug.Log("Unsupported drone type. " + heartbeat.message.mavtype.type);
                return;
            }
            
            // Instantiate the drone at the position and rotation of the georeference
            GameObject newDrone = Instantiate(droneTemplate, georeference.transform.position, Quaternion.identity, georeference.transform); // Parent set here

            // Rename the drone object
            newDrone.name = $"Drone_{heartbeat.header.system_id}";

            newDrone.GetComponent<DroneController>().systemId = heartbeat.header.system_id;
            newDrone.GetComponent<DroneController>().georeference = georeference;
            newDrone.GetComponent<DroneController>().mavlinkMessageProcessor = mavlinkMessageProcessor;
            newDrone.GetComponent<DroneController>().drone = newDrone;
            newDrone.GetComponent<DroneController>().enabled = true;
            newDrone.SetActive(true);

            VehicleConfig vehicleConfig = findVehicleConfig(heartbeat.header.system_id);
            if (vehicleConfig != null)
            {
                setUpCameras(vehicleConfig, newDrone);
            }
            else
            {
                Debug.LogError("Vehicle config not found for system ID: " + heartbeat.header.system_id);
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

            if (droneController == null)
            {
                droneController = newDrone.GetComponent<DroneController>();
                // hack since cesium expects exactly one camera name Dynamic camera to be present
                // find dynamic camera GameObject
                GameObject dynamicCamera = GameObject.Find("DynamicCamera");

                if (dynamicCamera == null)
                {
                    Debug.LogError("DynamicCamera GameObject not found in the scene.");
                }else{
                    // Set the DynamicCamera GameObject as a child of the newDrone GameObject.
                    // This operation modifies the DynamicCamera's transform so that its position, rotation, and scale are now relative to the newDrone.
                    dynamicCamera.transform.SetParent(newDrone.transform, false);

                }

                
            }

            Debug.Log($"Spawned a new drone for system ID: {heartbeat.header.system_id}.");
        }
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



    private void FixedUpdate()
    {   
        if (droneController != null)
        {
            updateWorldOriginIfNeeded(droneController.latLonAlt.y, droneController.latLonAlt.x, droneController.latLonAlt.z);
        }
    }
}