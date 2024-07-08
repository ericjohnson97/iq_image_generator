using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

public class TCPReceiver : MonoBehaviour
{
    private TcpListener tcpListener;
    private int port = 12345;  // Adjust the port as needed

    void Start()
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), null);
    }

    void AcceptCallback(IAsyncResult ar)
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(ar);
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[Marshal.SizeOf(typeof(FGNetFDM))];
        stream.Read(buffer, 0, buffer.Length);
        FGNetFDM fdm1 = ByteArrayToStructure<FGNetFDM>(buffer);
        fdm1.SwapEndian();
        Debug.Log($"Received: version = {fdm1.version}, latitude = {fdm1.latitude}");

        stream.Close();
        client.Close();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), null);
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
        tcpListener.Stop();
    }
}
