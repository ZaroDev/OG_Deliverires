using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Exercise2.Chat
{
    public class ChatServer : MonoBehaviour
    {
        private readonly List<Thread> _messageThread = new();
        private readonly object _clientMutex = new();
        private bool _requestUpdateMessages = false;
        private List<Message> _chatMessages;
        private LobbyManager _lobbyManager;

        private void Awake()
        {
            _lobbyManager = GetComponent<LobbyManager>();
            _chatMessages = _lobbyManager.chatMessages;
            _lobbyManager.StartClientChat();
            
            foreach (var client in ((ServerNetworkSocket)NetworkData.NetworkSocket).ConnectedClients)
            {
                var clientThread = new Thread(() =>
                {
                    var data = Encoding.ASCII.GetBytes("Connected");
                    if (NetworkData.ProtocolType == ProtocolType.Tcp)
                    {
                        client.Socket.Send(data);
                    }
                    else
                    {
                        NetworkData.NetworkSocket.Socket.SendTo(data, SocketFlags.None, client.EndPoint);
                    }
                    MessageJob(client);
                });
                _messageThread.Add(clientThread);
                clientThread.Start();
            }
        }

        private void Update()
        {
            if (_requestUpdateMessages)
            {
                _lobbyManager.UpdateMessageBox();
                lock (_clientMutex)
                    _requestUpdateMessages = false;
            }
        }

        private void MessageJob(NetworkSocket socket)
        {
            while (true)
            {
                var rBytes = NetworkData.ProtocolType == ProtocolType.Tcp
                    ? ReceiveMessagesTCP(socket)
                    : ReceiveMessagesUDP();

                lock (_clientMutex)
                    _requestUpdateMessages = true;

                if (rBytes != 0)
                    continue;
                
                // Handle client disconnection
                lock (_clientMutex)
                {
                    var msg = $"Disconnected client [{socket.Name}] from IP [{socket.IPAddrStr}]";
                    Debug.Log(msg);
                    lock (_clientMutex)
                    {
                        _chatMessages.Add(new Message(null, msg, null));
                    }

                    ((ServerNetworkSocket)NetworkData.NetworkSocket).ConnectedClients.Remove(socket);
                }

                break;
            }
        }

        private int ReceiveMessagesTCP(NetworkSocket socket)
        {
            var data = new byte[2048];
            var rBytes = socket.Socket.Receive(data);
            if (rBytes == 0)
                return rBytes;

            string message = Encoding.ASCII.GetString(data, 0, rBytes);
            Debug.Log($"Server received message: {message}");

            // Add the message and replicate to all clients
            lock (_clientMutex)
            {
                _chatMessages.Add(JsonUtility.FromJson<Message>(message));

                foreach (var client in ((ServerNetworkSocket)NetworkData.NetworkSocket).ConnectedClients)
                {
                    client.Socket.Send(data);
                }
            }

            return rBytes;
        }

        private int ReceiveMessagesUDP()
        {
            // Reset the end point
            NetworkData.EndPoint = new IPEndPoint(IPAddress.Any, NetworkData.Port);
            
            var data = new byte[2048];
            var rBytes = NetworkData.NetworkSocket.Socket.ReceiveFrom(data, SocketFlags.None, ref NetworkData.EndPoint);

            var message = Encoding.ASCII.GetString(data, 0, rBytes);
            Debug.Log($"Server received message: {message}");

            // Add the message and replicate to all clients
            lock (_clientMutex)
            {
                _chatMessages.Add(JsonUtility.FromJson<Message>(message));

                foreach (var client in ((ServerNetworkSocket)NetworkData.NetworkSocket).ConnectedClients)
                {
                    NetworkData.NetworkSocket.Socket.SendTo(data, client.EndPoint);
                }
            }
            return rBytes;
        }

        private void OnDestroy()
        {
            foreach (var thread in _messageThread)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }
        }
    }
}