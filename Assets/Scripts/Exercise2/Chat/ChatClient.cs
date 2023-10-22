using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Exercise2.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exercise2.Chat
{
    public class ChatClient : MonoBehaviour
    {
        private Thread _waitForHostThread;
        private Thread _messageThread;
        private List<Message> _chatMessages;
        private readonly object _mutex = new();
        private bool _requestUpdateMessages = false;
        private bool _disconnection = false;
        private bool _startChat = false;
        private LobbyManager _lobbyManager;
        
        private void Awake()
        {
            _lobbyManager = GetComponent<LobbyManager>();
            _chatMessages = _lobbyManager.chatMessages;

            _waitForHostThread = new Thread(ReceiveHostStart);
            _waitForHostThread.Start();
        }

        private void Update()
        {
            if (_startChat)
            {
                _lobbyManager.StartClientChat();
            }
            
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

        private void ReceiveHostStart()
        {
            // Wait for the host to confirm the start of the game
            {
                var rBytes = ReceiveStart();
                
                if (HandleDisconnection(rBytes))
                    return;

                lock (_mutex)
                {
                    _startChat = true;
                }
            }
            
            ReceiveMessageJob();
        }
        
        private void ReceiveMessageJob()
        {
            while (true)
            {
                var rBytes = NetworkData.ProtocolType == ProtocolType.Tcp ? ReceiveMessagesTCP() : ReceiveMessagesUDP();
                
                lock (_mutex)
                    _requestUpdateMessages = true;
                
                if (HandleDisconnection(rBytes))
                    break;
            }
        }

        private bool HandleDisconnection(int rBytes)
        {
            if (rBytes != 0)
                return false;
            
            lock (_mutex)
            {
                var msg = $"Disconnected from server";
                Debug.Log(msg);
                lock (_mutex)
                {
                    _chatMessages.Add(new Message(null, msg, null));
                    _disconnection = true;
                }
            }

            return true;

        }

        private int ReceiveStart()
        {
            var data = new byte[2048];
            return NetworkData.ProtocolType == ProtocolType.Tcp
                ? NetworkData.NetworkSocket.Socket.Receive(data)
                : NetworkData.NetworkSocket.Socket.ReceiveFrom(data, SocketFlags.None, ref NetworkData.EndPoint);
        }
        
        private int ReceiveMessagesTCP()
        {
            var data = new byte[2048];
            var rBytes = NetworkData.NetworkSocket.Socket.Receive(data);
            if (rBytes == 0)
                return rBytes;

            var message = Encoding.ASCII.GetString(data, 0, rBytes);
            Debug.Log($"Client received message: {message}");
            
            lock (_mutex)
            {
                _chatMessages.Add(JsonUtility.FromJson<Message>(message));
            }
            return rBytes;
        }

        private int ReceiveMessagesUDP()
        {
            var data = new byte[2048];
            var rBytes = NetworkData.NetworkSocket.Socket.ReceiveFrom(data, SocketFlags.None, ref NetworkData.EndPoint);
            if (rBytes == 0)
                return rBytes;

            var message = Encoding.ASCII.GetString(data, 0, rBytes);
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
