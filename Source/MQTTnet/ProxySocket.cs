using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MQTTnet;
public static class ProxySocket
{
    public static Socket ConnectThroughProxy(string proxyHost, int proxyPort,
        string targetHost, int targetPort)
    {
        // Create socket and connect to proxy
        Socket socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint proxyEndPoint = new IPEndPoint(IPAddress.Parse(proxyHost), proxyPort);
        socket.Connect(proxyEndPoint);

        // Send CONNECT request to proxy
        string connectRequest = $"CONNECT {targetHost}:{targetPort} HTTP/1.1\r\n" +
                                $"Host: {targetHost}:{targetPort}\r\n" +
                                $"Proxy-Connection: Keep-Alive\r\n\r\n";

        byte[] requestBytes = Encoding.ASCII.GetBytes(connectRequest);
        socket.Send(requestBytes);

        // Read proxy response
        byte[] buffer = new byte[1024];
        int bytesReceived = socket.Receive(buffer);
        string response = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

        // Check if connection was successful
        if (!response.StartsWith("HTTP/1.1 200") && !response.StartsWith("HTTP/1.0 200"))
        {
            socket.Close();
            throw new Exception($"Proxy connection failed: {response}");
        }

        return socket; // Now you can use this socket to communicate with target host
    }
}

