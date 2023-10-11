using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using TMPro;

namespace Exercise2
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TMP_InputField chatInputField;
        [SerializeField] private TextMeshProUGUI chatBox;
        public List<string> chatMessages = new();

        private void Awake()
        {
            playerNameText.text = NetworkData.NetworkSocket.Name;

            switch (NetworkData.NetworkSocket.ConnectionType)
            {
                case ConnectionType.Client:
                    gameObject.AddComponent<ChatClient>();
                    break;
                case ConnectionType.Host:
                    gameObject.AddComponent<ChatServer>();
                    break;
            }
        }

        public void UpdateMessageBox()
        {
            chatBox.text = "";
            foreach (var message in chatMessages)
            {
                chatBox.text += $"{message}\n";
            }
        }

        public void SendMessage()
        {
            string message = $"{DateTime.Now.TimeOfDay} [{NetworkData.NetworkSocket.Name}]:{chatInputField.text}";
            byte[] data = Encoding.ASCII.GetBytes(message);
            int rBytes = NetworkData.ProtocolType == ProtocolType.Tcp
                ? SendMessageTCP(data)
                : SendMessageUDP(data);
        }

        private int SendMessageUDP(byte[] message)
        {
            switch (NetworkData.NetworkSocket.ConnectionType)
            {
                case ConnectionType.Host:
                {
                    chatMessages.Add(Encoding.ASCII.GetString(message));
                    UpdateMessageBox();
                    foreach (var client in ((ServerNetworkSocket)NetworkData.NetworkSocket).ConnectedClients)
                    {
                        client.Socket.SendTo(message, SocketFlags.None, NetworkData.EndPoint);
                    }
                    return 0;
                }
                case ConnectionType.Client:
                    return NetworkData.NetworkSocket.Socket.SendTo(message, SocketFlags.None, NetworkData.EndPoint);
            }

            return 0;
        }

        private int SendMessageTCP(byte[] message)
        {
            switch (NetworkData.NetworkSocket.ConnectionType)
            {
                case ConnectionType.Host:
                {
                    chatMessages.Add(Encoding.ASCII.GetString(message));
                    UpdateMessageBox();
                    foreach (var client in ((ServerNetworkSocket)NetworkData.NetworkSocket).ConnectedClients)
                    {
                        client.Socket.Send(message);
                    }
                    return 0;
                }
                case ConnectionType.Client:
                    return NetworkData.NetworkSocket.Socket.Send(message);
            }

            return 0;
        }
    }
}