using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Exercise2
{
    public enum ConnectionType
    {
        Host,
        Client
    }

    public class NetworkSocket
    {
        public string Name;
        public string IPAddrStr = "127.0.0.1";
        public ConnectionType ConnectionType;
        public IPAddress IPAddress;
        public Socket Socket;

        public NetworkSocket(string name)
        {
            Name = name;
        }

        public NetworkSocket(string name, Socket socket, IPAddress ipAddress, string ipAddressStr)
        {
            Name = name;
            Socket = socket;
            IPAddress = ipAddress;
            IPAddrStr = ipAddressStr;
            ConnectionType = ConnectionType.Client;
        }

        ~NetworkSocket()
        {
            Debug.Log("Closing network socket");
            if (Socket.Connected)
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            Socket.Close();
        }
    }

    public class ServerNetworkSocket : NetworkSocket
    {
        public List<NetworkSocket> ConnectedClients = new();
        public ServerNetworkSocket(string name,Socket socket, IPAddress ipAddress, string ipAddressStr)
            : base(name, socket, ipAddress, ipAddressStr)
        {
            ConnectionType = ConnectionType.Host;
        }

        ~ServerNetworkSocket()
        {
            Debug.Log("Closing server socket");
            if (Socket.Connected)
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            Socket.Close();

            ConnectedClients.Clear();
        }
    }

    public static class NetworkData
    {
        public static NetworkSocket NetworkSocket;
        public static int Port = 6969;
        public static EndPoint EndPoint;
        public static ProtocolType ProtocolType = ProtocolType.Tcp;
       
        public static IPAddress GetIPAddress()
        {
            IPAddress hostIp = IPAddress.Any;
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                // Get the last IP as is always the local IP
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    hostIp = ip;
                }
            }

            return hostIp;
        }
    }
    
}
