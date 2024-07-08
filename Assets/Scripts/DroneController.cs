using System;
using UnityEngine;
using Unity.Mathematics;

public class DroneController : MonoBehaviour
{
    public CesiumForUnity.CesiumGeoreference georeference;
    public MavlinkMessageProcessor mavlinkMessageProcessor;
    public GameObject drone;

    private bool dynamicCameraController = false;

    public int systemId = 1;
    public Vector3 latLonAlt = new Vector3(0, 0, 0);

    public float alpha = 0.98f;
    public float positionAlpha = 0.98f;
    private string aircraftType = "NONE";
    private Vector3 nedPos = new Vector3(0, 0, 0);

    private Quaternion lastOrientation = Quaternion.identity;

    private double3 currentOriginECEF = new double3(0, 0, 0);

    public void setAsDynamicCameraController()
    {
        dynamicCameraController = true;
    }

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

    private void updatellaPos()
    {
        latLonAlt.x = mavlinkMessageProcessor.globalPositionIntArray[systemId].message.lat / 1e7f;
        latLonAlt.y = mavlinkMessageProcessor.globalPositionIntArray[systemId].message.lon / 1e7f;
        latLonAlt.z = mavlinkMessageProcessor.globalPositionIntArray[systemId].message.alt / 1e3f;
    }
    public Vector3 ConvertGeoToUnityCoordinates(double longitude, double latitude, double altitude)
    {
        // Step 1: Convert to ECEF
        double3 longitudeLatitudeHeight = new double3(longitude, latitude, altitude);
        double3 ecef = CesiumForUnity.CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(longitudeLatitudeHeight);

        // subtrace

        // Step 2: Transform ECEF to Unity coordinates
        double3 unityCoords = georeference.TransformEarthCenteredEarthFixedPositionToUnity(ecef);

        // Return as Vector3 for Unity
        return new Vector3((float)unityCoords.x, (float)unityCoords.y, (float)unityCoords.z);
    }
    private void calculateNedPos()
    {
        nedPos = ConvertGeoToUnityCoordinates(latLonAlt.y, latLonAlt.x, latLonAlt.z);
    }

    // TODO: make the dynamic loading of the aircraft mesh better
   private void UpdateAircraftType()
    {
       // Check if mavlinkMessageProcessor or heartbeatArray[systemId] is null
        if (mavlinkMessageProcessor == null || mavlinkMessageProcessor.heartbeatArray[systemId] == null)
        {
            Debug.Log("mavlinkMessageProcessor or heartbeatArray[systemId] is null");
            return;
        }

        // Check if nested properties are null
        if (mavlinkMessageProcessor.heartbeatArray[systemId].message == null || mavlinkMessageProcessor.heartbeatArray[systemId].message.mavtype == null)
        {
            Debug.Log("Message or mavtype is null for systemId: " + systemId);
            return;
        }
        // return if the aircraft type is already set
        if (mavlinkMessageProcessor.heartbeatArray[systemId].message.mavtype.type == aircraftType)
        {
            return;
        }
        aircraftType = mavlinkMessageProcessor.heartbeatArray[systemId].message.mavtype.type;
        Debug.Log("Aircraft type: " + aircraftType);

        // Assuming 'drone' is already assigned via the inspector or elsewhere in your code
        // If 'drone' could be unassigned, add a check here

        // Show the right mesh
        Transform planeTransform = drone.transform.Find("plane");
        Transform copterTransform = drone.transform.Find("copter");

        if (planeTransform == null || copterTransform == null)
        {
            Debug.LogError("One of the required GameObjects (plane or copter) is missing in the children of " + drone.name);
            return;
        }

        if (mavlinkMessageProcessor.heartbeatArray[systemId].message.mavtype.type == "MAV_TYPE_FIXED_WING")
        {
            planeTransform.gameObject.SetActive(true);
            copterTransform.gameObject.SetActive(false);
        }
        else if (mavlinkMessageProcessor.heartbeatArray[systemId].message.mavtype.type == "MAV_TYPE_QUADROTOR")
        {
            copterTransform.gameObject.SetActive(true);
            planeTransform.gameObject.SetActive(false);
        }
    }



    private void FixedUpdate()
    {
        UpdateAircraftType();
        // check that globalPositionIntArray[systemId] is not null and attituteArray[systemId] is not null
        if (mavlinkMessageProcessor.globalPositionIntArray[systemId].message == null || mavlinkMessageProcessor.attitudeArray[systemId] == null)
        {
            Debug.Log("globalPositionIntArray[systemId] or attituteArray[systemId] is null");
            return;
        }

        updatellaPos();
        calculateNedPos();

        // Integrate velocity to update position
        // Vector3 velocityChange = new Vector3(localPosNed.message.vy, -localPosNed.message.vz, localPosNed.message.vx) * Time.fixedDeltaTime;
        Vector3 velocityChange = new Vector3(mavlinkMessageProcessor.globalPositionIntArray[systemId].message.vy / 100f, -mavlinkMessageProcessor.globalPositionIntArray[systemId].message.vz / 100f, mavlinkMessageProcessor.globalPositionIntArray[systemId].message.vx / 100f) * Time.fixedDeltaTime;
        Vector3 integratedPosition = drone.transform.position + velocityChange;

        // Retrieve the estimated position (assuming it's more accurate but updates less frequently)
        Vector3 sensorEstimatedPosition = new Vector3(nedPos.x, nedPos.y, nedPos.z);

        // Apply the complementary filter for position
        Vector3 filteredPosition = Vector3.Lerp(integratedPosition, sensorEstimatedPosition, 1 - positionAlpha);

        // Update the GameObject's position
        drone.transform.position = filteredPosition;

        // Convert angular velocity from radians per second to degrees per second
        float rollDegreesPerSecond = mavlinkMessageProcessor.attitudeArray[systemId].message.rollspeed * Mathf.Rad2Deg;
        float pitchDegreesPerSecond = mavlinkMessageProcessor.attitudeArray[systemId].message.pitchspeed * Mathf.Rad2Deg;
        float yawDegreesPerSecond = mavlinkMessageProcessor.attitudeArray[systemId].message.yawspeed * Mathf.Rad2Deg;

        // Calculate change in orientation based on angular velocity
        Quaternion gyroDeltaRotation = Quaternion.Euler(-pitchDegreesPerSecond * Time.fixedDeltaTime, yawDegreesPerSecond * Time.fixedDeltaTime, -rollDegreesPerSecond * Time.fixedDeltaTime);

        // Apply the gyro-based orientation change to the last known orientation
        Quaternion gyroBasedOrientation = lastOrientation * gyroDeltaRotation;

        // Convert the estimated orientation from Euler angles to a Quaternion
        Quaternion sensorBasedOrientation = Quaternion.Euler(-mavlinkMessageProcessor.attitudeArray[systemId].message.pitch * Mathf.Rad2Deg, mavlinkMessageProcessor.attitudeArray[systemId].message.yaw * Mathf.Rad2Deg, -mavlinkMessageProcessor.attitudeArray[systemId].message.roll * Mathf.Rad2Deg);

        // Apply the complementary filter
        drone.transform.rotation = Quaternion.Slerp(gyroBasedOrientation, sensorBasedOrientation, 1 - alpha);

        // Store the last known orientation
        lastOrientation = drone.transform.rotation;

        if (dynamicCameraController)
        {
            // Update the georeference's origin if the drone has moved significantly
            updateWorldOriginIfNeeded(latLonAlt.y, latLonAlt.x, latLonAlt.z);
        }
    }

}