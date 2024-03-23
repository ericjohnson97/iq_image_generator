using System;
using UnityEngine;
using NativeWebSocket;

/// <summary>
/// Handles Mavlink messages over WebSocket, specifically for receiving and processing LOCAL_POSITION_NED and ATTITUDE messages.
/// Integrates received data to update a drone's position and attitude in the scene.
/// </summary>
public class MavlinkWS : MonoBehaviour
{
    [SerializeField]
    private GameObject drone;
    [SerializeField]
    private string connectionUrl;

    // WebSocket connection
    private WebSocket websocket;

    // Latest received messages
    private LocalPositionNed localPosNed = new LocalPositionNed();
    private Attitude attitude = new Attitude();

    // Filter parameters
    [SerializeField]
    private float alpha = 0.98f; // Closer to 1.0 favors gyroscope, closer to 0.0 favors accelerometer/magnetometer
    [SerializeField]
    private float positionAlpha = 0.95f; // Closer to 1.0 favors integrated position, closer to 0.0 favors estimated position

    // State variables
    private Quaternion lastOrientation = Quaternion.identity;
    private Vector3 lastEstimatedPosition = Vector3.zero;

    [Serializable]
    public class Header
    {
        public int componentId;
        public int sequence;
        public int systemId;
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
    public class LocalPositionNed
    {
        public Header header;
        public LocalPositionMessage message;
    }

    [Serializable]
    public class Attitude
    {
        public Header header;
        public AttitudeMessage message;
    }

    /// <summary>
    /// Initiates connection to the server at the specified URL, requesting both LOCAL_POSITION_NED and ATTITUDE messages.
    /// </summary>
    /// <param name="url">The URL to connect to, excluding protocol and endpoint specifics.</param>
    public async void Connect(string url)
    {
        Debug.Log($"Connecting to {url}");
        websocket = new WebSocket($"ws://{url}/ws/mavlink?filter=LOCAL_POSITION_NED|ATTITUDE");

        websocket.OnOpen += () => Debug.Log("Connection open!");
        websocket.OnError += (e) => Debug.Log($"Error! {e}");
        websocket.OnClose += (e) => Debug.Log("Connection closed!");
        websocket.OnMessage += ProcessMessage;

        await websocket.Connect();
    }

    /// <summary>
    /// Processes incoming WebSocket messages, updating the local position or attitude accordingly.
    /// </summary>
    /// <param name="bytes">The message payload as a byte array.</param>
    private void ProcessMessage(byte[] bytes)
    {
        var message = System.Text.Encoding.UTF8.GetString(bytes);
        if (message.Contains("LOCAL_POSITION_NED"))
        {
            localPosNed = JsonUtility.FromJson<LocalPositionNed>(message);
        }
        else if (message.Contains("ATTITUDE"))
        {
            attitude = JsonUtility.FromJson<Attitude>(message);
        }
    }

    private void FixedUpdate()
    {
        // Integrate velocity to update position
        Vector3 velocityChange = new Vector3(localPosNed.message.vy, -localPosNed.message.vz, localPosNed.message.vx) * Time.fixedDeltaTime;
        Vector3 integratedPosition = drone.transform.position + velocityChange;

        // Retrieve the estimated position (assuming it's more accurate but updates less frequently)
        Vector3 sensorEstimatedPosition = new Vector3(localPosNed.message.y, -localPosNed.message.z, localPosNed.message.x);

        // Apply the complementary filter for position
        Vector3 filteredPosition = Vector3.Lerp(integratedPosition, sensorEstimatedPosition, 1 - positionAlpha);

        // Update the GameObject's position
        drone.transform.position = filteredPosition;

        // Convert angular velocity from radians per second to degrees per second
        float rollDegreesPerSecond = attitude.message.rollspeed * Mathf.Rad2Deg;
        float pitchDegreesPerSecond = attitude.message.pitchspeed * Mathf.Rad2Deg;
        float yawDegreesPerSecond = attitude.message.yawspeed * Mathf.Rad2Deg;

        // Calculate change in orientation based on angular velocity
        Quaternion gyroDeltaRotation = Quaternion.Euler(-pitchDegreesPerSecond * Time.fixedDeltaTime, yawDegreesPerSecond * Time.fixedDeltaTime, -rollDegreesPerSecond * Time.fixedDeltaTime);

        // Apply the gyro-based orientation change to the last known orientation
        Quaternion gyroBasedOrientation = lastOrientation * gyroDeltaRotation;

        // Convert the estimated orientation from Euler angles to a Quaternion
        Quaternion sensorBasedOrientation = Quaternion.Euler(-attitude.message.pitch * Mathf.Rad2Deg, attitude.message.yaw * Mathf.Rad2Deg, -attitude.message.roll * Mathf.Rad2Deg);

        // Apply the complementary filter
        drone.transform.rotation = Quaternion.Slerp(gyroBasedOrientation, sensorBasedOrientation, 1 - alpha);

        // Store the last known orientation
        lastOrientation = drone.transform.rotation;
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    /// <summary>
    /// Starts the WebSocket connection and initializes component state.
    /// </summary>
    private void Start()
    {
        Connect(connectionUrl);
    }

    /// <summary>
    /// Handles message dispatch for the WebSocket connection, ensuring messages are processed.
    /// This method is called once per frame.
    /// </summary>
    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }
}
