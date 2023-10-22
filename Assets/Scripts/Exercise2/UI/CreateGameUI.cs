using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exercise2.UI
{
    public class CreateGameUI : MonoBehaviour
    {
        [SerializeField] private GameObject createServerBtn;
        [SerializeField] private GameObject startGameBtn;
        [SerializeField] private Server server;
        private void Awake()
        {
            startGameBtn.SetActive(false);
        }

        public void CreateServer()
        {
            server.CreateServer();
            createServerBtn.SetActive(false);
            startGameBtn.SetActive(true);
        }
        
        public void StartGame()
        {
            SceneManager.LoadScene("Scenes/Exercise2/Lobby");
        }
    }
}
