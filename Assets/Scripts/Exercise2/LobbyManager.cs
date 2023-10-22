using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Exercise2.Chat;
using Exercise2.UI;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Exercise2
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TMP_InputField chatInputField;
        [SerializeField] private GameObject chat;
        [SerializeField] private GameObject waitForHost;
        [SerializeField] private Transform chatBox;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private AudioSource messageAudio;
        public List<Message> chatMessages = new();

        private void Awake()
        {
            playerNameText.text = NetworkData.NetworkSocket.Name;
            
            chat.SetActive(false);
            waitForHost.SetActive(true);
            
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

        public void StartClientChat()
        {
            chat.SetActive(true);
            waitForHost.SetActive(false);
        }
        
        public void UpdateMessageBox()
        {
            foreach(var msg in chatMessages)
            {
                var message = Instantiate(messagePrefab, chatBox);
                message.GetComponent<ChatMessageUI>().SetMessages(msg);
                messageAudio.Play();
            }
            chatMessages.Clear();
        }

        public void SendMessage()
        {
            var msg = new Message(NetworkData.NetworkSocket.Name, chatInputField.text, DateTime.Now.ToString("MM-dd hh:mm"));
            chatInputField.text = "";
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

                foreach (var client in ((ServerNetworkSocket)NetworkData.NetworkSocket).ConnectedClients)
                {
                    NetworkData.NetworkSocket.Socket.SendTo(message, SocketFlags.None, client.EndPoint);
                }
                
                return 0;
            }
            else
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

        private void OnDestroy()
        {
            NetworkData.CleanUp();
        }
    }
}