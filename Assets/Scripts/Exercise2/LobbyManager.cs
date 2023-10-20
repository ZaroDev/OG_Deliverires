using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Exercise2
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TMP_InputField chatInputField;
        [SerializeField] private VerticalLayoutGroup chatBox;
        [SerializeField] private GameObject messagePrefab;
        public List<Message> chatMessages = new();

        private List<Message> _postedMessages = new();

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
            Debug.Log("Updated message box");
            foreach(var msg in chatMessages)
            {
                _postedMessages.Add(msg);
                var message = Instantiate(messagePrefab, chatBox.transform);
                message.GetComponent<ChatMessageUI>().SetMessages(msg);
            }
            chatMessages.Clear();
        }

        public void SendMessage()
        {
            var msg = new Message(NetworkData.NetworkSocket.Name, chatInputField.text, DateTime.Now.ToString());
            string message = JsonUtility.ToJson(msg);
            byte[] data = Encoding.ASCII.GetBytes(message);
            int rBytes = NetworkData.ProtocolType == ProtocolType.Tcp
                ? SendMessageTCP(data)
                : SendMessageUDP(data);
        }

        private int SendMessageUDP(byte[] message)
        {
            if(NetworkData.NetworkSocket.ConnectionType == ConnectionType.Host)
            {
                chatMessages.Add(JsonUtility.FromJson<Message>(Encoding.ASCII.GetString(message)));
                UpdateMessageBox();
            }
            
            return NetworkData.NetworkSocket.Socket.SendTo(message, SocketFlags.None, NetworkData.EndPoint);
        }

        private int SendMessageTCP(byte[] message)
        {
            switch (NetworkData.NetworkSocket.ConnectionType)
            {
                case ConnectionType.Host:
                {
                    chatMessages.Add(JsonUtility.FromJson<Message>(Encoding.ASCII.GetString(message)));
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