using System;
using UnityEngine;


public class MavlinkMessages : MonoBehaviour
{
    [Serializable]
    public class Header
    {
        public int componentId;
        public int sequence;
        public int systemId;
    }

    //  LOCAL_POSITION_NED
    [Serializable]
    public class LocalPositionNed
    {
        public Header header;
        public LocalPositionMessage message;
    }

    [Serializable]
    public class LocalPositionMessage
    {
        public int timeBootMs;
        public float vx;
        public float vy;
        public float vz;
        public float x;
        public float y;
        public float z;
    }

    // ATTITUDE
    [Serializable]
    public class Attitude
    {
        public Header header;
        public AttitudeMessage message;
    }

    [Serializable]
    public class AttitudeMessage
    {
        public int timeBootMs;
        public float roll;
        public float pitch;
        public float yaw;
        public float rollspeed;
        public float pitchspeed;
        public float yawspeed;
    }

    

    [Serializable]
    public class GlobalPositionInt
    {
        public Header header;
        public GlobalPositionIntMessage message;
    }

    [Serializable]
    public class GlobalPositionIntMessage
    {
        public int time_boot_ms;
        public int lat;
        public int lon;
        public int alt;
        public int relative_alt;
        public int vx;
        public int vy;
        public int vz;
        public int hdg;
    }

}