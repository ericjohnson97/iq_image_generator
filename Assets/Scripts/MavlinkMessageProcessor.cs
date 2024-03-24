using System;
using UnityEngine;

public class MavlinkMessageProcessor : MonoBehaviour
{
    // Arrays to hold the data for each system ID
    public MavlinkMessages.Attitude[] attitudeArray = new MavlinkMessages.Attitude[256];
    public MavlinkMessages.GlobalPositionInt[] globalPositionIntArray = new MavlinkMessages.GlobalPositionInt[256];
    public MavlinkMessages.Heartbeat[] heartbeatArray = new MavlinkMessages.Heartbeat[256];

    public MavlinkMessageProcessor()
    {
        // Initialize arrays
        for (int i = 0; i < 256; i++)
        {
            attitudeArray[i] = new MavlinkMessages.Attitude(); 
            globalPositionIntArray[i] = new MavlinkMessages.GlobalPositionInt(); 
        }
    }

    public void ProcessMessage(string message)
    {
        int systemId = 0; 

        if (message.Contains("ATTITUDE"))
        {
            var newAttitude = JsonUtility.FromJson<MavlinkMessages.Attitude>(message);
            systemId = newAttitude.header.system_id; 
            attitudeArray[systemId] = newAttitude;
        }
        else if (message.Contains("GLOBAL_POSITION_INT"))
        {
            var newGlobalPositionInt = JsonUtility.FromJson<MavlinkMessages.GlobalPositionInt>(message);
            systemId = newGlobalPositionInt.header.system_id; 
            globalPositionIntArray[systemId] = newGlobalPositionInt;
        }
        else if (message.Contains("HEARTBEAT"))
        {
            var newHeartbeat = JsonUtility.FromJson<MavlinkMessages.Heartbeat>(message);
            systemId = newHeartbeat.header.system_id; 
            heartbeatArray[systemId] = newHeartbeat;
            Debug.Log("aircraft type is " + newHeartbeat.message.mavtype.type + " for system id " + systemId);
        }
    }
}
