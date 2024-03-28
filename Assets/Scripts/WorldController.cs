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

            // Camera setup
            Camera camera = newDrone.transform.Find("DynamicCamera").GetComponent<Camera>();
            if (camera != null)
            {
                setUpCamera(camera, heartbeat.header.system_id);
                camera.enabled = true;
            }
            else
            {
                Debug.LogError("DynamicCamera component not found on the drone.");
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

            Debug.Log($"Spawned a new drone for system ID: {heartbeat.header.system_id}.");
            // Additional setup for newDrone as needed...
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

    public void setUpCamera(Camera camera, int systemId)
    {

        // find camera settings
        foreach (var vehicle in configLoader.config.vehicles)
        {
            if (vehicle.id == systemId)
            {
                foreach (var cameraConfig in vehicle.cameras)
                {
                    // TODO fix
                    if (cameraConfig.id == 1)
                    {
                        
                        camera.transform.position = convertNEDToUnity(cameraConfig.position[0], cameraConfig.position[1], cameraConfig.position[2]);
                        // read in euler angle orientation and convert to quaternion
                        camera.transform.rotation = convertEulerNEDToUnity(cameraConfig.orientation[0], cameraConfig.orientation[1], cameraConfig.orientation[2]);
                        camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().streamAddress = cameraConfig.destination;
                        camera.GetComponent<FFmpegOut.LiveStream.StreamCameraCapture>().enabled = true;
                    }
                }
            }
        }
    }


    private void FixedUpdate()
    {   
        updateWorldOriginIfNeeded(droneController.latLonAlt.y, droneController.latLonAlt.x, droneController.latLonAlt.z);
    }
}