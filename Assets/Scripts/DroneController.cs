using System;
using UnityEngine;
using Unity.Mathematics;

public class DroneController : MonoBehaviour
{
    public CesiumForUnity.CesiumGeoreference georeference;
    public MavlinkMessageProcessor mavlinkMessageProcessor;
    public GameObject drone;

    public Vector3 nedPos = new Vector3(0, 0, 0);

    public Vector3 latLonAlt = new Vector3(0, 0, 0);

    public float alpha = 0.98f;
    public float positionAlpha = 0.98f;

    private Quaternion lastOrientation = Quaternion.identity;

    private void updatellaPos()
    {
        latLonAlt.x = mavlinkMessageProcessor.globalPositionInt.message.lat / 1e7f;
        latLonAlt.y = mavlinkMessageProcessor.globalPositionInt.message.lon / 1e7f;
        latLonAlt.z = mavlinkMessageProcessor.globalPositionInt.message.alt / 1e3f;
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

    private void FixedUpdate()
    {
        updatellaPos();
        calculateNedPos();

        // Integrate velocity to update position
        // Vector3 velocityChange = new Vector3(localPosNed.message.vy, -localPosNed.message.vz, localPosNed.message.vx) * Time.fixedDeltaTime;
        Vector3 velocityChange = new Vector3(mavlinkMessageProcessor.globalPositionInt.message.vy / 100f, -mavlinkMessageProcessor.globalPositionInt.message.vz / 100f, mavlinkMessageProcessor.globalPositionInt.message.vx / 100f) * Time.fixedDeltaTime;
        Vector3 integratedPosition = drone.transform.position + velocityChange;

        // Retrieve the estimated position (assuming it's more accurate but updates less frequently)
        Vector3 sensorEstimatedPosition = new Vector3(nedPos.x, nedPos.y, nedPos.z);

        // Apply the complementary filter for position
        Vector3 filteredPosition = Vector3.Lerp(integratedPosition, sensorEstimatedPosition, 1 - positionAlpha);

        // Update the GameObject's position
        drone.transform.position = filteredPosition;

        // Convert angular velocity from radians per second to degrees per second
        float rollDegreesPerSecond = mavlinkMessageProcessor.attitude.message.rollspeed * Mathf.Rad2Deg;
        float pitchDegreesPerSecond = mavlinkMessageProcessor.attitude.message.pitchspeed * Mathf.Rad2Deg;
        float yawDegreesPerSecond = mavlinkMessageProcessor.attitude.message.yawspeed * Mathf.Rad2Deg;

        // Calculate change in orientation based on angular velocity
        Quaternion gyroDeltaRotation = Quaternion.Euler(-pitchDegreesPerSecond * Time.fixedDeltaTime, yawDegreesPerSecond * Time.fixedDeltaTime, -rollDegreesPerSecond * Time.fixedDeltaTime);

        // Apply the gyro-based orientation change to the last known orientation
        Quaternion gyroBasedOrientation = lastOrientation * gyroDeltaRotation;

        // Convert the estimated orientation from Euler angles to a Quaternion
        Quaternion sensorBasedOrientation = Quaternion.Euler(-mavlinkMessageProcessor.attitude.message.pitch * Mathf.Rad2Deg, mavlinkMessageProcessor.attitude.message.yaw * Mathf.Rad2Deg, -mavlinkMessageProcessor.attitude.message.roll * Mathf.Rad2Deg);

        // Apply the complementary filter
        drone.transform.rotation = Quaternion.Slerp(gyroBasedOrientation, sensorBasedOrientation, 1 - alpha);

        // Store the last known orientation
        lastOrientation = drone.transform.rotation;
    }

}