using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exercise2
{
    public class ChatClient : MonoBehaviour
    {
        private Thread _messageThread;
        private List<Message> _chatMessages;
        private readonly object _mutex = new();
        private bool _requestUpdateMessages = false;
        private bool _disconnection = false;
        private LobbyManager _lobbyManager;
        private void Awake()
        {
            _lobbyManager = GetComponent<LobbyManager>();
            _chatMessages = _lobbyManager.chatMessages;

            _messageThread = new Thread(ReceiveMessageJob);
            _messageThread.Start();
        }

        private void Update()
        {
            if (_requestUpdateMessages)
            {
                _lobbyManager.UpdateMessageBox();
                lock(_mutex)
                    _requestUpdateMessages = false;
            }

            if (_disconnection)
            {
                SceneManager.LoadScene("Scenes/Exercise2/MainMenu");
            }
        }

        private void ReceiveMessageJob()
        {
            while (true)
            {
                int rBytes = NetworkData.ProtocolType == ProtocolType.Tcp ? ReceiveMessagesTCP() : ReceiveMessagesUDP();
                
                lock (_mutex)
                    _requestUpdateMessages = true;
                
                
                // Handle client disconnection
                if (rBytes == 0)
                {
                    lock (_mutex)
                    {
                        string msg = $"Disconnected from server";
                        Debug.Log(msg);
                        lock (_mutex)
                        {
                            _chatMessages.Add(new Message(null, msg, null));
                            _disconnection = true;
                        }
                    }
                    break;
                }
            }
        }

        private int ReceiveMessagesTCP()
        {
            byte[] data = new byte[2048];
            int rBytes = NetworkData.NetworkSocket.Socket.Receive(data);
            if (rBytes == 0)
                return rBytes;

            string message = Encoding.ASCII.GetString(data, 0, rBytes);
            Debug.Log($"Client received message: {message}");
            
            lock (_mutex)
            {
                _chatMessages.Add(JsonUtility.FromJson<Message>(message));
            }
            return rBytes;
        }

        private int ReceiveMessagesUDP()
        {
            byte[] data = new byte[2048];
            int rBytes = NetworkData.NetworkSocket.Socket.ReceiveFrom(data, SocketFlags.None, ref NetworkData.EndPoint);
            if (rBytes == 0)
                return rBytes;

            string message = Encoding.ASCII.GetString(data, 0, rBytes);
            Debug.Log($"Client received message: {message}");
            
            lock (_mutex)
            {
                _chatMessages.Add(JsonUtility.FromJson<Message>(message));
            }
            
            return rBytes;
        }

        private void OnDestroy()
        {
            if (_messageThread is { IsAlive: true })
            {
                _messageThread.Abort();
            }
        }
    }
}
