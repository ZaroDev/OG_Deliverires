using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Exercise2.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exercise2
{
    public class Client : MonoBehaviour
    {
        public string PlayerName { get; set; } = "Player";
        public string IP { get; set; } = "127.0.0.1";
        private Thread _playerThread;

        private bool _connected = false;
        private bool _disconnected = false;
        
        public void ConnectToServer()
        {
            if (IP == "")
            {
                Debug.LogError("Trying to connect to an empty IP address.");
                return;
            }

            bool valid = IPAddress.TryParse(IP, out var ipAddress);

            if (!valid)
            {
                Debug.LogError("Couldn't parse ip address.");
                return;
            }

            // Connect to the server
            NetworkData.EndPoint = new IPEndPoint(ipAddress, NetworkData.Port);
            Socket clientSocket = new Socket(ipAddress.AddressFamily,
                NetworkData.ProtocolType == ProtocolType.Tcp ? SocketType.Stream : SocketType.Dgram,
                NetworkData.ProtocolType);

            try
            {
                clientSocket.Connect(NetworkData.EndPoint);
                Debug.Log($"Connecting to {clientSocket.RemoteEndPoint}");
                _playerThread = new Thread(SendToServerJob);
                _playerThread.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while attempting to connect to server: {e}");
                return;
            }

            NetworkData.NetworkSocket = new NetworkSocket(PlayerName, clientSocket, ipAddress, IP, NetworkData.Port);
        }

        void SendToServerJob()
        {
            while (true)
            {
                int rBytes = NetworkData.ProtocolType == ProtocolType.Tcp ? SendToServerTCP() : SentToServerUDP();

                // Handle server disconnection
                if (rBytes == 0)
                {
                    Debug.LogError($"Server disconnected returning to main menu");
                    _disconnected = true;
                    break;
                }

                // Handle lobby connection
                Debug.Log("Successfully connected to server traveling to lobby");
                _connected = true;
                break;
            }
        }

        int SendToServerTCP()
        {
            byte[] data = Encoding.ASCII.GetBytes(PlayerName);
            NetworkData.NetworkSocket.Socket.Send(data, data.Length, SocketFlags.None);

            data = new byte[2048];
            int rBytes = NetworkData.NetworkSocket.Socket.Receive(data);
            string serverName = Encoding.ASCII.GetString(data, 0, rBytes);
            Debug.Log($"Connected to server {serverName}");

            return rBytes;
        }

        int SentToServerUDP()
        {
            byte[] data = Encoding.ASCII.GetBytes(PlayerName);
            NetworkData.NetworkSocket.Socket.SendTo(data, data.Length, SocketFlags.None, NetworkData.EndPoint);

            data = new byte[2048];
            int rBytes = NetworkData.NetworkSocket.Socket.ReceiveFrom(data, ref NetworkData.EndPoint);
            string serverName = Encoding.ASCII.GetString(data, 0, rBytes);
            Debug.Log($"Connected to server {serverName}");

            return rBytes;
        }

        private void Update()
        {
            if (_connected)
            {
                SceneManager.LoadScene("Scenes/Exercise2/Lobby");
            }

            if (_disconnected)
            {
                SceneManager.LoadScene("Scenes/Exercise2/MainMenu");
            }
        }

        private void OnDestroy()
        {
            if (_playerThread is { IsAlive: true })
            {
                _playerThread.Abort();
            }
        }
    }
}