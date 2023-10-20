using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Exercise2
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
            
            foreach (var client in ((ServerNetworkSocket)NetworkData.NetworkSocket).ConnectedClients)
            {
                Thread clientThread = new Thread(() => MessageJob(client));
                _messageThread.Add(clientThread);
                clientThread.Start();
            }
        }

        private void Update()
        {
            if (_requestUpdateMessages)
            {
                _lobbyManager.UpdateMessageBox();
                lock(_clientMutex)
                    _requestUpdateMessages = false;
            }
        }

        private void MessageJob(NetworkSocket socket)
        {
            while (true)
            {
                int rBytes = NetworkData.ProtocolType == ProtocolType.Tcp ? ReceiveMessagesTCP(socket) : ReceiveMessagesUDP();
                
                lock(_clientMutex)
                    _requestUpdateMessages = true;
                
                // Handle client disconnection
                if (rBytes == 0)
                {
                    lock (_clientMutex)
                    {
                        string msg = $"Disconnected client [{socket.Name}] from IP [{socket.IPAddrStr}]";
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
        }

        int ReceiveMessagesTCP(NetworkSocket socket)
        {
            byte[] data = new byte[2048];
            int rBytes = socket.Socket.Receive(data);
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
        
        int ReceiveMessagesUDP()
        {
            byte[] data = new byte[2048];
            int rBytes = NetworkData.NetworkSocket.Socket.ReceiveFrom(data, SocketFlags.None, ref NetworkData.EndPoint);
            
            string message = Encoding.ASCII.GetString(data, 0, rBytes);
            Debug.Log($"Server received message: {message}");
            
            // Add the message and replicate to all clients
            lock (_clientMutex)
            {
                _chatMessages.Add(JsonUtility.FromJson<Message>(message));
            }
            NetworkData.NetworkSocket.Socket.SendTo(data, NetworkData.EndPoint);
            
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
