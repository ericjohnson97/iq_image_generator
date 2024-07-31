using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using CesiumForUnity;

public class GroundClipping : MonoBehaviour
{
    public CesiumGeoreference georeference;
    public CesiumGlobeAnchor anchor;
    public float altOffset = 0;
    public float updateInterval = 5f; // Update every 5 seconds

    void Start()
    {
        StartCoroutine(UpdatePositionRoutine());
    }

    private IEnumerator UpdatePositionRoutine()
    {
        while (true)
        {
            // Wait for the specified interval
            yield return new WaitForSeconds(updateInterval);

            // Check if georeference and anchor are set
            if (georeference == null || anchor == null)
            {
                Debug.LogError("Georeference or Anchor not set!");
                continue;
            }

            RaycastHit hit;
            if (Physics.Raycast(this.transform.position + Vector3.up * 10000f, Vector3.down, out hit, Mathf.Infinity))
            {
                double3 ecef = georeference.TransformUnityPositionToEarthCenteredEarthFixed(
                    new double3(this.transform.position.x, hit.point.y + altOffset, this.transform.position.z));
                double3 lonLatHeight = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
                anchor.longitudeLatitudeHeight = lonLatHeight;
            }
        }
    }
}
