using System;
using UnityEngine;


public class MavlinkMessageProcessor : MonoBehaviour
{
    

    public MavlinkMessages.LocalPositionNed localPosNed = new MavlinkMessages.LocalPositionNed();
    public MavlinkMessages.Attitude attitude = new MavlinkMessages.Attitude();
    public MavlinkMessages.GlobalPositionInt globalPositionInt = new MavlinkMessages.GlobalPositionInt();

    public void ProcessesMessage(string message)
    {
        if (message.Contains("LOCAL_POSITION_NED"))
        {
            localPosNed = JsonUtility.FromJson<MavlinkMessages.LocalPositionNed>(message);
        }
        else if (message.Contains("ATTITUDE"))
        {
            attitude = JsonUtility.FromJson<MavlinkMessages.Attitude>(message);
        }
        else if (message.Contains("GLOBAL_POSITION_INT"))
        {
            globalPositionInt = JsonUtility.FromJson<MavlinkMessages.GlobalPositionInt>(message);
            // nedPos = ConvertGeoToUnityCoordinates( globalPositionInt.message.lon / 1e7f, globalPositionInt.message.lat / 1e7f, globalPositionInt.message.alt / 1e3f);
            // var newLongitude = globalPositionInt.message.lon / 1e7;
            // var newLatitude = globalPositionInt.message.lat / 1e7;
            // var newAltitude = globalPositionInt.message.alt / 1e3;

            // // Check and update world origin if needed
            // UpdateWorldOriginIfNeeded(newLongitude, newLatitude, newAltitude);
            // georeference.SetOriginLongitudeLatitudeHeight(globalPositionInt.message.lat / 1e7f, globalPositionInt.message.alt / 1e3f, globalPositionInt.message.lon / 1e7f);
        }
    }
}

