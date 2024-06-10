using System;
using UnityEngine;
using Unity.Mathematics;

public class JSBSimDroneController : MonoBehaviour
{
    public CesiumForUnity.CesiumGeoreference georeference;

    private bool dynamicCameraController = false;
    public JSBUDPReceiver jsbUDPReceiver;
    public GameObject drone;

    public int systemId = 1;
    public Vector3 latLonAlt = new Vector3(0, 0, 0);

    public float alpha = 0.98f;
    public float positionAlpha = 0.98f;
    private string aircraftType = "NONE";
    private Vector3 nedPos = new Vector3(0, 0, 0);

    private Quaternion lastOrientation = Quaternion.identity;

    private double3 currentOriginECEF = new double3(0, 0, 0);


    public void setUpJSBReceiver(JSBUDPReceiver rec, int connectionPort)
    {
        jsbUDPReceiver = rec;
        jsbUDPReceiver.port = connectionPort;
        jsbUDPReceiver.SetupConnection();

    }

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
   public void UpdateAircraftType(string type)
    {

        // Show the right mesh
        Transform planeTransform = drone.transform.Find("plane");
        Transform copterTransform = drone.transform.Find("copter");

        if (planeTransform == null || copterTransform == null)
        {
            Debug.LogError("One of the required GameObjects (plane or copter) is missing in the children of " + drone.name);
            return;
        }

        if (type == "MAV_TYPE_FIXED_WING")
        {
            planeTransform.gameObject.SetActive(true);
            copterTransform.gameObject.SetActive(false);
        }
        else if (type == "MAV_TYPE_QUADROTOR")
        {
            copterTransform.gameObject.SetActive(true);
            planeTransform.gameObject.SetActive(false);
        }
    }


     private void FixedUpdate()
    {
        // Retrieve the FGNetFDM packet
        FGNetFDM aircraftState = jsbUDPReceiver.AircraftState;

        UpdatePosition(aircraftState);
        UpdateOrientation(aircraftState);

        if (dynamicCameraController)
        {
            // Update the georeference's origin if the drone has moved significantly
            updateWorldOriginIfNeeded(aircraftState.longitude * Mathf.Rad2Deg, aircraftState.latitude * Mathf.Rad2Deg, aircraftState.altitude);
        }
    }

    private void UpdatePosition(FGNetFDM aircraftState)
    {
        // Convert velocities from m/s to Unity units per second and integrate to get position
        Vector3 velocityChange = new Vector3(
            aircraftState.v_east,
            -aircraftState.v_down,
            aircraftState.v_north
        ) * Time.fixedDeltaTime;

        Vector3 integratedPosition = drone.transform.position + velocityChange;

        // Update NED position
        Vector3 nedPoslla = new Vector3(
            (float)aircraftState.longitude * Mathf.Rad2Deg,
            (float)aircraftState.latitude * Mathf.Rad2Deg,
            (float)aircraftState.altitude
        );

        nedPos = ConvertGeoToUnityCoordinates(nedPoslla.x, nedPoslla.y, nedPoslla.z);

        // Apply the complementary filter for position
        Vector3 filteredPosition = Vector3.Lerp(integratedPosition, nedPos, 1 - positionAlpha);

        // Update the GameObject's position
        drone.transform.position = filteredPosition;
    }

    private void UpdateOrientation(FGNetFDM aircraftState)
    {
        // Convert angular velocities from rad/s to degrees/s
        float rollDegreesPerSecond = aircraftState.phidot * Mathf.Rad2Deg;
        float pitchDegreesPerSecond = aircraftState.thetadot * Mathf.Rad2Deg;
        float yawDegreesPerSecond = aircraftState.psidot * Mathf.Rad2Deg;

        // Calculate change in orientation based on angular velocity
        Quaternion gyroDeltaRotation = Quaternion.Euler(
            -pitchDegreesPerSecond * Time.fixedDeltaTime,
            yawDegreesPerSecond * Time.fixedDeltaTime,
            -rollDegreesPerSecond * Time.fixedDeltaTime
        );

        // Apply the gyro-based orientation change to the last known orientation
        Quaternion gyroBasedOrientation = lastOrientation * gyroDeltaRotation;

        // Convert the estimated orientation from Euler angles to a Quaternion
        Quaternion sensorBasedOrientation = Quaternion.Euler(
            -aircraftState.theta * Mathf.Rad2Deg,
            aircraftState.psi * Mathf.Rad2Deg,
            -aircraftState.phi * Mathf.Rad2Deg
        );

        // Apply the complementary filter
        drone.transform.rotation = Quaternion.Slerp(gyroBasedOrientation, sensorBasedOrientation, 1 - alpha);

        // Store the last known orientation
        lastOrientation = drone.transform.rotation;
    }

}