using System;
using UnityEngine;
using Unity.Mathematics;
public class WorldController : MonoBehaviour
{
    public CesiumForUnity.CesiumGeoreference georeference;

    public DroneController droneController;
    public double3 currentOriginECEF = new double3(0, 0, 0);
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

    private void FixedUpdate()
    {   
        updateWorldOriginIfNeeded(droneController.latLonAlt.y, droneController.latLonAlt.x, droneController.latLonAlt.z);
    }
}