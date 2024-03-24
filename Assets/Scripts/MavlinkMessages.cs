using System;
using UnityEngine;


public class MavlinkMessages : MonoBehaviour
{
    [Serializable]
    public class Header
    {
        public int component_id;
        public int sequence;
        public int system_id;
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

    // HEARTBEAT
    [Serializable]
    public class Heartbeat
    {
        public Header header;
        public HeartbeatMessage message;
    }

    [Serializable]
    public class HeartbeatMessage
    {
        public Autopilot autopilot;
        public BaseMode base_mode;
        public int custom_mode;
        public int mavlink_version;
        public Mavtype mavtype;
        public SystemStatus system_status;
        public string type;
    }

    [Serializable]
    public class Autopilot
    {
        public string type;
    }

    [Serializable]
    public class BaseMode
    {
        public int bits;
    }

    [Serializable]
    public class Mavtype
    {
        public string type;
    }

    [Serializable]
    public class SystemStatus
    {
        public string type;
    }

}