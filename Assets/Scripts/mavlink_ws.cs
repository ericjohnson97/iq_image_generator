using System;
using UnityEngine;
using NativeWebSocket;
using Unity.Mathematics;

/// <summary>
/// Handles Mavlink messages over WebSocket, specifically for receiving and processing LOCAL_POSITION_NED and ATTITUDE messages.
/// Integrates received data to update a drone's position and attitude in the scene.
/// </summary>
public class MavlinkWS : MonoBehaviour
{

    public MavlinkMessageProcessor mavlinkMessageProcessor;
    public string connectionUrl;

    // WebSocket connection
    private WebSocket websocket;



    /// <summary>
    /// Initiates connection to the server at the specified URL, requesting both LOCAL_POSITION_NED and ATTITUDE messages.
    /// </summary>
    /// <param name="url">The URL to connect to, excluding protocol and endpoint specifics.</param>
    public async void Connect(string url)
    {
        Debug.Log($"Connecting to {url}");
        websocket = new WebSocket($"ws://{url}/ws/mavlink?filter=LOCAL_POSITION_NED|ATTITUDE|GLOBAL_POSITION_INT");

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
        mavlinkMessageProcessor.ProcessesMessage(message);
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
