using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

public class JSBUDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    public int port = 12345;  // Adjust the port as needed

    public FGNetFDM AircraftState = new FGNetFDM();
    private FGNetFDM TempAircraftState = new FGNetFDM();

    public void SetupConnection()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
        byte[] bytes = udpClient.EndReceive(ar, ref ip);

        try
        {

            // Parse the byte array
            TempAircraftState.ParseByteArray(bytes);

            // Swap endianess
            TempAircraftState.SwapEndian();

            // Validate TempAircraftState
            bool isValid = ValidateTempAircraftState(TempAircraftState, out string errorMessage);

            if (!isValid)
            {
                Debug.LogError(errorMessage);
                return;
            }

            // Assign to AircraftState if valid
            AircraftState = TempAircraftState;

            // Convert latitude and longitude to degrees
            float latitudeInDegrees = (float)(AircraftState.latitude * Mathf.Rad2Deg);
            float longitudeInDegrees = (float)(AircraftState.longitude * Mathf.Rad2Deg);

            // Log the final values
            // Debug.Log($"Received: version = {AircraftState.version}, latitude = {latitudeInDegrees}, longitude = {longitudeInDegrees}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to convert data to FGNetFDM: {ex.Message}");
            Debug.LogError($"Data received: {BitConverter.ToString(bytes)}");
        }

        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    private bool ValidateTempAircraftState(FGNetFDM tempAircraftState, out string errorMessage)
    {
        bool isValid = true;
        errorMessage = "TempAircraftState validation failed: ";

        // Validate individual fields
        if (float.IsNaN((float)tempAircraftState.latitude) || float.IsInfinity((float)tempAircraftState.latitude))
        {
            isValid = false;
            errorMessage += "Invalid latitude; ";
        }
        if (float.IsNaN((float)tempAircraftState.longitude) || float.IsInfinity((float)tempAircraftState.longitude))
        {
            isValid = false;
            errorMessage += "Invalid longitude; ";
        }
        if (float.IsNaN((float)tempAircraftState.version) || float.IsInfinity((float)tempAircraftState.version))
        {
            isValid = false;
            errorMessage += "Invalid version; ";
        }

        // Add more validation checks as needed

        return isValid;
    }

    private void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
