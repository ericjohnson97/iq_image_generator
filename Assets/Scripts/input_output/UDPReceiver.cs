using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;
// using Utilities;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    public int port = 12345;  // Adjust the port as needed

    void Start()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
        byte[] bytes = udpClient.EndReceive(ar, ref ip);
        FGNetFDM fdm1 = ByteArrayToStructure<FGNetFDM>(bytes);
        fdm1.SwapEndian();
        Debug.Log($"Received: version = {fdm1.version}, latitude = {fdm1.latitude}");

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
