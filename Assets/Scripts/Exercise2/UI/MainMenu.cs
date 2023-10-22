using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exercise2.UI
{
    public class MainMenu : MonoBehaviour
    {
        private void Awake()
        {
            // Clean up all sockets if any
            NetworkData.CleanUp();
        }

        public void SetProtocol(int value)
        {
            switch (value)
            {
                case 0:
                    NetworkData.ProtocolType = ProtocolType.Tcp;
                    break;
                case 1:
                    NetworkData.ProtocolType = ProtocolType.Udp;
                    break;
            }
        }
        
        public void JoinGame()
        {
            SceneManager.LoadScene("JoinGame");
        }

        public void CreateGame()
        {
            SceneManager.LoadScene("CreateGame");
        }
    }
}
