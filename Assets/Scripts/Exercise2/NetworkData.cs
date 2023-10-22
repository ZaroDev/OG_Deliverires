using System;
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
        public readonly string IPAddrStr = "127.0.0.1";
        public ConnectionType ConnectionType;
        public IPAddress IPAddress;
        public Socket Socket;

        public IPEndPoint EndPoint;

        public NetworkSocket(string name)
        {
            Name = name;
        }

        public NetworkSocket(string name, Socket socket, IPAddress ipAddress, string ipAddressStr, int port)
        {
            Name = name;
            Socket = socket;
            IPAddress = ipAddress;
            IPAddrStr = ipAddressStr;
            ConnectionType = ConnectionType.Client;

            EndPoint = new IPEndPoint(ipAddress, port);
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

        public ServerNetworkSocket(string name, Socket socket, IPAddress ipAddress, string ipAddressStr)
            : base(name, socket, ipAddress, ipAddressStr, NetworkData.Port)
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
        public const int Port = 6969;
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

        public static void CleanUp()
        {
            NetworkSocket = null;
            EndPoint = null;
            ProtocolType = ProtocolType.Tcp;
        }
    }
}