using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Mathematics;
// using Utilities;

public class JSBUDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    public int port = 12345;  // Adjust the port as needed

    public FGNetFDM AircraftState = new FGNetFDM();

    public void SetupConnection()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
        byte[] bytes = udpClient.EndReceive(ar, ref ip);
        AircraftState = ByteArrayToStructure<FGNetFDM>(bytes);
        AircraftState.SwapEndian();
        Debug.Log($"Received: version = {AircraftState.version}, latitude = {AircraftState.latitude * Mathf.Rad2Deg} longitude = {AircraftState.longitude * Mathf.Rad2Deg}");

        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }
    }

    private void OnDestroy()
    {
        udpClient.Close();
    }
}
